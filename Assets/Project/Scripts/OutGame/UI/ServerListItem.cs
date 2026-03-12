using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ProjectFirst.Data;
/// <summary>
/// ?쒕쾭 ?좏깮 ?앹뾽??????ぉ UI.
/// Prefab??諛곗튂?섍퀬 ServerListContent ?꾨옒??Instantiate?⑸땲??
///
/// [Inspector ?곌껐 媛?대뱶]
///   serverNameText    : ?쒕쾭 ?대쫫 TMP_Text
///   playerCountText   : ?묒냽????TMP_Text ("1,234 / 3,000")
///   congestionText    : ?쇱옟??TMP_Text ("?먰솢"/"蹂댄넻"/"?쇱옟")
///   congestionBar     : ?묒냽??鍮꾩쑉 gauge Image (Type: Filled)
///   selectButton      : ?좏깮 Button
///   selectedIndicator : ?좏깮?먯쓣 ???쒖떆?섎뒗 GameObject (?뚮몢由???
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

    // ?쇱옟?꾨퀎 ?됱긽
    private static readonly Color ColorSmooth  = new(0.2f, 0.85f, 0.4f);  // ?먰솢 ??珥덈줉
    private static readonly Color ColorNormal  = new(1f,   0.75f, 0.1f);  // 蹂댄넻 ???몃옉
    private static readonly Color ColorCrowded = new(0.9f, 0.25f, 0.25f); // ?쇱옟 ??鍮④컯

    private void Awake()
    {
        selectButton?.onClick.AddListener(OnSelect);
        if (selectedIndicator) selectedIndicator.SetActive(false);
    }

    /// <summary>
    /// LoginManager?먯꽌 ??ぉ ?앹꽦 吏곹썑 ?몄텧?⑸땲??
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




