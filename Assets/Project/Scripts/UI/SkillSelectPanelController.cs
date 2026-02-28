using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSelectPanelController : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button optionBtn1;
    [SerializeField] private Button optionBtn2;
    [SerializeField] private Button optionBtn3;
    [SerializeField] private TMP_Text optionTxt1;
    [SerializeField] private TMP_Text optionTxt2;
    [SerializeField] private TMP_Text optionTxt3;
    [Header("아이콘 Image (없으면 자동 탐지/생성)")]
    [SerializeField] private Image optionIcon1;
    [SerializeField] private Image optionIcon2;
    [SerializeField] private Image optionIcon3;
    [SerializeField] private Image dimImage;
    [SerializeField] private CanvasGroup dimCanvasGroup;

    [Header("아이콘 레이아웃 (Inspector에서 수정 가능)")]
    [Tooltip("아이콘 크기 (픽셀). 버튼 높이보다 작게 설정하세요.")]
    [SerializeField] private float iconSize = 80f;
    [Tooltip("아이콘 좌측/상하 패딩 (픽셀)")]
    [SerializeField] private float iconPadding = 8f;
    [Tooltip("텍스트 시작 X 위치 (아이콘 오른쪽 여백 포함, 픽셀)")]
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
            optionTxt1 = optionBtn1.transform.Find("Name")?.GetComponent<TMP_Text>();
        }
        if (optionTxt2 == null && optionBtn2 != null)
        {
            optionTxt2 = optionBtn2.transform.Find("Name")?.GetComponent<TMP_Text>();
        }
        if (optionTxt3 == null && optionBtn3 != null)
        {
            optionTxt3 = optionBtn3.transform.Find("Name")?.GetComponent<TMP_Text>();
        }

        // 아이콘 자동 탐지
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
        panelRoot = root;
        optionBtn1 = button1;
        optionBtn2 = button2;
        optionBtn3 = button3;
        optionTxt1 = text1;
        optionTxt2 = text2;
        optionTxt3 = text3;
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

        if (button != null)
        {
            button.gameObject.SetActive(hasOption);
            button.interactable = hasOption;
        }

        if (text != null)
        {
            text.text = hasOption ? currentOptions[index].name : string.Empty;
        }

        // 아이콘 적용
        Image icon = index switch { 0 => optionIcon1, 1 => optionIcon2, 2 => optionIcon3, _ => null };
        ApplyIcon(icon, hasOption ? currentOptions[index] : null);
    }

    private static void ApplyIcon(Image icon, SkillRow skill)
    {
        if (icon == null) return;
        if (skill?.icon != null)
        {
            icon.sprite  = skill.icon;
            icon.color   = Color.white;
            icon.enabled = true;
        }
        else
        {
            icon.sprite  = null;
            icon.color   = skill != null ? new Color(0.3f, 0.3f, 0.3f, 0.5f) : Color.clear;
            icon.enabled = skill != null;
        }
    }

    /// <summary>
    /// OptionButton에서 아이콘 Image를 탐지/생성합니다.
    /// OptionButton은 500×100 가로형: 아이콘을 좌측 80×80에 배치하고
    /// Name 텍스트를 아이콘 오른쪽으로 밀어냅니다.
    /// </summary>
    private Image AutoBindIcon(Button btn, Image existing)
    {
        if (existing != null) return existing;
        if (btn == null) return null;

        // "Icon" 이름 자식 탐색
        Transform iconChild = btn.transform.Find("Icon");
        if (iconChild != null)
        {
            Image img = iconChild.GetComponent<Image>();
            if (img != null) { img.raycastTarget = false; return img; }
        }

        // ── 새로 생성 ────────────────────────────────────────────────────────
        // OptionButton: 500×100 → 아이콘 좌측 80×80 고정 크기
        GameObject go = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(btn.transform, false);
        go.transform.SetAsFirstSibling();

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot     = new Vector2(0f, 0.5f);
        rt.offsetMin = new Vector2(iconPadding, iconPadding);
        rt.offsetMax = new Vector2(iconPadding + iconSize, -iconPadding); // iconSize × iconSize

        Image newImg = go.GetComponent<Image>();
        newImg.raycastTarget  = false;
        newImg.preserveAspect = true;
        newImg.enabled = false;

        // Name 텍스트를 아이콘 오른쪽으로 밀기
        TMP_Text[] texts = btn.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text txt in texts)
        {
            RectTransform txtRt = txt.GetComponent<RectTransform>();
            if (txtRt == null) continue;
            if (txtRt.offsetMin.x >= textLeftOffset - 4f) continue; // 이미 충분한 경우 스킵
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
        if (icon != null) icon.enabled = false;

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
