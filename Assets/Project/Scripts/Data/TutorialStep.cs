using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// 튜토리얼 한 단계를 정의하는 ScriptableObject.
    /// 하이라이트 대상 UI, 안내 텍스트, 팝업, 자동 진행 등을 설정합니다.
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

        /// <summary>이 스텝의 고유 식별자.</summary>
        public string StepId => _stepId;

        /// <summary>PlayerData.TutorialFlags의 키와 동일한 트리거 키.</summary>
        public string TriggerKey => _triggerKey;

        /// <summary>하이라이트할 UXML 요소의 name 속성값.</summary>
        public string TargetUIElementName => _targetUIElementName;

        /// <summary>안내 텍스트 패널의 위치 (UpperLeft / MiddleCenter / LowerCenter).</summary>
        public TextAnchor TextPanelPosition => _textPanelPosition;

        /// <summary>안내 텍스트 내용.</summary>
        public string GuideText => _guideText;

        /// <summary>팝업 이미지. null이면 팝업 없이 하이라이트만 표시합니다.</summary>
        public Sprite PopupImage => _popupImage;

        /// <summary>팝업 제목.</summary>
        public string PopupTitle => _popupTitle;

        /// <summary>팝업 설명.</summary>
        public string PopupDesc => _popupDesc;

        /// <summary>true면 클릭 시 다음 단계, false면 autoAdvanceDelay 후 자동 진행.</summary>
        public bool WaitForClick => _waitForClick;

        /// <summary>자동 진행 대기 시간(초). waitForClick이 false일 때 사용.</summary>
        public float AutoAdvanceDelay => _autoAdvanceDelay;

        /// <summary>다음 스텝 ID. 빈 문자열이면 튜토리얼 종료.</summary>
        public string NextStepId => _nextStepId;
    }
}
