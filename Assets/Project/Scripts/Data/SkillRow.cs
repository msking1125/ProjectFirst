using System;

[Serializable]
public class SkillRow
{
    public string id;
    public string name;
    public ElementType element = ElementType.Reason;
    public float coefficient = 1f;
    public float cooldown = 5f;
    public float range = 9999f;
}
