using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using ProjectFirst.Data;
using TMPro;

namespace ProjectFirst.InGame
{
    /// <summary>
    /// 罹먮┃???곸꽭 ?뺣낫 ?댄똻??愿由ы븯??而댄룷?뚰듃
    /// </summary>
    public class CharacterTooltipManager : MonoBehaviour
    {
        [Header("Tooltip Settings")]
        [SerializeField] private float tooltipDelay = 0.5f;
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private Vector2 tooltipOffset = new Vector2(10, 10);
        
        [Header("Tooltip UI References")]
        [SerializeField] private VisualElement tooltipRoot;
        [SerializeField] private Label characterNameLabel;
        [SerializeField] private Label characterLevelLabel;
        [SerializeField] private Label characterPowerLabel;
        [SerializeField] private Label hpLabel;
        [SerializeField] private Label atkLabel;
        [SerializeField] private Label defLabel;
        [SerializeField] private Label critRateLabel;
        [SerializeField] private Label critMultLabel;
        [SerializeField] private VisualElement elementIcon;
        [SerializeField] private VisualElement gradeStarsContainer;
        [SerializeField] private VisualElement skillsContainer;
        
        [Header("Data References")]
        [SerializeField] private AgentTable agentTable;
        [SerializeField] private PlayerData playerData;
        
        private VisualElement _currentTooltipTarget;
        private Coroutine _tooltipCoroutine;
        private bool _isTooltipVisible;
        
        private void Awake()
        {
            // ?댄똻???놁쑝硫?湲곕낯 ?댄똻 ?앹꽦
            if (tooltipRoot == null)
                CreateDefaultTooltip();
        }
        
        private void Start()
        {
            // ?댄똻 珥덇린 ?곹깭 ?ㅼ젙
            if (tooltipRoot != null)
            {
                tooltipRoot.style.display = DisplayStyle.None;
                tooltipRoot.style.opacity = 0f;
            }
        }
        
        /// <summary>
        /// 湲곕낯 ?댄똻 UI ?앹꽦
        /// </summary>
        private void CreateDefaultTooltip()
        {
            // ?댄똻 猷⑦듃 ?앹꽦
            tooltipRoot = new VisualElement();
            tooltipRoot.name = "character-tooltip";
            tooltipRoot.AddToClassList("character-tooltip");
            
            // ?댄똻 諛곌꼍
            var background = new VisualElement();
            background.AddToClassList("tooltip-background");
            tooltipRoot.Add(background);
            
            // 湲곕낯 ?뺣낫 而⑦뀒?대꼫
            var basicInfo = new VisualElement();
            basicInfo.AddToClassList("tooltip-basic-info");
            
            characterNameLabel = new Label("캐릭터 이름");
            characterNameLabel.AddToClassList("tooltip-name");
            
            characterLevelLabel = new Label("Lv.1");
            characterLevelLabel.AddToClassList("tooltip-level");
            
            characterPowerLabel = new Label("CP 0");
            characterPowerLabel.AddToClassList("tooltip-power");
            
            basicInfo.Add(characterNameLabel);
            basicInfo.Add(characterLevelLabel);
            basicInfo.Add(characterPowerLabel);
            
            // 속성 아이콘
            elementIcon = new VisualElement();
            elementIcon.AddToClassList("tooltip-element-icon");
            
            // ?깃툒 蹂?而⑦뀒?대꼫
            gradeStarsContainer = new VisualElement();
            gradeStarsContainer.AddToClassList("tooltip-stars");
            
            // ?ㅽ꺈 而⑦뀒?대꼫
            var statsContainer = new VisualElement();
            statsContainer.AddToClassList("tooltip-stats");
            
            hpLabel = new Label("HP: 0");
            hpLabel.AddToClassList("tooltip-stat");
            
            atkLabel = new Label("ATK: 0");
            atkLabel.AddToClassList("tooltip-stat");
            
            defLabel = new Label("DEF: 0");
            defLabel.AddToClassList("tooltip-stat");
            
            critRateLabel = new Label("치명률: 0%");
            critRateLabel.AddToClassList("tooltip-stat");
            
            critMultLabel = new Label("치명피해: x0.0");
            critMultLabel.AddToClassList("tooltip-stat");
            
            statsContainer.Add(hpLabel);
            statsContainer.Add(atkLabel);
            statsContainer.Add(defLabel);
            statsContainer.Add(critRateLabel);
            statsContainer.Add(critMultLabel);
            
            // ?ㅽ궗 而⑦뀒?대꼫
            skillsContainer = new VisualElement();
            skillsContainer.AddToClassList("tooltip-skills");
            
            // 紐⑤뱺 ?붿냼瑜??댄똻 猷⑦듃??異붽?
            background.Add(basicInfo);
            background.Add(elementIcon);
            background.Add(gradeStarsContainer);
            background.Add(statsContainer);
            background.Add(skillsContainer);
            
            // ?댄똻??罹붾쾭?ㅼ뿉 異붽?
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                var panel = new VisualElement();
                panel.style.position = Position.Absolute;
                panel.style.left = 0;
                panel.style.top = 0;
                panel.style.right = 0;
                panel.style.bottom = 0;
                panel.pickingMode = PickingMode.Ignore;
                
                tooltipRoot.style.position = Position.Absolute;
                panel.Add(tooltipRoot);
                canvas.transform.GetComponent<UIDocument>()?.rootVisualElement.Add(panel);
            }
        }
        
