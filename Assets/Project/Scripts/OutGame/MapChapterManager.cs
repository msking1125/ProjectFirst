using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ProjectFirst.Data;
/// <summary>
/// ?붾뱶留?梨뺥꽣 ?좏깮) 諛?梨뺥꽣留??ㅽ뀒?댁? ?좏깮) ?붾㈃??愿由ы빀?덈떎.
/// CSV 湲곕컲 ChapterTable/StageTable?먯꽌 ?곗씠?곕? ?쎄퀬,
/// ChapterData?먯꽌 ?쒓컖 ?먯뀑(?꾩씠肄? ?ㅽ봽?쇱씠????李몄“?⑸땲??
///
/// [Inspector ?곌껐 媛?대뱶]
/// ??Data (Tables)
/// ?? ??chapterTable : ChapterTable.asset (CSV ?꾪룷??
/// ?? ??stageTable   : StageTable.asset   (CSV ?꾪룷??
/// ?? ??playerData   : PlayerData.asset
/// ??Visual Assets (Optional)
/// ?? ??chapterData  : ChapterData.asset  (?꾩씠肄??ㅽ봽?쇱씠??
/// ??UI
/// ?? ??uiDocument   : Scene ??UIDocument 而댄룷?뚰듃
/// ??Events (Optional)
///    ??onStageSelected : ?ㅽ뀒?댁? ?좏깮 ??諛쒗뻾
/// </summary>
public class MapChapterManager : MonoBehaviour
{
    // ?? Data (Tables) ??????????????????????????????????????????

    [Header("Data (Tables)")]
    [SerializeField] private ChapterTable _chapterTable;
    [SerializeField] private StageTable _stageTable;
    [SerializeField] private PlayerData _playerData;

    // ?? Visual Assets (Optional) ???????????????????????????????

    [Header("Visual Assets")]
    [Tooltip("梨뺥꽣 ?꾩씠肄??ㅽ봽?쇱씠?몄슜. ?놁쑝硫?湲곕낯 ?ㅽ????ъ슜.")]
    [SerializeField] private ChapterData _chapterData;

    // ?? UI ??????????????????????????????????????????????????????

    [Header("UI")]
    [SerializeField] private UIDocument _uiDocument;

    // ?? Events (Optional) ??????????????????????????????????????

    [Header("Events")]
    [SerializeField] private VoidEventChannelSO _onStageSelected;

    // ?? ?곹깭 ????????????????????????????????????????????????????

    private enum MapViewState { WorldMap, ChapterMap }

    private MapViewState _currentView = MapViewState.WorldMap;

    /// <summary>?꾩옱 ?붾뱶留?酉??곹깭?몄? 諛섑솚?⑸땲??</summary>
    public bool IsWorldMapView => _currentView == MapViewState.WorldMap;

    private int _selectedChapterId;
    private StageRow _selectedStage;

    // ?? ?ㅽ겕濡??쒕옒洹????????????????????????????????????????????

    private Vector2 _dragStart;
    private Vector2 _mapOffset;
    private bool _isDragging;

    // ?? UI ?붿냼 罹먯떆 ????????????????????????????????????????????

    private VisualElement _root;
    private VisualElement _worldMapView;
    private VisualElement _chapterMapView;
    private VisualElement _chapterNodesContainer;
    private VisualElement _mapBackground;

    // 梨뺥꽣留?
    private Label _chapterHeaderLabel;
    private Button _chapterBackBtn;
    private VisualElement _stageList;
    private VisualElement _stageInfoPanel;
    private Label _stageNameLabel;
    private Label _stageDescLabel;
    private Label _recommendPowerLabel;
    private VisualElement _enemyElementIcons;
    private VisualElement _rewardPreview;
    private Button _battleReadyBtn;
    private Label _staminaCostLabel;

    // ?붾뱶留?
    private Button _worldBackBtn;

    // ?? ?꾪솚 ?좊땲硫붿씠???????????????????????????????????????????

    private const float TransitionDuration = 0.3f;

    // ?????????????????????????????????????????????????????????????

    private void OnEnable()
    {
        BindUI();
        BuildWorldMap();
    }

    // ?? UI 諛붿씤?????????????????????????????????????????????????

