using System;
using UnityEngine;

public class RunSession
{
    public int Level { get; private set; } = 1;
    public int Exp { get; private set; }
    public int Gold { get; private set; }

    public int ExpToNextLevel => GetRequiredExpForLevel(Level);

    public event Action<int> OnLevelChanged;
    public event Action<int> OnReachedSkillPickLevel;

    public void Reset()
    {
        Level = 1;
        Exp = 0;
        Gold = 0;
    }

    public int GetRequiredExpForLevel(int level)
    {
        return 10 + Math.Max(0, level - 1) * 5;
    }

    public int AddGold(int amount)
    {
        Gold += amount > 0 ? amount : 0;
        return Gold;
    }

    public int AddExp(int amount)
    {
        int added = amount > 0 ? amount : 0;
        int need = GetRequiredExpForLevel(Level);
        Debug.Log($"[RunSession] AddExp start added={added} currentExp={Exp} need={need} level={Level}");

        Exp += added;

        int levelUps = 0;
        while (Exp >= GetRequiredExpForLevel(Level))
        {
            Exp -= GetRequiredExpForLevel(Level);
            Level++;
            levelUps++;

            int nextNeed = GetRequiredExpForLevel(Level);
            Debug.Log($"[RunSession] LevelUp added={added} currentExp={Exp} need={nextNeed} level={Level}");

            OnLevelChanged?.Invoke(Level);
            if (Level % 3 == 0)
            {
                Debug.Log($"[RunSession] Trigger skill-pick event level={Level}");
                OnReachedSkillPickLevel?.Invoke(Level);
            }
        }

        return levelUps;
    }
}
