using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
using ProjectFirst.Data;

namespace ProjectFirst.OutGame
{
    /// <summary>
    /// 대화 시스템의 핵심 매니저입니다. 게임에서 대화 그룹을 재생하고 관리하는 핵심 클래스입니다.
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private DialogueTable _dialogueTable;
        [SerializeField] private VideoPlayer _videoPlayer;

        private const float DefaultCharDelay = 0.04f;
        private const float AutoDelayMultiplier = 0.5f;
        private const float AutoAdvanceWait = 1.5f;

        private float _charDelay = DefaultCharDelay;
        private bool _isAutoMode;
        private bool _isSkipRequested;
        private bool _isTyping;
        private Coroutine _typingCoroutine;

        // UI 변수
        private VisualElement _root;
        private VisualElement _bgImage;
        private VisualElement _charLeft;
        private VisualElement _charRight;
        private VisualElement _charCenter;
        private Label _speakerLabel;
        private Label _dialogueText;
        private VisualElement _dialogueBox;
        private VisualElement _narrationBox;
        private Label _narrationText;
        private VisualElement _choicePanel;
        private Button _choiceABtn;
        private Button _choiceBBtn;
        private Button _skipBtn;
        private Button _autoBtn;
        private VisualElement _nextArrow;
        private VisualElement _confirmPopup;
        private Button _confirmYesBtn;
        private Button _confirmNoBtn;