    private void BindUI()
    {
        if (_uiDocument == null)
        {
            Debug.LogError("[MapChapterManager] UIDocument媛 ?좊떦?섏? ?딆븯?듬땲??");
            return;
        }

        _root = _uiDocument.rootVisualElement;

        // ?붾뱶留?
        _worldMapView = _root.Q<VisualElement>("world-map-view");
        _mapBackground = _root.Q<VisualElement>("map-background");
        _chapterNodesContainer = _root.Q<VisualElement>("chapter-nodes-container");
        _worldBackBtn = _root.Q<Button>("world-back-btn");

        // 梨뺥꽣留?
        _chapterMapView = _root.Q<VisualElement>("chapter-map-view");
        _chapterHeaderLabel = _root.Q<Label>("chapter-header-label");
        _chapterBackBtn = _root.Q<Button>("chapter-back-btn");
        _stageList = _root.Q<VisualElement>("stage-list");
        _stageInfoPanel = _root.Q<VisualElement>("stage-info-panel");
        _stageNameLabel = _root.Q<Label>("stage-name");
        _stageDescLabel = _root.Q<Label>("stage-description");
        _recommendPowerLabel = _root.Q<Label>("recommend-power");
        _enemyElementIcons = _root.Q<VisualElement>("enemy-element-icons");
        _rewardPreview = _root.Q<VisualElement>("reward-preview");
        _battleReadyBtn = _root.Q<Button>("battle-ready-btn");
        _staminaCostLabel = _root.Q<Label>("stamina-cost-label");

        // 踰꾪듉 ?대깽??
        _worldBackBtn?.RegisterCallback<ClickEvent>(_ => OnWorldBackClicked());
        _chapterBackBtn?.RegisterCallback<ClickEvent>(_ => OnChapterBackClicked());
        _battleReadyBtn?.RegisterCallback<ClickEvent>(_ => OnBattleReadyClicked());

        // ?쒕옒洹??ㅽ겕濡?
        if (_mapBackground != null)
        {
            _mapBackground.RegisterCallback<PointerDownEvent>(OnMapPointerDown);
            _mapBackground.RegisterCallback<PointerMoveEvent>(OnMapPointerMove);
            _mapBackground.RegisterCallback<PointerUpEvent>(OnMapPointerUp);
        }
    }

    // ?? ?붾뱶留?援ъ텞 ?????????????????????????????????????????????

    /// <summary>
    /// 梨뺥꽣 ?몃뱶瑜??붾뱶留듭뿉 諛곗튂?⑸땲??
    /// </summary>
    private void BuildWorldMap()
    {
        _currentView = MapViewState.WorldMap;
        ShowWorldMap();

        if (_chapterNodesContainer == null || _chapterTable == null) return;

        _chapterNodesContainer.Clear();

        foreach (var chapter in _chapterTable.GetAll())
        {
            var node = CreateChapterNode(chapter);
            _chapterNodesContainer.Add(node);
        }
    }

    private VisualElement CreateChapterNode(ChapterRow chapter)
    {
        var node = new VisualElement();
        node.name = $"chapter-node-{chapter.id}";
        node.AddToClassList("chapter-node");
        node.style.position = Position.Absolute;
        node.style.left = chapter.WorldMapPosition.x;
        node.style.top = chapter.WorldMapPosition.y;

        // ???대?吏 (ChapterData ?먯뀑?먯꽌 ?쒓컖 由ъ냼??議고쉶)
        var islandImg = new VisualElement();
        islandImg.name = "chapter-island-img";
        islandImg.AddToClassList("chapter-island-img");
        var visualInfo = _chapterData != null ? _chapterData.GetById(chapter.id) : null;
        if (visualInfo?.worldMapIcon != null)
        {
            islandImg.style.backgroundImage =
                new StyleBackground(visualInfo.worldMapIcon);
        }
        node.Add(islandImg);

        // ?좉툑 ??援щ쫫 ?ㅻ쾭?덉씠
        if (!chapter.isUnlocked)
        {
            var cloud = new VisualElement();
            cloud.name = "cloud-overlay";
            cloud.AddToClassList("cloud-overlay");
            node.Add(cloud);
        }

        // 蹂꾩젏 (ChapterData?먯꽌 ?대━??蹂꾩젏 李몄“)
        int clearStars = visualInfo?.clearStars ?? 0;
        var starRow = new VisualElement();
        starRow.name = "star-row";
        starRow.AddToClassList("star-row");
        for (int i = 0; i < 3; i++)
        {
            var star = new VisualElement();
            star.AddToClassList(i < clearStars ? "star-filled" : "star-empty");
            starRow.Add(star);
        }
        node.Add(starRow);

        // ?꾩옱 吏꾪뻾 梨뺥꽣 ?쒖떆
        if (_playerData != null && _playerData.currentChapter == chapter.id)
        {
            var charSd = new VisualElement();
            charSd.name = "current-char-sd";
            charSd.AddToClassList("current-char-sd");
            node.Add(charSd);
        }

        // 梨뺥꽣 ?대쫫
        var nameLabel = new Label(chapter.name);
        nameLabel.AddToClassList("chapter-name-label");
        node.Add(nameLabel);

        // ?대┃ ?대깽??
        int capturedId = chapter.id;
        node.RegisterCallback<ClickEvent>(_ => OnChapterNodeClicked(capturedId));

        return node;
    }

