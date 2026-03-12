using UnityEngine;

namespace ProjectFirst.Data
{
    [CreateAssetMenu(fileName = "TutorialStep", menuName = "MindArk/Tutorial/Step")]
    public class TutorialStep : ScriptableObject
    {
        [SerializeField] private string _stepId;
        [SerializeField] private string _triggerKey;

        [Header("Highlight")]
        [SerializeField] private string _targetUIElementName;
        [SerializeField] private TextAnchor _textPanelPosition;
        [SerializeField] [TextArea(2, 5)] private string _guideText;

        [Header("Popup")]
        [SerializeField] private Sprite _popupImage;
        [SerializeField] private string _popupTitle;
        [SerializeField] [TextArea(2, 5)] private string _popupDesc;

        [Header("Auto Advance")]
        [SerializeField] private bool _waitForClick = true;
        [SerializeField] private float _autoAdvanceDelay;
        [SerializeField] private string _nextStepId;
        public string StepId => _stepId;
        public string TriggerKey => _triggerKey;
        public string TargetUIElementName => _targetUIElementName;
        public TextAnchor TextPanelPosition => _textPanelPosition;
        public string GuideText => _guideText;
        public Sprite PopupImage => _popupImage;
        public string PopupTitle => _popupTitle;
        public string PopupDesc => _popupDesc;
        public bool WaitForClick => _waitForClick;
        public float AutoAdvanceDelay => _autoAdvanceDelay;
        public string NextStepId => _nextStepId;
    }
}

