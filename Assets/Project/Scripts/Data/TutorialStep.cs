using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// Documentation cleaned.
    /// Documentation cleaned.
    /// </summary>
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

        /// Documentation cleaned.
        public string StepId => _stepId;

        /// Documentation cleaned.
        public string TriggerKey => _triggerKey;

        /// Documentation cleaned.
        public string TargetUIElementName => _targetUIElementName;

        /// Documentation cleaned.
        public TextAnchor TextPanelPosition => _textPanelPosition;

        /// Documentation cleaned.
        public string GuideText => _guideText;

        /// Documentation cleaned.
        public Sprite PopupImage => _popupImage;

        /// Documentation cleaned.
        public string PopupTitle => _popupTitle;

        /// Documentation cleaned.
        public string PopupDesc => _popupDesc;

        /// Documentation cleaned.
        public bool WaitForClick => _waitForClick;

        /// Documentation cleaned.
        public float AutoAdvanceDelay => _autoAdvanceDelay;

        /// Documentation cleaned.
        public string NextStepId => _nextStepId;
    }
}