    // ?? 梨뺥꽣 ?몃뱶 ?대┃ ??????????????????????????????????????????

    /// <summary>
    /// 梨뺥꽣 ?몃뱶 ?대┃ ???몄텧?⑸땲??
    /// </summary>
    private void OnChapterNodeClicked(int chapterId)
    {
        var chapter = _chapterTable.GetById(chapterId);
        if (chapter == null) return;

        if (!chapter.isUnlocked)
        {
            ShowLockedPopup();
            return;
        }

        _selectedChapterId = chapterId;
        TransitionToChapterMap(chapterId);
    }

    // ?? 梨뺥꽣留??꾪솚 ?????????????????????????????????????????????

    /// <summary>
    /// ?붾뱶留듭뿉??梨뺥꽣留듭쑝濡??꾪솚?⑸땲??
    /// </summary>
    private void TransitionToChapterMap(int chapterId)
    {
        _currentView = MapViewState.ChapterMap;

        var chapter = _chapterTable.GetById(chapterId);
        if (_chapterHeaderLabel != null && chapter != null)
            _chapterHeaderLabel.text = chapter.name;

        var stages = _stageTable.GetByChapter(chapterId);
        BuildStageList(stages);

        StartCoroutine(SlideTransitionCoroutine(
            _worldMapView, _chapterMapView, true));
    }

    /// <summary>
    /// 梨뺥꽣留듭뿉???붾뱶留듭쑝濡?蹂듦??⑸땲??
    /// </summary>
    private void TransitionToWorldMap()
    {
        _currentView = MapViewState.WorldMap;
        _selectedStage = null;

        StartCoroutine(SlideTransitionCoroutine(
            _chapterMapView, _worldMapView, false));
    }

    private IEnumerator SlideTransitionCoroutine(
        VisualElement hideTarget, VisualElement showTarget, bool slideLeft)
    {
        if (hideTarget == null || showTarget == null) yield break;

        float direction = slideLeft ? -1f : 1f;
        float elapsed = 0f;

        showTarget.style.display = DisplayStyle.Flex;
        showTarget.style.opacity = 0f;

        while (elapsed < TransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / TransitionDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            hideTarget.style.opacity = 1f - smooth;
            showTarget.style.opacity = smooth;
            hideTarget.style.translate =
                new Translate(direction * smooth * 100f, 0f, 0f);
            showTarget.style.translate =
                new Translate(direction * (1f - smooth) * -100f, 0f, 0f);

            yield return null;
        }

        hideTarget.style.display = DisplayStyle.None;
        hideTarget.style.translate = new Translate(0f, 0f, 0f);
        showTarget.style.translate = new Translate(0f, 0f, 0f);
        showTarget.style.opacity = 1f;
    }

    // ?? ?ㅽ뀒?댁? 紐⑸줉 援ъ텞 ??????????????????????????????????????

