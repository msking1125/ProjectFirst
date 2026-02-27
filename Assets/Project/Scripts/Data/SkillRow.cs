using System;
using UnityEngine;

[Serializable]
public class SkillRow
{
    public string id;
    public string name;
    public ElementType element = ElementType.Reason;
    public float coefficient  = 1f;
    public float cooldown     = 5f;
    public float range        = 9999f;

    [Tooltip("스킬 버튼/선택 패널에 표시할 아이콘 이미지")]
    public Sprite icon;

    [Tooltip("스킬 발동 시 캐릭터 위치에 출력할 Cast VFX 프리팹")]
    public GameObject castVfxPrefab;
}
