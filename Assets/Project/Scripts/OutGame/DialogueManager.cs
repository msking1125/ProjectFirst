using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private string dialogueTablePath = "DialogueTable"; // Resources나 Addressables 키

    private VisualElement root;
    private Label speakerLabel;
    private Label textLabel;
    private VisualElement bgImage;
    private VisualElement charL;
    private VisualElement charR;
    private VisualElement choiceContainer;

    private List<DialogueData> currentDialogueList = new();
    private int currentIndex = 0;

    private void Awake()
    {
        root = uiDocument.rootVisualElement;
        speakerLabel = root.Q<Label>("speaker");
        textLabel = root.Q<Label>("dialogue-text");
        bgImage = root.Q<VisualElement>("background");
        charL = root.Q<VisualElement>("character-left");
        charR = root.Q<VisualElement>("character-right");
        choiceContainer = root.Q<VisualElement>("choice-container");
    }

    public async UniTask StartDialogue(string dialogueGroupId)
    {
        // CSV 또는 ScriptableObject 테이블 로드 (예시: Resources)
        // 실제로는 CSV Importer로 로드 추천
        currentDialogueList = await LoadDialogueTable(dialogueGroupId);
        currentIndex = 0;
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (currentIndex >= currentDialogueList.Count) { EndDialogue(); return; }

        var line = currentDialogueList[currentIndex];
        speakerLabel.text = line.speakerName;
        textLabel.text = line.text;

        // 배경 & 캐릭터 Addressables 로드
        Addressables.LoadAssetAsync<Sprite>(line.background).Completed += h => bgImage.style.backgroundImage = new StyleBackground(h.Result);
        // charL, charR 동일 방식

        choiceContainer.Clear();
        if (!string.IsNullOrEmpty(line.choiceA))
        {
            var btnA = new Button { text = line.choiceA };
            btnA.clicked += () => ChoiceSelected(0);
            choiceContainer.Add(btnA);
        }
        if (!string.IsNullOrEmpty(line.choiceB))
        {
            var btnB = new Button { text = line.choiceB };
            btnB.clicked += () => ChoiceSelected(1);
            choiceContainer.Add(btnB);
        }
    }

    private void ChoiceSelected(int choiceIndex)
    {
        // 분기 로직 (나중에 DialogueBranch 테이블로 확장)
        currentIndex++;
        ShowCurrentLine();
    }

    private void EndDialogue()
    {
        Debug.Log("다이얼로그 종료");
        // 이벤트 발행 또는 다음 씬 이동
    }

    private async UniTask<List<DialogueData>> LoadDialogueTable(string groupId)
    {
        // Addressables를 통해 DialogueTable 로드
        var handle = Addressables.LoadAssetAsync<DialogueTable>(dialogueTablePath);
        await handle;
        
        if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            DialogueTable table = handle.Result;
            return table.GetByGroupId(groupId);
        }
        else
        {
            Debug.LogError($"[DialogueManager] DialogueTable 로드 실패: {dialogueTablePath}");
            return new List<DialogueData>();
        }
    }
}