        /// <summary>
        /// 罹먮┃??移대뱶???댄똻 ?대깽???깅줉
        /// </summary>
        public void RegisterCharacterCard(VisualElement card, int agentId)
        {
            if (card == null) return;
            
            // 留덉슦??吏꾩엯
            card.RegisterCallback<PointerEnterEvent>(evt =>
            {
                _currentTooltipTarget = card;
                if (_tooltipCoroutine != null)
                    StopCoroutine(_tooltipCoroutine);
                _tooltipCoroutine = StartCoroutine(ShowTooltipDelayed(agentId, evt.position));
            });
            
            // 留덉슦???대룞
            card.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (_isTooltipVisible && tooltipRoot != null)
                {
                    UpdateTooltipPosition(evt.position);
                }
            });
            
            // 留덉슦???댄깉
            card.RegisterCallback<PointerLeaveEvent>(evt =>
            {
                HideTooltip();
            });
        }
        
        /// <summary>
        /// 吏?곕맂 ?댄똻 ?쒖떆 肄붾（??        /// </summary>
        private IEnumerator ShowTooltipDelayed(int agentId, Vector2 position)
        {
            yield return new WaitForSeconds(tooltipDelay);
            
            if (_currentTooltipTarget != null)
            {
                ShowTooltip(agentId, position);
            }
        }
        
        /// <summary>
        /// ?댄똻 ?쒖떆
        /// </summary>
        private void ShowTooltip(int agentId, Vector2 position)
        {
            if (tooltipRoot == null || agentTable == null || playerData == null) return;
            
            var agentInfo = agentTable.GetAgentInfo(agentId);
            if (agentInfo == null) return;
            
            int level = GetCharacterLevel(agentId);
            
            // ?댄똻 ?곗씠???낅뜲?댄듃
            UpdateTooltipData(agentInfo, level);
            
            // ?꾩튂 ?ㅼ젙
            UpdateTooltipPosition(position);
            
            // ?댄똻 ?쒖떆 ?좊땲硫붿씠??            StartCoroutine(ShowTooltipAnimation());
        }
        
        /// <summary>
        /// ?댄똻 ?곗씠???낅뜲?댄듃
        /// </summary>
        private void UpdateTooltipData(AgentInfo agentInfo, int level)
        {
            if (characterNameLabel != null)
                characterNameLabel.text = agentInfo.agentName;
            
            if (characterLevelLabel != null)
                characterLevelLabel.text = $"Lv.{level}";
            
            if (characterPowerLabel != null)
                characterPowerLabel.text = $"CP {agentInfo.GetPower(level):N0}";
            
            if (hpLabel != null)
                hpLabel.text = $"HP: {agentInfo.GetHp(level):F0}";
            
            if (atkLabel != null)
                atkLabel.text = $"ATK: {agentInfo.GetAtk(level):F0}";
            
            if (defLabel != null)
                defLabel.text = $"DEF: {agentInfo.GetDef(level):F0}";
            
            if (critRateLabel != null)
                critRateLabel.text = $"치명률: {agentInfo.critRate * 100f:F1}%";
            
            if (critMultLabel != null)
                critMultLabel.text = $"치명피해: x{agentInfo.critMult:F2}";
            
            // ?띿꽦 ?꾩씠肄??낅뜲?댄듃
            UpdateElementIcon(agentInfo.element);
            
            // ?깃툒 蹂??낅뜲?댄듃
            UpdateGradeStars(agentInfo.grade);
            
            // ?ㅽ궗 ?뺣낫 ?낅뜲?댄듃
            UpdateSkills(agentInfo);
        }
        
        /// <summary>
        /// ?띿꽦 ?꾩씠肄??낅뜲?댄듃
        /// </summary>
        private void UpdateElementIcon(ElementType element)
        {
            if (elementIcon == null) return;
            
            elementIcon.Clear();
            elementIcon.AddToClassList($"element-{element.ToString().ToLower()}");
            
            var label = new Label(element.ToString());
            label.AddToClassList("element-label");
            elementIcon.Add(label);
        }
        
        /// <summary>
        /// ?깃툒 蹂??낅뜲?댄듃
        /// </summary>
        private void UpdateGradeStars(int grade)
        {
            if (gradeStarsContainer == null) return;
            
            gradeStarsContainer.Clear();
            
            for (int i = 0; i < grade; i++)
            {
                var star = new Label("★");
                star.AddToClassList("grade-star");
                gradeStarsContainer.Add(star);
            }
        }
        
        /// <summary>
        /// ?ㅽ궗 ?뺣낫 ?낅뜲?댄듃
        /// </summary>
        private void UpdateSkills(AgentInfo agentInfo)
        {
            if (skillsContainer == null || agentInfo.skills == null) return;
            
            skillsContainer.Clear();
            
            foreach (var skill in agentInfo.skills)
            {
                if (skill == null) continue;
                
                var skillCard = new VisualElement();
                skillCard.AddToClassList("tooltip-skill-card");
                
                var skillIcon = new VisualElement();
                skillIcon.AddToClassList("skill-icon");
                if (skill.icon != null)
                    skillIcon.style.backgroundImage = new StyleBackground(skill.icon);
                
                var skillName = new Label(skill.name);
                skillName.AddToClassList("skill-name");
                
                var skillDesc = new Label(skill.description);
                skillDesc.AddToClassList("skill-description");
                
                skillCard.Add(skillIcon);
                skillCard.Add(skillName);
                skillCard.Add(skillDesc);
                
                skillsContainer.Add(skillCard);
            }
        }
        
        /// <summary>
        /// ?댄똻 ?꾩튂 ?낅뜲?댄듃
        /// </summary>
        private void UpdateTooltipPosition(Vector2 mousePosition)
        {
            if (tooltipRoot == null) return;
            
            // 留덉슦???꾩튂 湲곗??쇰줈 ?ㅽ봽???곸슜
            Vector2 tooltipPosition = mousePosition + tooltipOffset;
            
            // ?붾㈃ 寃쎄퀎 泥댄겕
            var screenBounds = new Rect(0, 0, Screen.width, Screen.height);
            var tooltipSize = tooltipRoot.layout.size;
            
            // ?ㅻⅨ履?寃쎄퀎 泥댄겕
            if (tooltipPosition.x + tooltipSize.x > screenBounds.width)
                tooltipPosition.x = mousePosition.x - tooltipSize.x - tooltipOffset.x;
            
            // ?꾨옒履?寃쎄퀎 泥댄겕
            if (tooltipPosition.y + tooltipSize.y > screenBounds.height)
                tooltipPosition.y = mousePosition.y - tooltipSize.y - tooltipOffset.y;
            
            tooltipRoot.style.left = tooltipPosition.x;
            tooltipRoot.style.top = tooltipPosition.y;
        }
        
        /// <summary>
        /// ?댄똻 ?쒖떆 ?좊땲硫붿씠??        /// </summary>
        private IEnumerator ShowTooltipAnimation()
        {
            if (tooltipRoot == null) yield break;

            tooltipRoot.style.display = DisplayStyle.Flex;
            tooltipRoot.style.opacity = 0f;

            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                tooltipRoot.style.opacity = Mathf.Clamp01(elapsed / fadeInDuration);
                yield return null;
            }

            tooltipRoot.style.opacity = 1f;
            _isTooltipVisible = true;
        }
        
        /// <summary>
        /// ?댄똻 ?④린湲?        /// </summary>
        public void HideTooltip()
        {
            _currentTooltipTarget = null;
            
            if (_tooltipCoroutine != null)
            {
                StopCoroutine(_tooltipCoroutine);
                _tooltipCoroutine = null;
            }
            
            if (tooltipRoot != null)
            {
                StartCoroutine(HideTooltipAnimation());
            }
        }
        
        /// <summary>
        /// ?댄똻 ?④? ?좊땲硫붿씠??        /// </summary>
        private IEnumerator HideTooltipAnimation()
        {
            if (tooltipRoot == null) yield break;

            float startOpacity = tooltipRoot.resolvedStyle.opacity;
            float elapsed = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeInDuration);
                tooltipRoot.style.opacity = Mathf.Lerp(startOpacity, 0f, t);
                yield return null;
            }

            tooltipRoot.style.opacity = 0f;
            tooltipRoot.style.display = DisplayStyle.None;
            _isTooltipVisible = false;
        }
        
        /// <summary>
        /// 罹먮┃???덈꺼 媛?몄삤湲?        /// </summary>
        private int GetCharacterLevel(int agentId)
        {
            return playerData?.GetCharacterLevel(agentId) ?? 1;
        }
        
        private void OnDestroy()
        {
            if (_tooltipCoroutine != null)
                StopCoroutine(_tooltipCoroutine);
        }
    }
}


