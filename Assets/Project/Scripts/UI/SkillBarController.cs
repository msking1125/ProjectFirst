using ProjectFirst.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project
{
    public class SkillBarController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button slotBtn1;
        [SerializeField] private Button slotBtn2;
        [SerializeField] private Button slotBtn3;

        [Header("Labels")]
        [SerializeField] private TMP_Text slotTxt1;
        [SerializeField] private TMP_Text slotTxt2;
        [SerializeField] private TMP_Text slotTxt3;

        [Header("Icons")]
        [SerializeField] private Image slotIcon1;
        [SerializeField] private Image slotIcon2;
        [SerializeField] private Image slotIcon3;
        [SerializeField] private Color noIconColor = new Color(0f, 0f, 0f, 0.2f);

        [Header("Cooldown")]
        [SerializeField] private TMP_Text cooldownTxt1;
        [SerializeField] private TMP_Text cooldownTxt2;
        [SerializeField] private TMP_Text cooldownTxt3;
        [SerializeField] private Image dimOverlay1;
        [SerializeField] private Image dimOverlay2;
        [SerializeField] private Image dimOverlay3;

        private readonly SkillRow[] slotSkills = new SkillRow[3];
        private SkillSystem skillSystem;

        private void Awake()
        {
            AutoBind();
            EnsureIconComponents();
            EnsureCooldownComponents();
            EnsureDimOverlays();
            EnsureNameTextLayout();
            BindButtons();
            Refresh();
        }

        private void AutoBind()
        {
            string[] slotNames = { "SkillSlot_1", "SkillSlot_2", "SkillSlot_3" };

            for (int i = 0; i < slotNames.Length; i++)
            {
                Transform slot = transform.Find(slotNames[i]);
                if (slot == null)
                {
                    continue;
                }

                if (GetSlotButton(i) == null)
                {
                    Button btn = slot.GetComponent<Button>();
                    switch (i)
                    {
                        case 0: slotBtn1 = btn; break;
                        case 1: slotBtn2 = btn; break;
                        case 2: slotBtn3 = btn; break;
                    }
                }

                if (GetSlotText(i) == null)
                {
                    TMP_Text txt = slot.Find("Name")?.GetComponent<TMP_Text>() ?? slot.GetComponentInChildren<TMP_Text>(true);
                    switch (i)
                    {
                        case 0: slotTxt1 = txt; break;
                        case 1: slotTxt2 = txt; break;
                        case 2: slotTxt3 = txt; break;
                    }
                }

                if (GetSlotIcon(i) == null)
                {
                    Transform iconTransform = slot.Find("Icon");
                    Image icon = iconTransform?.GetComponent<Image>();
                    if (icon != null)
                    {
                        iconTransform.SetSiblingIndex(0);
                        icon.raycastTarget = false;
                    }

                    switch (i)
                    {
                        case 0: slotIcon1 = icon; break;
                        case 1: slotIcon2 = icon; break;
                        case 2: slotIcon3 = icon; break;
                    }
                }

                if (GetCooldownText(i) == null)
                {
                    TMP_Text cooldown = slot.Find("CooldownText")?.GetComponent<TMP_Text>() ?? slot.Find("CoolTime")?.GetComponent<TMP_Text>();
                    switch (i)
                    {
                        case 0: cooldownTxt1 = cooldown; break;
                        case 1: cooldownTxt2 = cooldown; break;
                        case 2: cooldownTxt3 = cooldown; break;
                    }
                }
            }
        }

        private void Update()
        {
            if (skillSystem == null)
            {
                return;
            }

            UpdateCooldownUI(0, slotBtn1, cooldownTxt1);
            UpdateCooldownUI(1, slotBtn2, cooldownTxt2);
            UpdateCooldownUI(2, slotBtn3, cooldownTxt3);
        }

        public void Configure(Button b1, Button b2, Button b3, TMP_Text t1, TMP_Text t2, TMP_Text t3)
        {
            slotBtn1 = b1;
            slotBtn2 = b2;
            slotBtn3 = b3;
            slotTxt1 = t1;
            slotTxt2 = t2;
            slotTxt3 = t3;

            EnsureIconComponents();
            EnsureCooldownComponents();
            EnsureDimOverlays();
            EnsureNameTextLayout();
            BindButtons();
            Refresh();
        }

        public void Setup(SkillSystem system)
        {
            skillSystem = system;
            Refresh();
        }

        public void CastSlot1() => CastSlot(0);
        public void CastSlot2() => CastSlot(1);
        public void CastSlot3() => CastSlot(2);

        private void CastSlot(int index)
        {
            if (skillSystem == null || index < 0 || index >= slotSkills.Length || slotSkills[index] == null)
            {
                return;
            }

            skillSystem.Cast(index);
        }

        private void UpdateCooldownUI(int index, Button btn, TMP_Text cdTxt)
        {
            if (slotSkills[index] == null)
            {
                if (cdTxt != null)
                {
                    cdTxt.gameObject.SetActive(false);
                }

                Image dimWhenEmpty = GetDimOverlay(index);
                if (dimWhenEmpty != null)
                {
                    dimWhenEmpty.gameObject.SetActive(false);
                }

                return;
            }

            bool onCooldown = skillSystem.IsOnCooldown(index);
            float remaining = skillSystem.GetRemainingCooldown(index);

            if (btn != null)
            {
                btn.interactable = !onCooldown;
            }

            Image dim = GetDimOverlay(index);
            if (dim != null)
            {
                dim.gameObject.SetActive(onCooldown);
            }

            if (cdTxt != null)
            {
                cdTxt.gameObject.SetActive(onCooldown);
                if (onCooldown)
                {
                    cdTxt.text = remaining >= 10f ? $"{Mathf.CeilToInt(remaining)}" : $"{remaining:F1}";
                }
            }
        }

        public void SetSlot(int index, SkillRow skill)
        {
            if (index < 0 || index >= slotSkills.Length)
            {
                return;
            }

            slotSkills[index] = skill;
            RefreshSlot(index);
        }

        private void Refresh()
        {
            for (int i = 0; i < slotSkills.Length; i++)
            {
                if (skillSystem != null && i < skillSystem.EquippedSkills.Count)
                {
                    slotSkills[i] = skillSystem.EquippedSkills[i];
                }

                RefreshSlot(i);
            }
        }

        private void RefreshSlot(int index)
        {
            SkillRow skill = slotSkills[index];
            Button btn = GetSlotButton(index);
            TMP_Text txt = GetSlotText(index);
            Image icon = GetSlotIcon(index);

            if (txt != null)
            {
                txt.text = skill != null ? skill.name : $"EMPTY {index + 1}";
            }

            if (icon != null)
            {
                if (skill?.icon != null)
                {
                    icon.sprite = skill.icon;
                    icon.color = Color.white;
                    icon.enabled = true;
                }
                else
                {
                    icon.sprite = null;
                    icon.color = noIconColor;
                    icon.enabled = skill != null;
                }
            }

            if (btn != null)
            {
                btn.interactable = skillSystem != null && skill != null;
            }
        }

        private void EnsureIconComponents()
        {
            slotIcon1 = EnsureIcon(slotBtn1, slotIcon1);
            slotIcon2 = EnsureIcon(slotBtn2, slotIcon2);
            slotIcon3 = EnsureIcon(slotBtn3, slotIcon3);
        }

        private Image EnsureIcon(Button btn, Image existing)
        {
            if (existing != null)
            {
                existing.transform.SetSiblingIndex(0);
                existing.raycastTarget = false;
                return existing;
            }
            if (btn == null)
            {
                return null;
            }

            GameObject go = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(btn.transform, false);
            go.transform.SetSiblingIndex(0);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = go.GetComponent<Image>();
            img.raycastTarget = false;
            img.enabled = false;
            return img;
        }

        private void EnsureCooldownComponents()
        {
            cooldownTxt1 = EnsureCooldown(slotBtn1, cooldownTxt1);
            cooldownTxt2 = EnsureCooldown(slotBtn2, cooldownTxt2);
            cooldownTxt3 = EnsureCooldown(slotBtn3, cooldownTxt3);
        }

        private TMP_Text EnsureCooldown(Button btn, TMP_Text existing)
        {
            if (existing != null)
            {
                return existing;
            }
            if (btn == null)
            {
                return null;
            }

            GameObject go = new GameObject("CooldownText", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(btn.transform, false);

            TextMeshProUGUI txt = go.GetComponent<TextMeshProUGUI>();
            txt.alignment = TextAlignmentOptions.Center;
            txt.fontSize = 28;
            txt.color = Color.white;

            go.SetActive(false);
            return txt;
        }

        private void BindButtons()
        {
            Bind(slotBtn1, CastSlot1);
            Bind(slotBtn2, CastSlot2);
            Bind(slotBtn3, CastSlot3);
        }

        private static void Bind(Button btn, UnityEngine.Events.UnityAction action)
        {
            if (btn == null)
            {
                return;
            }

            btn.onClick.RemoveListener(action);
            btn.onClick.AddListener(action);
        }

        private Button GetSlotButton(int i) => i switch { 0 => slotBtn1, 1 => slotBtn2, 2 => slotBtn3, _ => null };
        private TMP_Text GetSlotText(int i) => i switch { 0 => slotTxt1, 1 => slotTxt2, 2 => slotTxt3, _ => null };
        private Image GetSlotIcon(int i) => i switch { 0 => slotIcon1, 1 => slotIcon2, 2 => slotIcon3, _ => null };
        private TMP_Text GetCooldownText(int i) => i switch { 0 => cooldownTxt1, 1 => cooldownTxt2, 2 => cooldownTxt3, _ => null };
        private Image GetDimOverlay(int i) => i switch { 0 => dimOverlay1, 1 => dimOverlay2, 2 => dimOverlay3, _ => null };

        private void EnsureDimOverlays()
        {
            dimOverlay1 = EnsureDim(slotBtn1, dimOverlay1);
            dimOverlay2 = EnsureDim(slotBtn2, dimOverlay2);
            dimOverlay3 = EnsureDim(slotBtn3, dimOverlay3);
        }

        private Image EnsureDim(Button btn, Image existing)
        {
            if (existing != null)
            {
                return existing;
            }
            if (btn == null)
            {
                return null;
            }

            Transform found = btn.transform.Find("CooldownDim");
            if (found != null)
            {
                return found.GetComponent<Image>();
            }

            GameObject go = new GameObject("CooldownDim", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(btn.transform, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = go.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.6f);
            img.raycastTarget = false;

            go.SetActive(false);
            go.transform.SetAsLastSibling();
            return img;
        }

        private void EnsureNameTextLayout()
        {
            ApplyBottomLayout(slotBtn1, slotTxt1);
            ApplyBottomLayout(slotBtn2, slotTxt2);
            ApplyBottomLayout(slotBtn3, slotTxt3);
        }

        private static void ApplyBottomLayout(Button btn, TMP_Text txt)
        {
            if (btn == null || txt == null)
            {
                return;
            }

            RectTransform rt = txt.GetComponent<RectTransform>();
            if (rt == null)
            {
                return;
            }

            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0.38f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0f);

            txt.alignment = TextAlignmentOptions.Center;
            txt.fontSize = 22f;
            txt.enableWordWrapping = true;
            txt.color = Color.white;

            Transform bgTransform = btn.transform.Find("NameBg");
            if (bgTransform == null)
            {
                GameObject bg = new GameObject("NameBg", typeof(RectTransform), typeof(Image));
                bg.transform.SetParent(btn.transform, false);

                RectTransform bgRt = bg.GetComponent<RectTransform>();
                bgRt.anchorMin = new Vector2(0f, 0f);
                bgRt.anchorMax = new Vector2(1f, 0.38f);
                bgRt.offsetMin = Vector2.zero;
                bgRt.offsetMax = Vector2.zero;
                bgRt.pivot = new Vector2(0.5f, 0f);

                Image bgImg = bg.GetComponent<Image>();
                bgImg.color = new Color(0f, 0f, 0f, 0.55f);
                bgImg.raycastTarget = false;
                bg.transform.SetSiblingIndex(txt.transform.GetSiblingIndex());
            }

            txt.transform.SetAsLastSibling();
        }
    }
}
