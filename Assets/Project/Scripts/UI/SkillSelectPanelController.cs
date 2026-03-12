using System;
using System.Collections.Generic;
using ProjectFirst.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project
{
    public class SkillSelectPanelController : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Image dimImage;
        [SerializeField] private CanvasGroup dimCanvasGroup;

        [Header("Option Buttons")]
        [SerializeField] private Button optionBtn1;
        [SerializeField] private Button optionBtn2;
        [SerializeField] private Button optionBtn3;

        [Header("Option Text")]
        [SerializeField] private TMP_Text optionTxt1;
        [SerializeField] private TMP_Text optionTxt2;
        [SerializeField] private TMP_Text optionTxt3;
        [SerializeField] private TMP_Text optionDesc1;
        [SerializeField] private TMP_Text optionDesc2;
        [SerializeField] private TMP_Text optionDesc3;

        [Header("Option Icons")]
        [SerializeField] private Image optionIcon1;
        [SerializeField] private Image optionIcon2;
        [SerializeField] private Image optionIcon3;

        [Header("Layout")]
        [SerializeField] private float iconSize = 80f;
        [SerializeField] private float iconPadding = 8f;
        [SerializeField] private float textLeftOffset = 96f;

        private readonly List<SkillRow> currentOptions = new List<SkillRow>(3);
        private Action<SkillRow> onPicked;
        private bool hasLoggedMissingBindings;

        private void Awake()
        {
            AutoBind();
            BindButtons();

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            SetDimRaycast(false);
        }

        private void AutoBind()
        {
            panelRoot ??= gameObject;

            Transform optionBtn1Transform = transform.Find("Skill_Layer/Options/OptionButton_1");
            Transform optionBtn2Transform = transform.Find("Skill_Layer/Options/OptionButton_2");
            Transform optionBtn3Transform = transform.Find("Skill_Layer/Options/OptionButton_3");

            if (optionBtn1 == null && optionBtn1Transform != null)
            {
                optionBtn1 = optionBtn1Transform.GetComponent<Button>();
            }
            if (optionBtn2 == null && optionBtn2Transform != null)
            {
                optionBtn2 = optionBtn2Transform.GetComponent<Button>();
            }
            if (optionBtn3 == null && optionBtn3Transform != null)
            {
                optionBtn3 = optionBtn3Transform.GetComponent<Button>();
            }

            if (optionTxt1 == null && optionBtn1 != null)
            {
                optionTxt1 = optionBtn1.transform.Find("Name")?.GetComponent<TMP_Text>() ?? optionBtn1.transform.Find("OptionButton_1_Name")?.GetComponent<TMP_Text>();
            }
            if (optionTxt2 == null && optionBtn2 != null)
            {
                optionTxt2 = optionBtn2.transform.Find("Name")?.GetComponent<TMP_Text>() ?? optionBtn2.transform.Find("OptionButton_2_Name")?.GetComponent<TMP_Text>();
            }
            if (optionTxt3 == null && optionBtn3 != null)
            {
                optionTxt3 = optionBtn3.transform.Find("Name")?.GetComponent<TMP_Text>() ?? optionBtn3.transform.Find("OptionButton_3_Name")?.GetComponent<TMP_Text>();
            }

            if (optionDesc1 == null && optionBtn1 != null)
            {
                optionDesc1 = optionBtn1.transform.Find("OptionButton_1_Desc")?.GetComponent<TMP_Text>() ?? optionBtn1.transform.Find("Desc")?.GetComponent<TMP_Text>();
            }
            if (optionDesc2 == null && optionBtn2 != null)
            {
                optionDesc2 = optionBtn2.transform.Find("OptionButton_2_Desc")?.GetComponent<TMP_Text>() ?? optionBtn2.transform.Find("Desc")?.GetComponent<TMP_Text>();
            }
            if (optionDesc3 == null && optionBtn3 != null)
            {
                optionDesc3 = optionBtn3.transform.Find("OptionButton_3_Desc")?.GetComponent<TMP_Text>() ?? optionBtn3.transform.Find("Desc")?.GetComponent<TMP_Text>();
            }

            optionIcon1 = AutoBindIcon(optionBtn1, optionIcon1);
            optionIcon2 = AutoBindIcon(optionBtn2, optionIcon2);
            optionIcon3 = AutoBindIcon(optionBtn3, optionIcon3);

            if (dimImage == null)
            {
                dimImage = transform.Find("Dim")?.GetComponent<Image>();
            }

            if (dimCanvasGroup == null && dimImage != null)
            {
                dimCanvasGroup = dimImage.GetComponent<CanvasGroup>();
            }
        }

        private void WarnIfMissingBindings()
        {
            if (hasLoggedMissingBindings)
            {
                return;
            }

            List<string> missingFields = new List<string>();
            CheckField(nameof(panelRoot), panelRoot, missingFields);
            CheckField(nameof(optionBtn1), optionBtn1, missingFields);
            CheckField(nameof(optionBtn2), optionBtn2, missingFields);
            CheckField(nameof(optionBtn3), optionBtn3, missingFields);
            CheckField(nameof(optionTxt1), optionTxt1, missingFields);
            CheckField(nameof(optionTxt2), optionTxt2, missingFields);
            CheckField(nameof(optionTxt3), optionTxt3, missingFields);
            CheckField(nameof(dimImage), dimImage, missingFields);

            if (missingFields.Count == 0)
            {
                return;
            }

            hasLoggedMissingBindings = true;
            Debug.LogWarning($"[SkillSelectPanelController] Missing serialized references after AutoBind: {string.Join(", ", missingFields)}", this);
        }

        private static void CheckField(string fieldName, UnityEngine.Object obj, List<string> missingFields)
        {
            if (obj == null)
            {
                missingFields.Add(fieldName);
            }
        }

        public void Configure(GameObject root, Button button1, Button button2, Button button3, TMP_Text text1, TMP_Text text2, TMP_Text text3)
        {
            Configure(root, button1, button2, button3, text1, text2, text3, null, null, null);
        }

        public void Configure(GameObject root, Button button1, Button button2, Button button3, TMP_Text text1, TMP_Text text2, TMP_Text text3, TMP_Text desc1, TMP_Text desc2, TMP_Text desc3)
        {
            panelRoot = root;
            optionBtn1 = button1;
            optionBtn2 = button2;
            optionBtn3 = button3;
            optionTxt1 = text1;
            optionTxt2 = text2;
            optionTxt3 = text3;
            optionDesc1 = desc1;
            optionDesc2 = desc2;
            optionDesc3 = desc3;

            AutoBind();
            WarnIfMissingBindings();
            BindButtons();

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            SetDimRaycast(false);
        }

        public void ShowOptions(List<SkillRow> candidates, Action<SkillRow> onSelected)
        {
            AutoBind();
            BindButtons();

            currentOptions.Clear();
            onPicked = onSelected;

            if (candidates != null)
            {
                for (int i = 0; i < candidates.Count && i < 3; i++)
                {
                    if (candidates[i] != null)
                    {
                        currentOptions.Add(candidates[i]);
                    }
                }
            }

            optionIcon1 = AutoBindIcon(optionBtn1, optionIcon1);
            optionIcon2 = AutoBindIcon(optionBtn2, optionIcon2);
            optionIcon3 = AutoBindIcon(optionBtn3, optionIcon3);

            BindOption(0, optionBtn1, optionTxt1);
            BindOption(1, optionBtn2, optionTxt2);
            BindOption(2, optionBtn3, optionTxt3);

            if (panelRoot != null)
            {
                panelRoot.SetActive(currentOptions.Count > 0);
            }

            SetDimRaycast(currentOptions.Count > 0);

            if (currentOptions.Count > 0)
            {
                Time.timeScale = 0f;
            }
        }

        public void Pick0() => HandlePick(0);
        public void Pick1() => HandlePick(1);
        public void Pick2() => HandlePick(2);

        private void BindOption(int index, Button button, TMP_Text text)
        {
            bool hasOption = index < currentOptions.Count;
            SkillRow skill = hasOption ? currentOptions[index] : null;

            if (button != null)
            {
                button.gameObject.SetActive(hasOption);
                button.interactable = hasOption;
            }

            if (text != null)
            {
                text.text = skill != null ? skill.name : string.Empty;
            }

            TMP_Text desc = index switch { 0 => optionDesc1, 1 => optionDesc2, 2 => optionDesc3, _ => null };
            if (desc != null)
            {
                desc.text = skill != null ? skill.description : string.Empty;
            }

            Image icon = index switch { 0 => optionIcon1, 1 => optionIcon2, 2 => optionIcon3, _ => null };
            ApplyIcon(icon, skill);
        }

        private static void ApplyIcon(Image icon, SkillRow skill)
        {
            if (icon == null)
            {
                return;
            }

            if (skill?.icon != null)
            {
                icon.sprite = skill.icon;
                icon.color = Color.white;
                icon.enabled = true;
            }
            else
            {
                icon.sprite = null;
                icon.color = skill != null ? new Color(0.3f, 0.3f, 0.3f, 0.5f) : Color.clear;
                icon.enabled = skill != null;
            }
        }

        private Image AutoBindIcon(Button btn, Image existing)
        {
            if (existing != null)
            {
                return existing;
            }
            if (btn == null)
            {
                return null;
            }

            Transform iconChild = btn.transform.Find("Icon");
            if (iconChild != null)
            {
                Image img = iconChild.GetComponent<Image>();
                if (img != null)
                {
                    img.raycastTarget = false;
                    return img;
                }
            }

            GameObject go = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(btn.transform, false);
            go.transform.SetAsFirstSibling();

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.offsetMin = new Vector2(iconPadding, iconPadding);
            rt.offsetMax = new Vector2(iconPadding + iconSize, -iconPadding);

            Image newImg = go.GetComponent<Image>();
            newImg.raycastTarget = false;
            newImg.preserveAspect = true;
            newImg.enabled = false;

            TMP_Text[] texts = btn.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text txt in texts)
            {
                RectTransform txtRt = txt.GetComponent<RectTransform>();
                if (txtRt == null || txtRt.offsetMin.x >= textLeftOffset - 4f)
                {
                    continue;
                }

                txtRt.anchorMin = new Vector2(0f, 0f);
                txtRt.anchorMax = new Vector2(1f, 1f);
                txtRt.offsetMin = new Vector2(textLeftOffset, 4f);
                txtRt.offsetMax = new Vector2(-8f, -4f);
                break;
            }

            return newImg;
        }

        private void HandlePick(int index)
        {
            if (index < 0 || index >= currentOptions.Count)
            {
                return;
            }

            SkillRow selected = currentOptions[index];
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            SetDimRaycast(false);
            Time.timeScale = 1f;
            onPicked?.Invoke(selected);

            currentOptions.Clear();
            onPicked = null;
        }

        private void BindButtons()
        {
            if (optionBtn1 != null)
            {
                optionBtn1.onClick.RemoveListener(Pick0);
                optionBtn1.onClick.AddListener(Pick0);
            }

            if (optionBtn2 != null)
            {
                optionBtn2.onClick.RemoveListener(Pick1);
                optionBtn2.onClick.AddListener(Pick1);
            }

            if (optionBtn3 != null)
            {
                optionBtn3.onClick.RemoveListener(Pick2);
                optionBtn3.onClick.AddListener(Pick2);
            }
        }

        [ContextMenu("Show Dummy Options For Test")]
        public void ShowDummyOptionsForTest()
        {
            AutoBind();
            WarnIfMissingBindings();

            if (panelRoot != null)
            {
                panelRoot.gameObject.SetActive(true);
            }

            SetDimRaycast(true);

            currentOptions.Clear();
            onPicked = null;

            SetDummyOption(optionBtn1, optionTxt1, "Skill 1", "Option1 clicked");
            SetDummyOption(optionBtn2, optionTxt2, "Skill 2", "Option2 clicked");
            SetDummyOption(optionBtn3, optionTxt3, "Skill 3", "Option3 clicked");
        }

        private void SetDummyOption(Button button, TMP_Text text, string label, string logMessage)
        {
            Image icon = AutoBindIcon(button, null);
            if (icon != null)
            {
                icon.enabled = false;
            }

            if (button != null)
            {
                button.gameObject.SetActive(true);
                button.interactable = true;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    Debug.Log(logMessage);
                    if (panelRoot != null)
                    {
                        panelRoot.SetActive(false);
                    }

                    SetDimRaycast(false);
                });
            }

            if (text != null)
            {
                text.text = label;
            }
        }

        private void SetDimRaycast(bool isEnabled)
        {
            if (dimImage != null)
            {
                dimImage.raycastTarget = isEnabled;
            }

            if (dimCanvasGroup != null)
            {
                dimCanvasGroup.blocksRaycasts = isEnabled;
                dimCanvasGroup.interactable = isEnabled;
            }
        }
    }
}
