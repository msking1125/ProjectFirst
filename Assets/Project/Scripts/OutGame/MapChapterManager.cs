using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// 월드맵(챕터 선택) 및 챕터맵(스테이지 선택) 화면을 관리합니다.
///
/// [Inspector 연결 가이드]
/// ┌ Data
/// │  ├ chapterData : ChapterData.asset
/// │  ├ stageData   : StageData.asset
/// │  └ playerData  : PlayerData.asset
/// ├ UI
/// │  └ uiDocument  : Scene 의 UIDocument 컴포넌트
/// └ Events (Optional)
///    └ onStageSelected : 스테이지 선택 시 발행
/// </summary>
public class MapChapterManager : MonoBehaviour
{
    // ── Data ────────────────────────────────────────────────────

    [Header("Data")]
    [SerializeField] private ChapterData _chapterData;
    [SerializeField] private StageData _stageData;
    [SerializeField] private PlayerData _playerData;

    // ── UI ──────────────────────────────────────────────────────

    [Header("UI")]
    [SerializeField] private UIDocument _uiDocument;

    // ── Events (Optional) ──────────────────────────────────────

    [Header("Events")]
    [SerializeField] private VoidEventChannelSO _onStageSelected;

    // ── 상태 ────────────────────────────────────────────────────

    private enum MapViewState { WorldMap, ChapterMap }

    private MapViewState _currentView = MapViewState.WorldMap;
    private int _selectedChapterId;
    private StageData.StageInfo _selectedStage;

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

        if (_chapterNodesContainer == null || _chapterData == null) return;

        _chapterNodesContainer.Clear();

