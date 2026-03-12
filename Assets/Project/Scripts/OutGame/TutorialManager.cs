using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectFirst.Data;
using UnityEngine;
using UnityEngine.UIElements;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.OutGame
{
    /// <summary>
    /// 튜토리얼 오버레이를 제어하는 싱글턴 매니저.
    /// TryTrigger(triggerKey)를 호출하면 해당 키의 튜토리얼이 시작됩니다.
    /// 완료된 튜토리얼은 PlayerPrefs에 JSON으로 저장하여 재출력을 방지합니다.
    /// </summary>
#if ODIN_INSPECTOR
    [HideMonoScript]
#endif
    public class TutorialManager : MonoBehaviour
    {
        // ── 싱글턴 ──────────────────────────────────────────────
        public static TutorialManager Instance { get; private set; }

        // ── Inspector ───────────────────────────────────────────
#if ODIN_INSPECTOR
        [Title("UI 연결", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("UI")]
        [LabelText("Tutorial UI")]
        [Tooltip("튜토리얼 UI UIDocument")]
        [SceneObjectsOnly]
#endif
        [SerializeField] private UIDocument _tutorialUI;

#if ODIN_INSPECTOR
        [BoxGroup("UI")]
        [LabelText("PlayerData")]
        [Tooltip("플레이어 데이터 (튜토리얼 완료 상태 저장)")]
        [AssetsOnly]
        [PreviewField(50, ObjectFieldAlignment.Left)]
#endif
        [SerializeField] private PlayerData _playerData;

#if ODIN_INSPECTOR
        [Title("단계", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("단계")]
        [LabelText("튜토리얼 단계들")]
        [Tooltip("모든 튜토리얼 단계 정의")]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = false)]
#endif
        [SerializeField] private List<TutorialStep> _allSteps;

        // ── 런타임 ─────────────────────────────────────────────
        private TutorialStep _currentStep;
        private VisualElement _overlayRoot;
        private VisualElement _topPanel;
        private VisualElement _bottomPanel;
        private VisualElement _leftPanel;
        private VisualElement _rightPanel;
        private VisualElement _guideTextPanel;
        private Label _guideLabel;
        private VisualElement _popupPanel;
        private VisualElement _popupImage;
        private Label _popupTitleLabel;
        private Label _popupDescLabel;
        private Button _popupCloseBtn;
        private Coroutine _autoAdvanceCoroutine;

        private const string OverlayColor = "rgba(0,0,0,0.7)";
        private const string PlayerPrefsKey = "tutorialFlags";

        // ─────────────────────────────────────────────────────────
        // Lifecycle
        // ─────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            LoadTutorialFlags();
            BuildOverlay();
            HideOverlay();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        // ─────────────────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────────────────

        /// <summary>
        /// triggerKey에 해당하는 튜토리얼을 시작합니다.
        /// 이미 완료된 키이거나 등록되지 않은 키이면 무시합니다.
        /// </summary>
        public void TryTrigger(string triggerKey)
        {
            if (string.IsNullOrEmpty(triggerKey)) return;
            if (_playerData == null) return;

            if (_playerData.TutorialFlags.TryGetValue(triggerKey, out bool done) && done)
                return;

            TutorialStep step = _allSteps?.FirstOrDefault(s => s.TriggerKey == triggerKey);
            if (step == null) return;

            StartStep(step);
        }

        // ─────────────────────────────────────────────────────────
        // Step 진행
        // ─────────────────────────────────────────────────────────

        private void StartStep(TutorialStep step)
        {
            _currentStep = step;

            StopAutoAdvance();
            ShowOverlay();
            ApplyHighlight(step.TargetUIElementName);
            ShowGuideText(step.GuideText, step.TextPanelPosition);

            if (step.PopupImage != null)
                ShowPopup(step);
            else
                HidePopup();

            if (step.WaitForClick)
                _overlayRoot.RegisterCallback<ClickEvent>(OnOverlayClicked);
            else
                _autoAdvanceCoroutine = StartCoroutine(AutoAdvanceCoroutine(step.AutoAdvanceDelay));
        }

        private void OnOverlayClicked(ClickEvent evt)
        {
            _overlayRoot.UnregisterCallback<ClickEvent>(OnOverlayClicked);
            AdvanceOrComplete();
        }

        private IEnumerator AutoAdvanceCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            AdvanceOrComplete();
        }

        private void AdvanceOrComplete()
        {
            if (_currentStep == null) return;
            CompleteStep(_currentStep);
        }

        private void CompleteStep(TutorialStep step)
        {
            _playerData.TutorialFlags[step.TriggerKey] = true;
            _playerData.SyncTutorialFlagEntries();
            SaveTutorialFlags();
            HideOverlay();

            if (!string.IsNullOrEmpty(step.NextStepId))
            {
                TutorialStep nextStep = _allSteps?.FirstOrDefault(s => s.StepId == step.NextStepId);
                if (nextStep != null)
                {
                    StartStep(nextStep);
                    return;
                }
            }

            _currentStep = null;
        }

        // ─────────────────────────────────────────────────────────
        // 오버레이 구축 (4-패널 구멍 방식)
        // ─────────────────────────────────────────────────────────

        private void BuildOverlay()
        {
            if (_tutorialUI == null) return;

            _overlayRoot = _tutorialUI.rootVisualElement;

            // 기존 자식 초기화
            _overlayRoot.Clear();
            _overlayRoot.style.position = Position.Absolute;
            _overlayRoot.style.left = 0;
            _overlayRoot.style.top = 0;
            _overlayRoot.style.right = 0;
            _overlayRoot.style.bottom = 0;
            _overlayRoot.pickingMode = PickingMode.Position;

            // 어두운 패널 4개 (구멍을 표현)
            _topPanel = CreateDarkPanel("tutorial-top-panel");
            _bottomPanel = CreateDarkPanel("tutorial-bottom-panel");
            _leftPanel = CreateDarkPanel("tutorial-left-panel");
            _rightPanel = CreateDarkPanel("tutorial-right-panel");

            _overlayRoot.Add(_topPanel);
            _overlayRoot.Add(_bottomPanel);
            _overlayRoot.Add(_leftPanel);
            _overlayRoot.Add(_rightPanel);

            // 안내 텍스트 패널
            _guideTextPanel = new VisualElement();
            _guideTextPanel.name = "tutorial-guide-panel";
            _guideTextPanel.style.position = Position.Absolute;
            _guideTextPanel.style.backgroundColor = new Color(0f, 0f, 0f, 0.85f);
            _guideTextPanel.style.paddingTop = 16;
            _guideTextPanel.style.paddingBottom = 16;
            _guideTextPanel.style.paddingLeft = 24;
            _guideTextPanel.style.paddingRight = 24;
            _guideTextPanel.style.borderTopLeftRadius = 8;
            _guideTextPanel.style.borderTopRightRadius = 8;
            _guideTextPanel.style.borderBottomLeftRadius = 8;
            _guideTextPanel.style.borderBottomRightRadius = 8;

            _guideLabel = new Label();
            _guideLabel.name = "tutorial-guide-label";
            _guideLabel.style.color = Color.white;
            _guideLabel.style.fontSize = 28;
            _guideLabel.style.whiteSpace = WhiteSpace.Normal;
            _guideTextPanel.Add(_guideLabel);
            _overlayRoot.Add(_guideTextPanel);

            // 팝업 패널
            _popupPanel = new VisualElement();
            _popupPanel.name = "tutorial-popup-panel";
            _popupPanel.style.position = Position.Absolute;
            _popupPanel.style.alignSelf = Align.Center;
            _popupPanel.style.backgroundColor = new Color(0.12f, 0.12f, 0.15f, 0.95f);
            _popupPanel.style.paddingTop = 24;
            _popupPanel.style.paddingBottom = 24;
            _popupPanel.style.paddingLeft = 32;
            _popupPanel.style.paddingRight = 32;
            _popupPanel.style.borderTopLeftRadius = 12;
            _popupPanel.style.borderTopRightRadius = 12;
            _popupPanel.style.borderBottomLeftRadius = 12;
            _popupPanel.style.borderBottomRightRadius = 12;
            _popupPanel.style.top = Length.Percent(30);
            _popupPanel.style.left = Length.Percent(20);
            _popupPanel.style.right = Length.Percent(20);

            _popupImage = new VisualElement();
            _popupImage.name = "tutorial-popup-image";
            _popupImage.style.width = 200;
            _popupImage.style.height = 200;
            _popupImage.style.alignSelf = Align.Center;
            _popupImage.style.marginBottom = 12;
            _popupPanel.Add(_popupImage);

            _popupTitleLabel = new Label();
            _popupTitleLabel.name = "tutorial-popup-title";
            _popupTitleLabel.style.color = Color.white;
            _popupTitleLabel.style.fontSize = 32;
            _popupTitleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _popupTitleLabel.style.marginBottom = 8;
            _popupPanel.Add(_popupTitleLabel);

            _popupDescLabel = new Label();
            _popupDescLabel.name = "tutorial-popup-desc";
            _popupDescLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
            _popupDescLabel.style.fontSize = 24;
            _popupDescLabel.style.whiteSpace = WhiteSpace.Normal;
            _popupDescLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _popupPanel.Add(_popupDescLabel);

            _popupCloseBtn = new Button();
            _popupCloseBtn.name = "tutorial-popup-close";
            _popupCloseBtn.text = "확인";
            _popupCloseBtn.style.marginTop = 16;
            _popupCloseBtn.style.alignSelf = Align.Center;
            _popupCloseBtn.style.paddingLeft = 40;
            _popupCloseBtn.style.paddingRight = 40;
            _popupCloseBtn.style.paddingTop = 8;
            _popupCloseBtn.style.paddingBottom = 8;
            _popupCloseBtn.RegisterCallback<ClickEvent>(evt =>
            {
                evt.StopPropagation();
                HidePopup();
                AdvanceOrComplete();
            });
            _popupPanel.Add(_popupCloseBtn);

            _overlayRoot.Add(_popupPanel);
            HidePopup();
        }

        private VisualElement CreateDarkPanel(string panelName)
        {
            var panel = new VisualElement();
            panel.name = panelName;
            panel.style.position = Position.Absolute;
            panel.style.backgroundColor = new Color(0f, 0f, 0f, 0.7f);
            return panel;
        }

        // ─────────────────────────────────────────────────────────
        // 하이라이트
        // ─────────────────────────────────────────────────────────

        /// <summary>
        /// 모든 UIDocument에서 name으로 VisualElement를 탐색하고
        /// worldBound를 기반으로 4개 패널의 위치를 조정하여 구멍을 뚫습니다.
        /// </summary>
        private void ApplyHighlight(string elementName)
        {
            if (string.IsNullOrEmpty(elementName))
            {
                SetFullOverlay();
                return;
            }

            VisualElement target = FindUIElement(elementName);
            if (target == null)
            {
                Debug.LogWarning($"[TutorialManager] UI 요소를 찾을 수 없습니다: {elementName}");
                SetFullOverlay();
                return;
            }

            Rect worldRect = target.worldBound;
            SetOverlayRects(worldRect);
        }

        private VisualElement FindUIElement(string elementName)
        {
            UIDocument[] docs = FindObjectsOfType<UIDocument>(true);
            foreach (UIDocument doc in docs)
            {
                if (doc == _tutorialUI) continue;
                VisualElement found = doc.rootVisualElement?.Q(elementName);
                if (found != null) return found;
            }
            return null;
        }

        private void SetOverlayRects(Rect rect)
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // Top panel: 화면 상단부터 대상 영역 상단까지
            _topPanel.style.left = 0;
            _topPanel.style.top = 0;
            _topPanel.style.width = screenWidth;
            _topPanel.style.height = rect.y;

            // Bottom panel: 대상 영역 하단부터 화면 하단까지
            _bottomPanel.style.left = 0;
            _bottomPanel.style.top = rect.yMax;
            _bottomPanel.style.width = screenWidth;
            _bottomPanel.style.height = screenHeight - rect.yMax;

            // Left panel: 대상 영역 좌측 (대상 높이만큼)
            _leftPanel.style.left = 0;
            _leftPanel.style.top = rect.y;
            _leftPanel.style.width = rect.x;
            _leftPanel.style.height = rect.height;

            // Right panel: 대상 영역 우측 (대상 높이만큼)
            _rightPanel.style.left = rect.xMax;
            _rightPanel.style.top = rect.y;
            _rightPanel.style.width = screenWidth - rect.xMax;
            _rightPanel.style.height = rect.height;
        }

        private void SetFullOverlay()
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            _topPanel.style.left = 0;
            _topPanel.style.top = 0;
            _topPanel.style.width = screenWidth;
            _topPanel.style.height = screenHeight;

            _bottomPanel.style.height = 0;
            _leftPanel.style.width = 0;
            _rightPanel.style.width = 0;
        }

        // ─────────────────────────────────────────────────────────
        // 안내 텍스트
        // ─────────────────────────────────────────────────────────

        private void ShowGuideText(string text, TextAnchor position)
        {
            if (_guideTextPanel == null || _guideLabel == null) return;

            _guideLabel.text = text;
            _guideTextPanel.style.display = string.IsNullOrEmpty(text)
                ? DisplayStyle.None
                : DisplayStyle.Flex;

            switch (position)
            {
                case TextAnchor.UpperLeft:
                case TextAnchor.UpperCenter:
                case TextAnchor.UpperRight:
                    _guideTextPanel.style.top = Length.Percent(5);
                    _guideTextPanel.style.bottom = StyleKeyword.Auto;
                    break;
                case TextAnchor.MiddleLeft:
                case TextAnchor.MiddleCenter:
                case TextAnchor.MiddleRight:
                    _guideTextPanel.style.top = Length.Percent(45);
                    _guideTextPanel.style.bottom = StyleKeyword.Auto;
                    break;
                default:
                    _guideTextPanel.style.top = StyleKeyword.Auto;
                    _guideTextPanel.style.bottom = Length.Percent(10);
                    break;
            }

            _guideTextPanel.style.left = Length.Percent(10);
            _guideTextPanel.style.right = Length.Percent(10);
        }

        // ─────────────────────────────────────────────────────────
        // 팝업
        // ─────────────────────────────────────────────────────────

        private void ShowPopup(TutorialStep step)
        {
            if (_popupPanel == null) return;

            if (step.PopupImage != null)
            {
                _popupImage.style.backgroundImage = new StyleBackground(step.PopupImage);
                _popupImage.style.display = DisplayStyle.Flex;
            }
            else
            {
                _popupImage.style.display = DisplayStyle.None;
            }

            _popupTitleLabel.text = step.PopupTitle ?? string.Empty;
            _popupDescLabel.text = step.PopupDesc ?? string.Empty;
            _popupPanel.style.display = DisplayStyle.Flex;

            // 팝업이 있으면 오버레이 클릭 대신 팝업의 확인 버튼으로 진행
            _overlayRoot.UnregisterCallback<ClickEvent>(OnOverlayClicked);
        }

        private void HidePopup()
        {
            if (_popupPanel != null)
                _popupPanel.style.display = DisplayStyle.None;
        }

        // ─────────────────────────────────────────────────────────
        // 오버레이 표시 / 숨김
        // ─────────────────────────────────────────────────────────

        private void ShowOverlay()
        {
            if (_overlayRoot != null)
                _overlayRoot.style.display = DisplayStyle.Flex;
        }

        private void HideOverlay()
        {
            StopAutoAdvance();

            if (_overlayRoot != null)
            {
                _overlayRoot.UnregisterCallback<ClickEvent>(OnOverlayClicked);
                _overlayRoot.style.display = DisplayStyle.None;
            }

            HidePopup();
        }

        private void StopAutoAdvance()
        {
            if (_autoAdvanceCoroutine != null)
            {
                StopCoroutine(_autoAdvanceCoroutine);
                _autoAdvanceCoroutine = null;
            }
        }

        // ─────────────────────────────────────────────────────────
        // 저장 / 불러오기 (PlayerPrefs + JSON)
        // ─────────────────────────────────────────────────────────

        /// <summary>튜토리얼 플래그를 PlayerPrefs에 JSON으로 저장합니다.</summary>
        private void SaveTutorialFlags()
        {
            if (_playerData == null) return;

            var wrapper = new TutorialFlagListWrapper();
            foreach (var kvp in _playerData.TutorialFlags)
                wrapper.entries.Add(new TutorialFlagEntry { key = kvp.Key, done = kvp.Value });

            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
        }

        /// <summary>PlayerPrefs에서 튜토리얼 플래그를 불러와 PlayerData에 적용합니다.</summary>
        public void LoadTutorialFlags()
        {
            if (_playerData == null) return;

            string json = PlayerPrefs.GetString(PlayerPrefsKey, "");
            if (string.IsNullOrEmpty(json))
            {
                _playerData.RebuildTutorialFlags();
                return;
            }

            TutorialFlagListWrapper wrapper = JsonUtility.FromJson<TutorialFlagListWrapper>(json);
            if (wrapper?.entries != null)
            {
                _playerData.TutorialFlags.Clear();
                foreach (TutorialFlagEntry entry in wrapper.entries)
                {
                    if (!string.IsNullOrEmpty(entry.key))
                        _playerData.TutorialFlags[entry.key] = entry.done;
                }
                _playerData.SyncTutorialFlagEntries();
            }
            else
            {
                _playerData.RebuildTutorialFlags();
            }
        }

        /// <summary>JsonUtility 직렬화를 위한 래퍼 클래스.</summary>
        [Serializable]
        private class TutorialFlagListWrapper
        {
            public List<TutorialFlagEntry> entries = new List<TutorialFlagEntry>();
        }
    }
}
