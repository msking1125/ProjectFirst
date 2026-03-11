using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// 월드맵(챕터 선택) 및 챕터맵(스테이지 선택) 화면을 관리합니다.
/// CSV 기반 ChapterTable/StageTable에서 데이터를 읽고,
/// ChapterData에서 시각 에셋(아이콘, 스프라이트)을 참조합니다.
///
/// [Inspector 연결 가이드]
/// ┌ Data (Tables)
/// │  ├ chapterTable : ChapterTable.asset (CSV 임포트)
/// │  ├ stageTable   : StageTable.asset   (CSV 임포트)
/// │  └ playerData   : PlayerData.asset
/// ├ Visual Assets (Optional)
/// │  └ chapterData  : ChapterData.asset  (아이콘/스프라이트)
/// ├ UI
/// │  └ uiDocument   : Scene 의 UIDocument 컴포넌트
/// └ Events (Optional)
///    └ onStageSelected : 스테이지 선택 시 발행
/// </summary>
public class MapChapterManager : MonoBehaviour
{
    // ── Data (Tables) ──────────────────────────────────────────

    [Header("Data (Tables)")]
    [SerializeField] private ChapterTable _chapterTable;
    [SerializeField] private StageTable _stageTable;
    [SerializeField] private PlayerData _playerData;

    // ── Visual Assets (Optional) ───────────────────────────────

    [Header("Visual Assets")]
    [Tooltip("챕터 아이콘/스프라이트용. 없으면 기본 스타일 사용.")]
    [SerializeField] private ChapterData _chapterData;

    // ── UI ──────────────────────────────────────────────────────

    [Header("UI")]
    [SerializeField] private UIDocument _uiDocument;

    // ── Events (Optional) ──────────────────────────────────────

    [Header("Events")]
    [SerializeField] private VoidEventChannelSO _onStageSelected;

    // ── 상태 ────────────────────────────────────────────────────

    private enum MapViewState { WorldMap, ChapterMap }

    private MapViewState _currentView = MapViewState.WorldMap;

    /// <summary>현재 월드맵 뷰 상태인지 반환합니다.</summary>
    public bool IsWorldMapView => _currentView == MapViewState.WorldMap;

    private int _selectedChapterId;
    private StageRow _selectedStage;

    // ── 스크롤 드래그 ───────────────────────────────────────────

    private Vector2 _dragStart;
    private Vector2 _mapOffset;
    private bool _isDragging;

    // ── UI 요소 캐시 ────────────────────────────────────────────

    private VisualElement _root;
    private VisualElement _worldMapView;
    private VisualElement _chapterMapView;
    private VisualElement _chapterNodesContainer;
    private VisualElement _mapBackground;

    // 챕터맵
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

    // 월드맵
    private Button _worldBackBtn;

    // ── 전환 애니메이션 ─────────────────────────────────────────

    private const float TransitionDuration = 0.3f;

