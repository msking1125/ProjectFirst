using System;
using UnityEngine;

[Serializable]
public class SkillRow
{
    public string id;
    public string name;
    public ElementType element  = ElementType.Reason;
    public float coefficient    = 1f;
    public float cooldown       = 5f;
    public float range          = 9999f;

    [Tooltip("스킬 아이콘 이미지 (Assets/Project/UI/Icon/*.jpg|png)")]
    public Sprite icon;

    [Tooltip("스킬 발동 시 재생할 VFX 프리팹")]
    public GameObject castVfxPrefab;
}
