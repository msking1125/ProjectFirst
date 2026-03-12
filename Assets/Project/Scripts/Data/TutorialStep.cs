using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// ?쒗넗由ъ뼹 ???④퀎瑜??뺤쓽?섎뒗 ScriptableObject.
    /// ?섏씠?쇱씠?????UI, ?덈궡 ?띿뒪?? ?앹뾽, ?먮룞 吏꾪뻾 ?깆쓣 ?ㅼ젙?⑸땲??
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

        /// <summary>???ㅽ뀦??怨좎쑀 ?앸퀎??</summary>
        public string StepId => _stepId;

        /// <summary>PlayerData.TutorialFlags???ㅼ? ?숈씪???몃━嫄???</summary>
        public string TriggerKey => _triggerKey;

        /// <summary>?섏씠?쇱씠?명븷 UXML ?붿냼??name ?띿꽦媛?</summary>
        public string TargetUIElementName => _targetUIElementName;

        /// <summary>?덈궡 ?띿뒪???⑤꼸???꾩튂 (UpperLeft / MiddleCenter / LowerCenter).</summary>
        public TextAnchor TextPanelPosition => _textPanelPosition;

        /// <summary>?덈궡 ?띿뒪???댁슜.</summary>
        public string GuideText => _guideText;

        /// <summary>?앹뾽 ?대?吏. null?대㈃ ?앹뾽 ?놁씠 ?섏씠?쇱씠?몃쭔 ?쒖떆?⑸땲??</summary>
        public Sprite PopupImage => _popupImage;

        /// <summary>?앹뾽 ?쒕ぉ.</summary>
        public string PopupTitle => _popupTitle;

        /// <summary>?앹뾽 ?ㅻ챸.</summary>
        public string PopupDesc => _popupDesc;

        /// <summary>true硫??대┃ ???ㅼ쓬 ?④퀎, false硫?autoAdvanceDelay ???먮룞 吏꾪뻾.</summary>
        public bool WaitForClick => _waitForClick;

        /// <summary>?먮룞 吏꾪뻾 ?湲??쒓컙(珥?. waitForClick??false?????ъ슜.</summary>
        public float AutoAdvanceDelay => _autoAdvanceDelay;

        /// <summary>?ㅼ쓬 ?ㅽ뀦 ID. 鍮?臾몄옄?댁씠硫??쒗넗由ъ뼹 醫낅즺.</summary>
        public string NextStepId => _nextStepId;
    }
}
