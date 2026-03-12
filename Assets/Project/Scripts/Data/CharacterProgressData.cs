using System;
using UnityEngine;

namespace ProjectFirst.Data
{
    [Serializable]
    public class CharacterProgressRecord
    {
        [SerializeField] private int _agentId;
        [SerializeField] private int _level = 1;
        [SerializeField] private int _exp;

        public int agentId { get => _agentId; set => _agentId = value; }
        public int level { get => _level; set => _level = Mathf.Max(1, value); }
        public int exp { get => _exp; set => _exp = Mathf.Max(0, value); }
    }

    [Serializable]
    public class ExpItemInventoryRecord
    {
        [SerializeField] private ExpItemType _type;
        [SerializeField] private int _count;

        public ExpItemType type { get => _type; set => _type = value; }
        public int count { get => _count; set => _count = Mathf.Max(0, value); }
    }
}
