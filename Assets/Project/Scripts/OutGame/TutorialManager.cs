using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectFirst.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectFirst.OutGame
{
    /// <summary>
    /// ?쒗넗由ъ뼹 ?ㅻ쾭?덉씠瑜??쒖뼱?섎뒗 ?깃???留ㅻ땲?.
    /// TryTrigger(triggerKey)瑜??몄텧?섎㈃ ?대떦 ?ㅼ쓽 ?쒗넗由ъ뼹???쒖옉?⑸땲??
    /// ?꾨즺???쒗넗由ъ뼹? PlayerPrefs??JSON?쇰줈 ??ν븯???ъ텧?μ쓣 諛⑹??⑸땲??
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        // ?? ?깃?????????????????????????????????????????????????
        public static TutorialManager Instance { get; private set; }

        // ?? Inspector ???????????????????????????????????????????
        [SerializeField] private UIDocument _tutorialUI;
        [SerializeField] private PlayerData _playerData;
        [SerializeField] private List<TutorialStep> _allSteps;

        // ?? ?고????????????????????????????????????????????????
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

        // ?????????????????????????????????????????????????????????
        // Lifecycle
        // ?????????????????????????????????????????????????????????

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

        // ?????????????????????????????????????????????????????????
        // Public API
        // ?????????????????????????????????????????????????????????

        /// <summary>
        /// triggerKey???대떦?섎뒗 ?쒗넗由ъ뼹???쒖옉?⑸땲??
        /// ?대? ?꾨즺???ㅼ씠嫄곕굹 ?깅줉?섏? ?딆? ?ㅼ씠硫?臾댁떆?⑸땲??
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

        // ?????????????????????????????????????????????????????????
        // Step 吏꾪뻾
        // ?????????????????????????????????????????????????????????

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

        // ?????????????????????????????????????????????????????????
        // ?ㅻ쾭?덉씠 援ъ텞 (4-?⑤꼸 援щ찉 諛⑹떇)
        // ?????????????????????????????????????????????????????????

        private void BuildOverlay()
        {
            if (_tutorialUI == null) return;

            _overlayRoot = _tutorialUI.rootVisualElement;

            // 湲곗〈 ?먯떇 珥덇린??
            _overlayRoot.Clear();
            _overlayRoot.style.position = Position.Absolute;
            _overlayRoot.style.left = 0;
            _overlayRoot.style.top = 0;
            _overlayRoot.style.right = 0;
            _overlayRoot.style.bottom = 0;
            _overlayRoot.pickingMode = PickingMode.Position;

            // ?대몢???⑤꼸 4媛?(援щ찉???쒗쁽)
            _topPanel = CreateDarkPanel("tutorial-top-panel");
            _bottomPanel = CreateDarkPanel("tutorial-bottom-panel");
            _leftPanel = CreateDarkPanel("tutorial-left-panel");
            _rightPanel = CreateDarkPanel("tutorial-right-panel");

            _overlayRoot.Add(_topPanel);
            _overlayRoot.Add(_bottomPanel);
            _overlayRoot.Add(_leftPanel);
            _overlayRoot.Add(_rightPanel);

            // ?덈궡 ?띿뒪???⑤꼸
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

            // ?앹뾽 ?⑤꼸
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
            _popupCloseBtn.text = "?뺤씤";
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

        // ?????????????????????????????????????????????????????????
        // ?섏씠?쇱씠??
        // ?????????????????????????????????????????????????????????

        /// <summary>
        /// 紐⑤뱺 UIDocument?먯꽌 name?쇰줈 VisualElement瑜??먯깋?섍퀬
        /// worldBound瑜?湲곕컲?쇰줈 4媛??⑤꼸???꾩튂瑜?議곗젙?섏뿬 援щ찉???レ뒿?덈떎.
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
                Debug.LogWarning($"[TutorialManager] UI ?붿냼瑜?李얠쓣 ???놁뒿?덈떎: {elementName}");
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

            // Top panel: ?붾㈃ ?곷떒遺??????곸뿭 ?곷떒源뚯?
            _topPanel.style.left = 0;
            _topPanel.style.top = 0;
            _topPanel.style.width = screenWidth;
            _topPanel.style.height = rect.y;

            // Bottom panel: ????곸뿭 ?섎떒遺???붾㈃ ?섎떒源뚯?
            _bottomPanel.style.left = 0;
            _bottomPanel.style.top = rect.yMax;
            _bottomPanel.style.width = screenWidth;
            _bottomPanel.style.height = screenHeight - rect.yMax;

            // Left panel: ????곸뿭 醫뚯륫 (????믪씠留뚰겮)
            _leftPanel.style.left = 0;
            _leftPanel.style.top = rect.y;
            _leftPanel.style.width = rect.x;
            _leftPanel.style.height = rect.height;

            // Right panel: ????곸뿭 ?곗륫 (????믪씠留뚰겮)
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

        // ?????????????????????????????????????????????????????????
        // ?덈궡 ?띿뒪??
        // ?????????????????????????????????????????????????????????

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

        // ?????????????????????????????????????????????????????????
        // ?앹뾽
        // ?????????????????????????????????????????????????????????

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

            // ?앹뾽???덉쑝硫??ㅻ쾭?덉씠 ?대┃ ????앹뾽???뺤씤 踰꾪듉?쇰줈 吏꾪뻾
            _overlayRoot.UnregisterCallback<ClickEvent>(OnOverlayClicked);
        }

        private void HidePopup()
        {
            if (_popupPanel != null)
                _popupPanel.style.display = DisplayStyle.None;
        }

        // ?????????????????????????????????????????????????????????
        // ?ㅻ쾭?덉씠 ?쒖떆 / ?④?
        // ?????????????????????????????????????????????????????????

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

        // ?????????????????????????????????????????????????????????
        // ???/ 遺덈윭?ㅺ린 (PlayerPrefs + JSON)
        // ?????????????????????????????????????????????????????????

        /// <summary>?쒗넗由ъ뼹 ?뚮옒洹몃? PlayerPrefs??JSON?쇰줈 ??ν빀?덈떎.</summary>
        private void SaveTutorialFlags()
        {
            if (_playerData == null) return;

            var wrapper = new TutorialFlagListWrapper();
            foreach (var kvp in _playerData.TutorialFlags)
                wrapper.entries.Add(new PlayerData.TutorialFlagEntry { key = kvp.Key, done = kvp.Value });

            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
        }

        /// <summary>PlayerPrefs?먯꽌 ?쒗넗由ъ뼹 ?뚮옒洹몃? 遺덈윭? PlayerData???곸슜?⑸땲??</summary>
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
                foreach (PlayerData.TutorialFlagEntry entry in wrapper.entries)
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

        /// <summary>JsonUtility 吏곷젹?붾? ?꾪븳 ?섑띁 ?대옒??</summary>
        [Serializable]
        private class TutorialFlagListWrapper
        {
            public List<PlayerData.TutorialFlagEntry> entries = new List<PlayerData.TutorialFlagEntry>();
        }
    }
}