    // ─────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        BindUI();
        BuildWorldMap();
    }

    // ── UI 바인딩 ───────────────────────────────────────────────

    private void BindUI()
    {
        if (_uiDocument == null)
        {
            Debug.LogError("[MapChapterManager] UIDocument가 할당되지 않았습니다.");
            return;
        }

        _root = _uiDocument.rootVisualElement;

        // 월드맵
        _worldMapView = _root.Q<VisualElement>("world-map-view");
        _mapBackground = _root.Q<VisualElement>("map-background");
        _chapterNodesContainer = _root.Q<VisualElement>("chapter-nodes-container");
        _worldBackBtn = _root.Q<Button>("world-back-btn");

        // 챕터맵
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

        // 버튼 이벤트
        _worldBackBtn?.RegisterCallback<ClickEvent>(_ => OnWorldBackClicked());
        _chapterBackBtn?.RegisterCallback<ClickEvent>(_ => OnChapterBackClicked());
        _battleReadyBtn?.RegisterCallback<ClickEvent>(_ => OnBattleReadyClicked());

        // 드래그 스크롤
        if (_mapBackground != null)
        {
            _mapBackground.RegisterCallback<PointerDownEvent>(OnMapPointerDown);
            _mapBackground.RegisterCallback<PointerMoveEvent>(OnMapPointerMove);
            _mapBackground.RegisterCallback<PointerUpEvent>(OnMapPointerUp);
        }
    }

    // ── 월드맵 구축 ─────────────────────────────────────────────

    /// <summary>
    /// 챕터 노드를 월드맵에 배치합니다.
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

        // 섬 이미지 (ChapterData 에셋에서 시각 리소스 조회)
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

        // 잠금 시 구름 오버레이
        if (!chapter.isUnlocked)
        {
            var cloud = new VisualElement();
            cloud.name = "cloud-overlay";
            cloud.AddToClassList("cloud-overlay");
            node.Add(cloud);
        }

        // 별점 (ChapterData에서 클리어 별점 참조)
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

        // 현재 진행 챕터 표시
        if (_playerData != null && _playerData.currentChapter == chapter.id)
        {
            var charSd = new VisualElement();
            charSd.name = "current-char-sd";
            charSd.AddToClassList("current-char-sd");
            node.Add(charSd);
        }

        // 챕터 이름
        var nameLabel = new Label(chapter.name);
        nameLabel.AddToClassList("chapter-name-label");
        node.Add(nameLabel);

        // 클릭 이벤트
        int capturedId = chapter.id;
        node.RegisterCallback<ClickEvent>(_ => OnChapterNodeClicked(capturedId));

        return node;
    }

    // ── 챕터 노드 클릭 ──────────────────────────────────────────

    /// <summary>
    /// 챕터 노드 클릭 시 호출됩니다.
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

    // ── 챕터맵 전환 ─────────────────────────────────────────────

    /// <summary>
    /// 월드맵에서 챕터맵으로 전환합니다.
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
    /// 챕터맵에서 월드맵으로 복귀합니다.
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

    // ── 스테이지 목록 구축 ──────────────────────────────────────

    /// <summary>
    /// 챕터에 속한 스테이지 버튼 목록을 생성합니다.
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

            // 스테이지 번호
            var numberLabel = new Label($"{_selectedChapterId}-{stage.stageNumber}");
            numberLabel.AddToClassList("stage-number");
            btn.Add(numberLabel);

            // 별점 (PlayerData 기반 클리어 상태)
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

            // 잠금 아이콘
            if (!isUnlocked)
            {
                var lockIcon = new VisualElement();
                lockIcon.AddToClassList("lock-icon");
                btn.Add(lockIcon);
                btn.SetEnabled(false);
            }

            // 선택 이벤트
            var capturedStage = stage;
            bool capturedUnlocked = isUnlocked;
            btn.RegisterCallback<ClickEvent>(_ =>
            {
                if (capturedUnlocked)
                    OnStageSelected(capturedStage);
            });

            _stageList.Add(btn);

            // 다음 스테이지 잠금 여부 결정
            previousCleared = clearStars > 0;
        }

        // 첫 번째 해금 스테이지 자동 선택
        if (stages.Count > 0)
            OnStageSelected(stages[0]);
    }

    /// <summary>
    /// 스테이지 클리어 별점을 반환합니다.
    /// </summary>
    private int GetStageClearStars(int stageId)
    {
        // TODO: PlayerData에 스테이지별 클리어 정보 추가 시 연동
        // 현재는 첫 챕터 첫 스테이지만 해금
        if (_playerData != null && _playerData.currentChapter <= 1
            && _playerData.currentStage <= 1 && stageId <= 101)
            return 0;
        return 0;
    }

    // ── 스테이지 선택 ───────────────────────────────────────────

    /// <summary>
    /// 스테이지 선택 시 정보 패널을 갱신합니다.
    /// </summary>
    private void OnStageSelected(StageRow stage)
    {
        _selectedStage = stage;
        UpdateInfoPanel(stage);
        _onStageSelected?.RaiseEvent();
    }

    // ── 정보 패널 갱신 ──────────────────────────────────────────

    private void UpdateInfoPanel(StageRow stage)
    {
        if (_stageNameLabel != null)
            _stageNameLabel.text = stage.name;

        if (_stageDescLabel != null)
            _stageDescLabel.text = stage.description;

        if (_recommendPowerLabel != null)
            _recommendPowerLabel.text = $"권장 전투력: {stage.recommendedPower:N0}";

        // 적 속성 아이콘
        if (_enemyElementIcons != null)
        {
            _enemyElementIcons.Clear();
            var elementLabel = new Label(stage.enemyElement.ToString());
            elementLabel.AddToClassList("element-label");
            elementLabel.AddToClassList(
                $"element-{stage.enemyElement.ToString().ToLower()}");
            _enemyElementIcons.Add(elementLabel);
        }

        // 보상 미리보기 (CSV 골드/경험치)
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

        // 스태미나 비용
        if (_staminaCostLabel != null)
            _staminaCostLabel.text = $"스태미나 {stage.staminaCost}";

        // 전투 준비 버튼
        if (_battleReadyBtn != null)
            _battleReadyBtn.SetEnabled(true);
    }

    // ── 전투 준비 ───────────────────────────────────────────────

    /// <summary>
    /// 전투 준비 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnBattleReadyClicked()
    {
        if (_selectedStage == null) return;

        // 스태미나 확인
        if (_playerData != null && _playerData.stamina < _selectedStage.staminaCost)
        {
            ShowStaminaLackPopup(_selectedStage.staminaCost);
            return;
        }

        // 진행 정보 저장
        if (_playerData != null)
        {
            _playerData.currentChapter = _selectedChapterId;
            _playerData.currentStage = _selectedStage.stageNumber;
        }

        // 씬 전환
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

    // ── 뒤로가기 ────────────────────────────────────────────────

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

    // ── 팝업 ────────────────────────────────────────────────────

    private void ShowLockedPopup()
    {
        Debug.Log("[MapChapterManager] 이전 챕터를 클리어해야 합니다.");
    }

    private void ShowStaminaLackPopup(int required)
    {
        int current = _playerData != null ? _playerData.stamina : 0;
        Debug.Log(
            $"[MapChapterManager] 스태미나 부족 (필요: {required}, 보유: {current})");
    }

    // ── 드래그 스크롤 ───────────────────────────────────────────

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

    // ── 뷰 전환 유틸 ───────────────────────────────────────────

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

    // ── 유틸 ────────────────────────────────────────────────────

    /// <summary>
    /// 파티 전투력을 계산합니다.
    /// </summary>
    private int CalcPartyPower()
    {
        // TODO: 파티 편성 시스템 연동 후 실제 전투력 계산
        return 0;
    }
}
