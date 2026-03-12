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
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
///
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// Documentation cleaned.
/// </summary>
public class MapChapterManager : MonoBehaviour
{
    // Note: cleaned comment.

    [Header("Data (Tables)")]
    [SerializeField] private ChapterTable _chapterTable;
    [SerializeField] private StageTable _stageTable;
    [SerializeField] private PlayerData _playerData;

    // Note: cleaned comment.

    [Header("Visual Assets")]
    [Tooltip("Configured in inspector.")]
    [SerializeField] private ChapterData _chapterData;

    // Note: cleaned comment.

    [Header("UI")]
    [SerializeField] private UIDocument _uiDocument;

    // Note: cleaned comment.

    [Header("Events")]
    [SerializeField] private VoidEventChannelSO _onStageSelected;

    // Note: cleaned comment.

    private enum MapViewState { WorldMap, ChapterMap }

    private MapViewState _currentView = MapViewState.WorldMap;

    /// Documentation cleaned.
    public bool IsWorldMapView => _currentView == MapViewState.WorldMap;

    private int _selectedChapterId;
    private StageRow _selectedStage;

    // Note: cleaned comment.

    private Vector2 _dragStart;
    private Vector2 _mapOffset;
    private bool _isDragging;

    // Note: cleaned comment.

    private VisualElement _root;
    private VisualElement _worldMapView;
    private VisualElement _chapterMapView;
    private VisualElement _chapterNodesContainer;
    private VisualElement _mapBackground;

    // Note: cleaned comment.
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

    // Note: cleaned comment.
    private Button _worldBackBtn;

    // Note: cleaned comment.

    private const float TransitionDuration = 0.3f;

    // Note: cleaned comment.

    private void OnEnable()
    {
        BindUI();
        BuildWorldMap();
    }

    // Note: cleaned comment.

    private void BindUI()
    {
        if (_uiDocument == null)
        {
            Debug.LogError("[Log] Error message cleaned.");
            return;
        }

        _root = _uiDocument.rootVisualElement;

        // Note: cleaned comment.
        _worldMapView = _root.Q<VisualElement>("world-map-view");
        _mapBackground = _root.Q<VisualElement>("map-background");
        _chapterNodesContainer = _root.Q<VisualElement>("chapter-nodes-container");
        _worldBackBtn = _root.Q<Button>("world-back-btn");

        // Note: cleaned comment.
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

        // Note: cleaned comment.
        _worldBackBtn?.RegisterCallback<ClickEvent>(_ => OnWorldBackClicked());
        _chapterBackBtn?.RegisterCallback<ClickEvent>(_ => OnChapterBackClicked());
        _battleReadyBtn?.RegisterCallback<ClickEvent>(_ => OnBattleReadyClicked());

        // Note: cleaned comment.
        if (_mapBackground != null)
        {
            _mapBackground.RegisterCallback<PointerDownEvent>(OnMapPointerDown);
            _mapBackground.RegisterCallback<PointerMoveEvent>(OnMapPointerMove);
            _mapBackground.RegisterCallback<PointerUpEvent>(OnMapPointerUp);
        }
    }

    // Note: cleaned comment.

    /// <summary>
    /// Documentation cleaned.
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

        // Note: cleaned comment.
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

        // Note: cleaned comment.
        if (!chapter.isUnlocked)
        {
            var cloud = new VisualElement();
            cloud.name = "cloud-overlay";
            cloud.AddToClassList("cloud-overlay");
            node.Add(cloud);
        }

        // Note: cleaned comment.
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

        // Note: cleaned comment.
        if (_playerData != null && _playerData.currentChapter == chapter.id)
        {
            var charSd = new VisualElement();
            charSd.name = "current-char-sd";
            charSd.AddToClassList("current-char-sd");
            node.Add(charSd);
        }

        // Note: cleaned comment.
        var nameLabel = new Label(chapter.name);
        nameLabel.AddToClassList("chapter-name-label");
        node.Add(nameLabel);

        // Note: cleaned comment.
        int capturedId = chapter.id;
        node.RegisterCallback<ClickEvent>(_ => OnChapterNodeClicked(capturedId));