        // 대화 상태
        private string _currentGroupId;
        private int _currentLineIndex;
        private List<DialogueLine> _currentGroup;
        private Action _onGroupComplete;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BindUI();
            RegisterCallbacks();
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            UnregisterCallbacks();
        }

        /// <summary>
        /// 대화 그룹을 실행한다.
        /// </summary>
        public void PlayGroup(string groupId, Action onComplete = null)
        {
            _currentGroup = _dialogueTable.GetGroup(groupId);
            if (_currentGroup.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            _currentGroupId = groupId;
            _currentLineIndex = 0;
            _onGroupComplete = onComplete;
            _isAutoMode = false;
            _isSkipRequested = false;
            UpdateAutoButtonStyle();
            gameObject.SetActive(true);
            ShowLine(_currentGroup[0]);
        }

        /// <summary>
        /// 대화 UI를 숨기고 상태를 초기화한다.
        /// </summary>
        public void Hide()
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
            }
            _isTyping = false;
            gameObject.SetActive(false);
        }

        private void BindUI()
        {
            _root = _uiDocument.rootVisualElement;

            _bgImage = _root.Q<VisualElement>("bg-image");
            _charLeft = _root.Q<VisualElement>("char-left");
            _charRight = _root.Q<VisualElement>("char-right");
            _charCenter = _root.Q<VisualElement>("char-center");
            _speakerLabel = _root.Q<Label>("speaker-name-label");
            _dialogueText = _root.Q<Label>("dialogue-text");
            _dialogueBox = _root.Q<VisualElement>("dialogue-box");
            _narrationBox = _root.Q<VisualElement>("narration-box");
            _narrationText = _root.Q<Label>("narration-text");
            _choicePanel = _root.Q<VisualElement>("choice-panel");
            _choiceABtn = _root.Q<Button>("choice-a-btn");
            _choiceBBtn = _root.Q<Button>("choice-b-btn");
            _skipBtn = _root.Q<Button>("skip-btn");
            _autoBtn = _root.Q<Button>("auto-btn");
            _nextArrow = _root.Q<VisualElement>("next-arrow");
            _confirmPopup = _root.Q<VisualElement>("confirm-popup");
            _confirmYesBtn = _root.Q<Button>("confirm-yes-btn");
            _confirmNoBtn = _root.Q<Button>("confirm-no-btn");
        }

        private void RegisterCallbacks()
        {
            _skipBtn?.RegisterCallback<ClickEvent>(OnSkipClicked);
            _autoBtn?.RegisterCallback<ClickEvent>(OnAutoClicked);
            _nextArrow?.RegisterCallback<ClickEvent>(OnNextClicked);
            _choiceABtn?.RegisterCallback<ClickEvent>(OnChoiceAClicked);
            _choiceBBtn?.RegisterCallback<ClickEvent>(OnChoiceBClicked);
            _confirmYesBtn?.RegisterCallback<ClickEvent>(OnConfirmYes);
            _confirmNoBtn?.RegisterCallback<ClickEvent>(OnConfirmNo);
        }

        private void UnregisterCallbacks()
        {
            _skipBtn?.UnregisterCallback<ClickEvent>(OnSkipClicked);
            _autoBtn?.UnregisterCallback<ClickEvent>(OnAutoClicked);
            _nextArrow?.UnregisterCallback<ClickEvent>(OnNextClicked);
            _choiceABtn?.UnregisterCallback<ClickEvent>(OnChoiceAClicked);
            _choiceBBtn?.UnregisterCallback<ClickEvent>(OnChoiceBClicked);
            _confirmYesBtn?.UnregisterCallback<ClickEvent>(OnConfirmYes);
            _confirmNoBtn?.UnregisterCallback<ClickEvent>(OnConfirmNo);
        }

        private void ShowLine(DialogueLine line)
        {
            _nextArrow.style.display = DisplayStyle.None;

            if (!string.IsNullOrEmpty(line.backgroundKey))
            {
                LoadBackground(line.backgroundKey);
            }

            UpdateCharacterSprites(line.characterLKey, line.characterRKey);

            bool isNarration = string.IsNullOrEmpty(line.speakerName);
            _dialogueBox.style.display = isNarration
                ? DisplayStyle.None : DisplayStyle.Flex;
            _narrationBox.style.display = isNarration
                ? DisplayStyle.Flex : DisplayStyle.None;
            _choicePanel.style.display = DisplayStyle.None;

            if (!isNarration)
            {
                _speakerLabel.text = line.speakerName;
            }

            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);

            Label targetLabel = isNarration ? _narrationText : _dialogueText;
            _typingCoroutine = StartCoroutine(
                TypeText(targetLabel, line.text, () => OnTypingComplete(line)));
        }

        private IEnumerator TypeText(
            Label label, string text, Action onComplete)
        {
            _isTyping = true;
            _isSkipRequested = false;
            label.text = "";

            float delay = _isAutoMode
                ? _charDelay * AutoDelayMultiplier : _charDelay;

            for (int i = 0; i < text.Length; i++)
            {
                if (_isSkipRequested)
                {
                    label.text = text;
                    break;
                }
                label.text += text[i];
                yield return new WaitForSeconds(delay);
            }

            _isTyping = false;
            _isSkipRequested = false;
            _typingCoroutine = null;
            onComplete?.Invoke();
        }

        private void OnTypingComplete(DialogueLine line)
        {
            if (!string.IsNullOrEmpty(line.choiceAText))
            {
                ShowChoices(line);
            }
            else if (_isAutoMode)
            {
                StartCoroutine(AutoAdvance());
            }
            else
            {
                _nextArrow.style.display = DisplayStyle.Flex;
            }
        }

        private void ShowChoices(DialogueLine line)
        {
            _choicePanel.style.display = DisplayStyle.Flex;
            _choiceABtn.text = line.choiceAText;
            _choiceBBtn.text = line.choiceBText;
            _choiceABtn.style.display = DisplayStyle.Flex;
            _choiceBBtn.style.display = string.IsNullOrEmpty(line.choiceBText)
                ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private IEnumerator AutoAdvance()
        {
            yield return new WaitForSeconds(AutoAdvanceWait);
            if (_isAutoMode)
            {
                AdvanceToNext();
            }
        }

        private void AdvanceToNext()
        {
            if (_currentGroup == null || _currentGroup.Count == 0) return;

            DialogueLine current = _currentGroup[_currentLineIndex];

            if (!string.IsNullOrEmpty(current.nextId))
            {
                JumpTo(current.nextId);
                return;
            }

            _currentLineIndex++;
            if (_currentLineIndex < _currentGroup.Count)
            {
                ShowLine(_currentGroup[_currentLineIndex]);
            }
            else
            {
                Hide();
                _onGroupComplete?.Invoke();
            }
        }

        private void JumpTo(string dialogueId)
        {
            DialogueLine line = _dialogueTable.GetById(dialogueId);
            if (line == null)
            {
                Debug.LogWarning(
                    $"[DialogueManager] 해당 ID를 찾을 수 없습니다: {dialogueId}");
                Hide();
                _onGroupComplete?.Invoke();
                return;
            }

            for (int i = 0; i < _currentGroup.Count; i++)
            {
                if (_currentGroup[i].dialogueId == dialogueId)
                {
                    _currentLineIndex = i;
                    ShowLine(_currentGroup[i]);
                    return;
                }
            }

            ShowLine(line);
        }

        private void LoadBackground(string key)
        {
            Sprite bg = Resources.Load<Sprite>($"Backgrounds/{key}");
            if (bg != null)
            {
                _bgImage.style.backgroundImage =
                    new StyleBackground(bg);
            }
        }

        private void UpdateCharacterSprites(
            string leftKey, string rightKey)
        {
            SetCharacterSprite(_charLeft, leftKey);
            SetCharacterSprite(_charRight, rightKey);
            _charCenter.style.display = DisplayStyle.None;
        }

        private void SetCharacterSprite(
            VisualElement element, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                element.style.display = DisplayStyle.None;
                return;
            }

            element.style.display = DisplayStyle.Flex;
            Sprite sprite = Resources.Load<Sprite>(
                $"Characters/{key}");
            if (sprite != null)
            {
                element.style.backgroundImage =
                    new StyleBackground(sprite);
            }
        }

        private void UpdateAutoButtonStyle()
        {
            if (_autoBtn == null) return;

            if (_isAutoMode)
            {
                _autoBtn.AddToClassList("auto-active");
            }
            else
            {
                _autoBtn.RemoveFromClassList("auto-active");
            }
        }

        private void ShowConfirmPopup()
        {
            if (_confirmPopup != null)
            {
                _confirmPopup.style.display = DisplayStyle.Flex;
            }
        }

        private void HideConfirmPopup()
        {
            if (_confirmPopup != null)
            {
                _confirmPopup.style.display = DisplayStyle.None;
            }
        }

        // --- 버튼 콜백 ---

        private void OnSkipClicked(ClickEvent evt)
        {
            if (_isTyping)
            {
                _isSkipRequested = true;
                return;
            }
            ShowConfirmPopup();
        }

        private void OnAutoClicked(ClickEvent evt)
        {
            _isAutoMode = !_isAutoMode;
            UpdateAutoButtonStyle();

            if (_isAutoMode && !_isTyping
                && _choicePanel.style.display == DisplayStyle.None)
            {
                StartCoroutine(AutoAdvance());
            }
        }

        private void OnNextClicked(ClickEvent evt)
        {
            if (_isTyping)
            {
                _isSkipRequested = true;
                return;
            }
            AdvanceToNext();
        }

        private void OnChoiceAClicked(ClickEvent evt)
        {
            DialogueLine current = _currentGroup[_currentLineIndex];
            _choicePanel.style.display = DisplayStyle.None;
            if (!string.IsNullOrEmpty(current.choiceANext))
            {
                JumpTo(current.choiceANext);
            }
            else
            {
                AdvanceToNext();
            }
        }

        private void OnChoiceBClicked(ClickEvent evt)
        {
            DialogueLine current = _currentGroup[_currentLineIndex];
            _choicePanel.style.display = DisplayStyle.None;
            if (!string.IsNullOrEmpty(current.choiceBNext))
            {
                JumpTo(current.choiceBNext);
            }
            else
            {
                AdvanceToNext();
            }
        }

        private void OnConfirmYes(ClickEvent evt)
        {
            HideConfirmPopup();
            Hide();
            _onGroupComplete?.Invoke();
        }

        private void OnConfirmNo(ClickEvent evt)
        {
            HideConfirmPopup();
        }
    }
}
