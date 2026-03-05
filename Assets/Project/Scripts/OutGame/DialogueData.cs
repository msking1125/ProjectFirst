using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "MindArk/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public string dialogueId;
    public string speakerName;
    public string text;
    public string background;           // 배경 이미지 이름 (Addressables 키)
    public string characterL;           // 왼쪽 캐릭터 이미지 이름
    public string characterR;           // 오른쪽 캐릭터 이미지 이름
    public string choiceA;              // 선택지 A
    public string choiceB;              // 선택지 B
}