    /// <summary>
    /// 梨뺥꽣???랁븳 ?ㅽ뀒?댁? 踰꾪듉 紐⑸줉???앹꽦?⑸땲??
    /// </summary>
    private void BuildStageList(List<StageRow> stages)
    {
        if (_stageList == null) return;
        _stageList.Clear();

        bool previousCleared = true;

        foreach (var stage in stages)
        {
            bool isUnlocked = previousCleared;
            var btn = new Button();
            btn.name = $"stage-btn-{stage.id}";
            btn.AddToClassList("stage-button");

            // ?ㅽ뀒?댁? 踰덊샇
            var numberLabel = new Label($"{_selectedChapterId}-{stage.stageNumber}");
            numberLabel.AddToClassList("stage-number");
            btn.Add(numberLabel);

            // 蹂꾩젏 (PlayerData 湲곕컲 ?대━???곹깭)
            int clearStars = GetStageClearStars(stage.id);
            var starRow = new VisualElement();
            starRow.AddToClassList("star-row");
            for (int i = 0; i < 3; i++)
            {
                var star = new VisualElement();
                star.AddToClassList(i < clearStars ? "star-filled" : "star-empty");
                starRow.Add(star);
            }
            btn.Add(starRow);

            // ?좉툑 ?꾩씠肄?
            if (!isUnlocked)
            {
                var lockIcon = new VisualElement();
                lockIcon.AddToClassList("lock-icon");
                btn.Add(lockIcon);
                btn.SetEnabled(false);
            }

            // ?좏깮 ?대깽??
            var capturedStage = stage;
            bool capturedUnlocked = isUnlocked;
            btn.RegisterCallback<ClickEvent>(_ =>
            {
                if (capturedUnlocked)
                    OnStageSelected(capturedStage);
            });

            _stageList.Add(btn);

            // ?ㅼ쓬 ?ㅽ뀒?댁? ?좉툑 ?щ? 寃곗젙
            previousCleared = clearStars > 0;
        }

        // 泥?踰덉㎏ ?닿툑 ?ㅽ뀒?댁? ?먮룞 ?좏깮
        if (stages.Count > 0)
            OnStageSelected(stages[0]);
    }

    /// <summary>
    /// ?ㅽ뀒?댁? ?대━??蹂꾩젏??諛섑솚?⑸땲??
    /// </summary>
    private int GetStageClearStars(int stageId)
    {
        // TODO: PlayerData???ㅽ뀒?댁?蹂??대━???뺣낫 異붽? ???곕룞
        // ?꾩옱??泥?梨뺥꽣 泥??ㅽ뀒?댁?留??닿툑
        if (_playerData != null && _playerData.currentChapter <= 1
            && _playerData.currentStage <= 1 && stageId <= 101)
            return 0;
        return 0;
    }

    // ?? ?ㅽ뀒?댁? ?좏깮 ???????????????????????????????????????????

    /// <summary>
    /// ?ㅽ뀒?댁? ?좏깮 ???뺣낫 ?⑤꼸??媛깆떊?⑸땲??
    /// </summary>
    private void OnStageSelected(StageRow stage)
    {
        _selectedStage = stage;
        UpdateInfoPanel(stage);
        _onStageSelected?.RaiseEvent();
    }

    // ?? ?뺣낫 ?⑤꼸 媛깆떊 ??????????????????????????????????????????

    private void UpdateInfoPanel(StageRow stage)
    {
        if (_stageNameLabel != null)
            _stageNameLabel.text = stage.name;

        if (_stageDescLabel != null)
            _stageDescLabel.text = stage.description;

        if (_recommendPowerLabel != null)
            _recommendPowerLabel.text = $"沅뚯옣 ?꾪닾?? {stage.recommendedPower:N0}";

        // ???띿꽦 ?꾩씠肄?
        if (_enemyElementIcons != null)
        {
            _enemyElementIcons.Clear();
            var elementLabel = new Label(stage.enemyElement.ToString());
            elementLabel.AddToClassList("element-label");
            elementLabel.AddToClassList(
                $"element-{stage.enemyElement.ToString().ToLower()}");
            _enemyElementIcons.Add(elementLabel);
        }

        // 蹂댁긽 誘몃━蹂닿린 (CSV 怨⑤뱶/寃쏀뿕移?
        if (_rewardPreview != null)
        {
            _rewardPreview.Clear();

            if (stage.rewardGold > 0)
            {
                var goldSlot = new VisualElement();
                goldSlot.AddToClassList("reward-slot");
                var goldIcon = new VisualElement();
                goldIcon.AddToClassList("reward-icon");
                goldIcon.AddToClassList("gold-icon");
                goldSlot.Add(goldIcon);
                var goldLabel = new Label($"x{stage.rewardGold}");
                goldLabel.AddToClassList("reward-amount");
                goldSlot.Add(goldLabel);
                _rewardPreview.Add(goldSlot);
            }

            if (stage.rewardExp > 0)
            {
                var expSlot = new VisualElement();
                expSlot.AddToClassList("reward-slot");
                var expIcon = new VisualElement();
                expIcon.AddToClassList("reward-icon");
                expIcon.AddToClassList("exp-icon");
                expSlot.Add(expIcon);
                var expLabel = new Label($"x{stage.rewardExp}");
                expLabel.AddToClassList("reward-amount");
                expSlot.Add(expLabel);
                _rewardPreview.Add(expSlot);
            }
        }

        // ?ㅽ깭誘몃굹 鍮꾩슜
        if (_staminaCostLabel != null)
            _staminaCostLabel.text = $"?ㅽ깭誘몃굹 {stage.staminaCost}";

        // ?꾪닾 以鍮?踰꾪듉
        if (_battleReadyBtn != null)
            _battleReadyBtn.SetEnabled(true);
    }

