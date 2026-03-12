using System;
using UnityEngine;

namespace ProjectFirst.Data
{
    public enum ExpItemType
    {
        Small = 100,
        Medium = 500,
        Large = 2000,
        Crystal = 10000
    }

    /// <summary>
    /// 경험치 아이템 데이터. 보유 수량과 경험치 값을 포함합니다.
    /// </summary>
    [Serializable]
    public class ExpItem
    {
        [SerializeField] private ExpItemType _type;
        [SerializeField] private string _itemName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private int _count;

        public ExpItemType type => _type;
        public string itemName => _itemName;
        public Sprite icon => _icon;
        public int count { get => _count; set => _count = Mathf.Max(0, value); }
        public int expValue => (int)_type;
    }
}
