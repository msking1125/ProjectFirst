using System.Collections.Generic;

public class RunSession
{
    public int Level { get; private set; }
    public int CurrentExp { get; private set; }
    public int Gold { get; private set; }

    private readonly HashSet<string> ownedSkillIds = new HashSet<string>();

    public IReadOnlyCollection<string> OwnedSkillIds => ownedSkillIds;
    public int ExpToNextLevel => GetRequiredExpForNextLevel();

    public void Reset()
    {
        Level = 1;
        CurrentExp = 0;
        Gold = 0;
        ownedSkillIds.Clear();
    }

    public int GetRequiredExpForNextLevel()
    {
        // Simple progression formula.
        return 10 + (Level - 1) * 5;
    }

    public int AddGold(int amount)
    {
        Gold += amount > 0 ? amount : 0;
        return Gold;
    }

    public int AddExperience(int amount)
    {
        CurrentExp += amount > 0 ? amount : 0;

        int levelUps = 0;
        while (CurrentExp >= GetRequiredExpForNextLevel())
        {
            CurrentExp -= GetRequiredExpForNextLevel();
            Level++;
            levelUps++;
        }

        return levelUps;
    }

    public bool TryAddSkill(string skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId))
        {
            return false;
        }

        return ownedSkillIds.Add(skillId.Trim());
    }

    public bool HasSkill(string skillId)
    {
        return !string.IsNullOrWhiteSpace(skillId) && ownedSkillIds.Contains(skillId.Trim());
    }
}