        foreach (var chapter in _chapterData.chapters)
        {
            var node = CreateChapterNode(chapter);
            _chapterNodesContainer.Add(node);
        }
    }

    private VisualElement CreateChapterNode(ChapterData.ChapterInfo chapter)
    {
        var node = new VisualElement();
        node.name = $"chapter-node-{chapter.chapterId}";
        node.AddToClassList("chapter-node");
        node.style.position = Position.Absolute;
        node.style.left = chapter.worldMapPosition.x;
        node.style.top = chapter.worldMapPosition.y;

        // 섬 이미지
        var islandImg = new VisualElement();
        islandImg.name = "chapter-island-img";
        islandImg.AddToClassList("chapter-island-img");
        if (chapter.worldMapIcon != null)
        {
            islandImg.style.backgroundImage =
                new StyleBackground(chapter.worldMapIcon);
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

        // 별점
        var starRow = new VisualElement();
        starRow.name = "star-row";
        starRow.AddToClassList("star-row");
        for (int i = 0; i < 3; i++)
        {
            var star = new VisualElement();
            star.AddToClassList(i < chapter.clearStars ? "star-filled" : "star-empty");
            starRow.Add(star);
        }
        node.Add(starRow);

        // 현재 진행 챕터 표시
        if (_playerData != null && _playerData.currentChapter == chapter.chapterId)
        {
            var charSd = new VisualElement();
            charSd.name = "current-char-sd";
            charSd.AddToClassList("current-char-sd");
            node.Add(charSd);
        }

        // 챕터 이름
        var nameLabel = new Label(chapter.chapterName);
        nameLabel.AddToClassList("chapter-name-label");
        node.Add(nameLabel);

        // 클릭 이벤트
        int capturedId = chapter.chapterId;
        node.RegisterCallback<ClickEvent>(_ => OnChapterNodeClicked(capturedId));

        return node;
    }

    // ── 챕터 노드 클릭 ──────────────────────────────────────────

    /// <summary>
    /// 챕터 노드 클릭 시 호출됩니다.
    /// </summary>
    private void OnChapterNodeClicked(int chapterId)
    {
        var chapter = _chapterData.GetById(chapterId);
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

        var chapter = _chapterData.GetById(chapterId);
        if (_chapterHeaderLabel != null && chapter != null)
            _chapterHeaderLabel.text = chapter.chapterName;

        var stages = _stageData.GetByChapter(chapterId);
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
    private void BuildStageList(List<StageData.StageInfo> stages)
    {
        if (_stageList == null) return;
        _stageList.Clear();

        foreach (var stage in stages)
        {
            var btn = new Button();
            btn.name = $"stage-btn-{stage.stageId}";
            btn.AddToClassList("stage-button");

            // 스테이지 번호
            var numberLabel = new Label($"{_selectedChapterId}-{stage.stageNumber}");
            numberLabel.AddToClassList("stage-number");
            btn.Add(numberLabel);

            // 별점
            var starRow = new VisualElement();
            starRow.AddToClassList("star-row");
            for (int i = 0; i < 3; i++)
            {
                var star = new VisualElement();
                star.AddToClassList(i < stage.clearStars ? "star-filled" : "star-empty");
                starRow.Add(star);
            }
            btn.Add(starRow);

            // 잠금 아이콘
            if (!stage.isUnlocked)
            {
                var lockIcon = new VisualElement();
                lockIcon.AddToClassList("lock-icon");
                btn.Add(lockIcon);
                btn.SetEnabled(false);
            }

            // 선택 이벤트
            var capturedStage = stage;
            btn.RegisterCallback<ClickEvent>(_ => OnStageSelected(capturedStage));

            _stageList.Add(btn);
        }

        // 첫 번째 해금 스테이지 자동 선택
        var firstUnlocked = stages.Find(s => s.isUnlocked);
        if (firstUnlocked != null)
            OnStageSelected(firstUnlocked);
    }

    // ── 스테이지 선택 ───────────────────────────────────────────

    /// <summary>
    /// 스테이지 선택 시 정보 패널을 갱신합니다.
    /// </summary>
    private void OnStageSelected(StageData.StageInfo stage)
    {
        _selectedStage = stage;
        UpdateInfoPanel(stage);
        _onStageSelected?.RaiseEvent();
    }

    // ── 정보 패널 갱신 ──────────────────────────────────────────

    private void UpdateInfoPanel(StageData.StageInfo stage)
    {
        if (_stageNameLabel != null)
            _stageNameLabel.text = stage.stageName;

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
            elementLabel.AddToClassList($"element-{stage.enemyElement.ToString().ToLower()}");
            _enemyElementIcons.Add(elementLabel);
        }

        // 보상 미리보기
        if (_rewardPreview != null)
        {
            _rewardPreview.Clear();
            foreach (var reward in stage.previewRewards)
            {
                var rewardSlot = new VisualElement();
                rewardSlot.AddToClassList("reward-slot");

                if (reward.icon != null)
                {
                    var icon = new VisualElement();
                    icon.AddToClassList("reward-icon");
                    icon.style.backgroundImage =
                        new StyleBackground(reward.icon);
                    rewardSlot.Add(icon);
                }

                var amountLabel = new Label($"x{reward.amount}");
                amountLabel.AddToClassList("reward-amount");
                rewardSlot.Add(amountLabel);

                _rewardPreview.Add(rewardSlot);
            }
        }

        // 스태미나 비용
        if (_staminaCostLabel != null)
            _staminaCostLabel.text = $"스태미나 {stage.staminaCost}";

        // 전투 준비 버튼 활성화
        if (_battleReadyBtn != null)
            _battleReadyBtn.SetEnabled(stage.isUnlocked);
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
        Debug.Log($"[MapChapterManager] 스태미나 부족 (필요: {required}, 보유: {current})");
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

    private void ShowChapterMap()
    {
        if (_worldMapView != null)
            _worldMapView.style.display = DisplayStyle.None;

        if (_chapterMapView != null)
        {
            _chapterMapView.style.display = DisplayStyle.Flex;
            _chapterMapView.style.opacity = 1f;
        }
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
