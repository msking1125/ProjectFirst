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

    [Serializable]
    public class ExpItem
    {
        [SerializeField] private ExpItemType _type;
        [SerializeField] private string _itemName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private int _count;

        public ExpItem()
        {
        }

        public ExpItem(ExpItemType type, string itemName, Sprite icon = null, int count = 0)
        {
            _type = type;
            _itemName = itemName;
            _icon = icon;
            _count = Mathf.Max(0, count);
        }

        public ExpItemType type => _type;
        public string itemName => _itemName;
        public Sprite icon => _icon;
        public int count { get => _count; set => _count = Mathf.Max(0, value); }
        public int expValue => (int)_type;
    }
}
