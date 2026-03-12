using System;
using ProjectFirst.Data;
using UnityEngine;

namespace ProjectFirst.Data
{
    [Serializable]
    public class AgentInfo
    {
        [SerializeField] private int _id;
        [SerializeField] private string _agentName;
        [SerializeField] private string _subName;
        [SerializeField] private ElementType _element;
        [SerializeField] private AttackType _attackType;
        [SerializeField] private int _grade;
        [SerializeField] private Sprite _thumbnail;
        [SerializeField] private GameObject _modelPrefab;
        [SerializeField] private SkillRow[] _skills;
        [Header("Settings")]
        [SerializeField] private float _baseHp;
        [SerializeField] private float _baseAtk;
        [SerializeField] private float _baseDef;
        [SerializeField] private float _critRate;
        [SerializeField] private float _critMult = 1.5f;
        [Header("Settings")]
        [SerializeField] private float _hpGrowth = 0.08f;
        [SerializeField] private float _atkGrowth = 0.06f;
        [SerializeField] private float _defGrowth = 0.04f;

        public int id => _id;
        public string agentName => _agentName;
        public string subName => _subName;
        public ElementType element => _element;
        public AttackType attackType => _attackType;
        public int grade => _grade;
        public Sprite thumbnail => _thumbnail;
        public GameObject modelPrefab => _modelPrefab;
        public SkillRow[] skills => _skills;
        public float baseHp => _baseHp;
        public float baseAtk => _baseAtk;
        public float baseDef => _baseDef;
        public float critRate => _critRate;
        public float critMult => _critMult;
        public float GetHp(int level) => _baseHp * (1 + _hpGrowth * (level - 1));
        public float GetAtk(int level) => _baseAtk * (1 + _atkGrowth * (level - 1));
        public float GetDef(int level) => _baseDef * (1 + _defGrowth * (level - 1));
        public int GetPower(int level) => (int)(GetHp(level) * 0.5f + GetAtk(level) * 3f + GetDef(level) * 2f);
    }
}


