using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueData
{
    public string groupId;              // 대화 그룹 식별자
    public string dialogueId;
    public string speakerName;
    [TextArea(3, 10)]
    public string text;
    public string background;           // 배경 이미지 이름 (Addressables 키)
    public string characterL;           // 왼쪽 캐릭터 이미지 이름
    public string characterR;           // 오른쪽 캐릭터 이미지 이름
    public string choiceA;              // 선택지 A
    public string choiceB;              // 선택지 B
}