        return node;
    }

    // Note: cleaned comment.

    /// <summary>
    /// Documentation cleaned.
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

    // Note: cleaned comment.

    /// <summary>
    /// Documentation cleaned.
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
    /// Documentation cleaned.
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

    // Note: cleaned comment.

    /// <summary>
    /// Documentation cleaned.
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

            // Note: cleaned comment.
            var numberLabel = new Label($"{_selectedChapterId}-{stage.stageNumber}");
            numberLabel.AddToClassList("stage-number");
            btn.Add(numberLabel);

            // Note: cleaned comment.
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

            // Note: cleaned comment.
            if (!isUnlocked)
            {
                var lockIcon = new VisualElement();
                lockIcon.AddToClassList("lock-icon");
                btn.Add(lockIcon);
                btn.SetEnabled(false);
            }

            // Note: cleaned comment.
            var capturedStage = stage;
            bool capturedUnlocked = isUnlocked;
            btn.RegisterCallback<ClickEvent>(_ =>
            {
                if (capturedUnlocked)
                    OnStageSelected(capturedStage);
            });

            _stageList.Add(btn);

            // Note: cleaned comment.
            previousCleared = clearStars > 0;
        }

        // Note: cleaned comment.
        if (stages.Count > 0)
            OnStageSelected(stages[0]);
    }

    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    private int GetStageClearStars(int stageId)
    {
        // Note: cleaned comment.
        // Note: cleaned comment.
        if (_playerData != null && _playerData.currentChapter <= 1
            && _playerData.currentStage <= 1 && stageId <= 101)
            return 0;
        return 0;
    }

    // Note: cleaned comment.

    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    private void OnStageSelected(StageRow stage)
    {
        _selectedStage = stage;
        UpdateInfoPanel(stage);
        _onStageSelected?.RaiseEvent();
    }

    // Note: cleaned comment.

    private void UpdateInfoPanel(StageRow stage)
    {
        if (_stageNameLabel != null)
            _stageNameLabel.text = stage.name;

        if (_stageDescLabel != null)
            _stageDescLabel.text = stage.description;

        if (_recommendPowerLabel != null)
            _recommendPowerLabel.text = $"沅뚯옣 ?꾪닾?? {stage.recommendedPower:N0}";

        // Note: cleaned comment.
        if (_enemyElementIcons != null)
        {
            _enemyElementIcons.Clear();
            var elementLabel = new Label(stage.enemyElement.ToString());
            elementLabel.AddToClassList("element-label");
            elementLabel.AddToClassList(
                $"element-{stage.enemyElement.ToString().ToLower()}");
            _enemyElementIcons.Add(elementLabel);
        }

        // Note: cleaned comment.
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

        // Note: cleaned comment.
        if (_staminaCostLabel != null)
            _staminaCostLabel.text = $"?ㅽ깭誘몃굹 {stage.staminaCost}";

        // Note: cleaned comment.
        if (_battleReadyBtn != null)
            _battleReadyBtn.SetEnabled(true);
    }

    // Note: cleaned comment.

    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    private void OnBattleReadyClicked()
    {
        if (_selectedStage == null) return;

        // Note: cleaned comment.
        if (_playerData != null && _playerData.stamina < _selectedStage.staminaCost)
        {
            ShowStaminaLackPopup(_selectedStage.staminaCost);
            return;
        }

        // Note: cleaned comment.
        if (_playerData != null)
        {
            _playerData.currentChapter = _selectedChapterId;
            _playerData.currentStage = _selectedStage.stageNumber;
        }

        // Note: cleaned comment.
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

    // Note: cleaned comment.

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

    // Note: cleaned comment.

    private void ShowLockedPopup()
    {
        Debug.Log("[Log] Message cleaned.");
    }

    private void ShowStaminaLackPopup(int required)
    {
        int current = _playerData != null ? _playerData.stamina : 0;
        Debug.Log(
            $"[MapChapterManager] Not enough stamina. Required: {required}, Current: {current}");
    }

    // Note: cleaned comment.

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

    // Note: cleaned comment.

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

    // Note: cleaned comment.

    /// <summary>
    /// Documentation cleaned.
    /// </summary>
    private int CalcPartyPower()
    {
        // Note: cleaned comment.
        return 0;
    }
}




