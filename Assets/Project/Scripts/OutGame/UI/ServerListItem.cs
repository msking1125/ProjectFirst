using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 서버 선택 팝업의 한 항목 UI.
/// Prefab에 배치하고 ServerListContent 아래에 Instantiate됩니다.
///
/// [Inspector 연결 가이드]
///   serverNameText    : 서버 이름 TMP_Text
///   playerCountText   : 접속자 수 TMP_Text ("1,234 / 3,000")
///   congestionText    : 혼잡도 TMP_Text ("원활"/"보통"/"혼잡")
///   congestionBar     : 접속자 비율 gauge Image (Type: Filled)
///   selectButton      : 선택 Button
///   selectedIndicator : 선택됐을 때 표시하는 GameObject (테두리 등)
/// </summary>
public class ServerListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text serverNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text congestionText;
    [SerializeField] private Image    congestionBar;
    [SerializeField] private Button   selectButton;
    [SerializeField] private GameObject selectedIndicator;

    private ServerData _data;
    private Action<string> _onSelected;

    // 혼잡도별 색상
    private static readonly Color ColorSmooth  = new(0.2f, 0.85f, 0.4f);  // 원활 — 초록
    private static readonly Color ColorNormal  = new(1f,   0.75f, 0.1f);  // 보통 — 노랑
    private static readonly Color ColorCrowded = new(0.9f, 0.25f, 0.25f); // 혼잡 — 빨강

    private void Awake()
    {
        selectButton?.onClick.AddListener(OnSelect);
        if (selectedIndicator) selectedIndicator.SetActive(false);
    }

    /// <summary>
    /// LoginManager에서 항목 생성 직후 호출합니다.
    /// </summary>
    public void Setup(ServerData data, Action<string> onSelected)
    {
        _data       = data;
        _onSelected = onSelected;

        if (serverNameText)
            serverNameText.text = data.displayName;

        if (playerCountText)
            playerCountText.text = $"{data.currentPlayers:N0} / {data.maxPlayers:N0}";

        Color congColor = data.LoadRatio < 0.5f ? ColorSmooth
                        : data.LoadRatio < 0.85f ? ColorNormal
                        : ColorCrowded;

        if (congestionText)
        {
            congestionText.text  = data.CongestionLabel;
            congestionText.color = congColor;
        }

        if (congestionBar)
        {
            congestionBar.type       = Image.Type.Filled;
            congestionBar.fillMethod = Image.FillMethod.Horizontal;
            congestionBar.fillAmount = data.LoadRatio;
            congestionBar.color      = congColor;
        }

        SetSelected(false);
    }

    private void OnSelect()
    {
        SetSelected(true);
        _onSelected?.Invoke(_data?.serverId);
    }

    public void SetSelected(bool selected)
    {
        if (selectedIndicator) selectedIndicator.SetActive(selected);
    }
}