    // ?? ?꾪닾 以鍮????????????????????????????????????????????????

    /// <summary>
    /// ?꾪닾 以鍮?踰꾪듉 ?대┃ ???몄텧?⑸땲??
    /// </summary>
    private void OnBattleReadyClicked()
    {
        if (_selectedStage == null) return;

        // ?ㅽ깭誘몃굹 ?뺤씤
        if (_playerData != null && _playerData.stamina < _selectedStage.staminaCost)
        {
            ShowStaminaLackPopup(_selectedStage.staminaCost);
            return;
        }

        // 吏꾪뻾 ?뺣낫 ???
        if (_playerData != null)
        {
            _playerData.currentChapter = _selectedChapterId;
            _playerData.currentStage = _selectedStage.stageNumber;
        }

        // ???꾪솚
        if (AsyncSceneLoader.Instance != null)
        {
            AsyncSceneLoader.Instance.LoadSceneAsync(
                "BattleReadyScene", LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene("BattleReadyScene");
        }
    }

    // ?? ?ㅻ줈媛湲?????????????????????????????????????????????????

    private void OnWorldBackClicked()
    {
        if (AsyncSceneLoader.Instance != null)
            AsyncSceneLoader.Instance.LoadSceneAsync("LobbyScene", LoadSceneMode.Single);
        else
            SceneManager.LoadScene("LobbyScene");
    }

    private void OnChapterBackClicked()
    {
        TransitionToWorldMap();
    }

    // ?? ?앹뾽 ????????????????????????????????????????????????????

    private void ShowLockedPopup()
    {
        Debug.Log("[MapChapterManager] ?댁쟾 梨뺥꽣瑜??대━?댄빐???⑸땲??");
    }

    private void ShowStaminaLackPopup(int required)
    {
        int current = _playerData != null ? _playerData.stamina : 0;
        Debug.Log(
            $"[MapChapterManager] ?ㅽ깭誘몃굹 遺議?(?꾩슂: {required}, 蹂댁쑀: {current})");
    }

    // ?? ?쒕옒洹??ㅽ겕濡????????????????????????????????????????????

    private void OnMapPointerDown(PointerDownEvent evt)
    {
        _isDragging = true;
        _dragStart = evt.position;
    }

    private void OnMapPointerMove(PointerMoveEvent evt)
    {
        if (!_isDragging) return;

        Vector2 delta = (Vector2)evt.position - _dragStart;
        _dragStart = evt.position;
        _mapOffset += delta;

        if (_chapterNodesContainer != null)
        {
            _chapterNodesContainer.style.translate =
                new Translate(_mapOffset.x, _mapOffset.y, 0f);
        }
    }

    private void OnMapPointerUp(PointerUpEvent evt)
    {
        _isDragging = false;
    }

    // ?? 酉??꾪솚 ?좏떥 ???????????????????????????????????????????

    private void ShowWorldMap()
    {
        if (_worldMapView != null)
        {
            _worldMapView.style.display = DisplayStyle.Flex;
            _worldMapView.style.opacity = 1f;
        }

        if (_chapterMapView != null)
            _chapterMapView.style.display = DisplayStyle.None;
    }

    // ?? ?좏떥 ????????????????????????????????????????????????????

    /// <summary>
    /// ?뚰떚 ?꾪닾?μ쓣 怨꾩궛?⑸땲??
    /// </summary>
    private int CalcPartyPower()
    {
        // TODO: ?뚰떚 ?몄꽦 ?쒖뒪???곕룞 ???ㅼ젣 ?꾪닾??怨꾩궛
        return 0;
    }
}




