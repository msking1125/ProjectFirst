using System;
using System.Collections.Generic;
using UnityEngine;

public class RunSession
{
    public int Level { get; private set; } = 1;
    public int Exp { get; private set; }
    public int Gold { get; private set; }

    // ?? ?꾪닾 以鍮??뺣낫 ????????????????????????????????????????????
    /// <summary>?몄꽦???먯씠?꾪듃 ID 紐⑸줉 (理쒕? 3媛?.</summary>
    public List<int> selectedAgentIds = new List<int>();
    /// <summary>吏꾩엯???ㅽ뀒?댁? ID.</summary>
    public int currentStageId;
    /// <summary>吏꾩엯??梨뺥꽣 ID.</summary>
    public int currentChapterId;
    /// <summary>?꾪닾 寃쎄낵 ?쒓컙(珥?.</summary>
    public float battleElapsedTime;
    /// <summary>?⑥씠釉?泥섏튂 ??</summary>
    public int waveKillCount;

    public int ExpToNextLevel => GetRequiredExpForLevel(Level);

    public event Action<int> OnLevelChanged;
    public event Action<int> OnReachedSkillPickLevel;

    public void Reset()
    {
        Level = 1;
        Exp = 0;
        Gold = 0;
        selectedAgentIds.Clear();
        currentStageId = 0;
        currentChapterId = 0;
        battleElapsedTime = 0f;
        waveKillCount = 0;
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
