using System;
using ProjectFirst.Data;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    /// <summary>
    /// 캐릭터 관리 화면에서 사용하는 캐릭터 상세 정보.
    /// AgentTable.agentInfos 리스트로 관리됩니다.
    /// </summary>
    [Serializable]
#if ODIN_INSPECTOR
    [HideLabel]
#endif
    public class AgentInfo
    {
#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/ID")]
        [LabelText("ID")]
#endif
        [SerializeField] private int _id;

#if ODIN_INSPECTOR
        [HorizontalGroup("기본", 0.5f)]
        [BoxGroup("기본/이름")]
        [LabelText("캐릭터명")]
#endif
        [SerializeField] private string _agentName;

#if ODIN_INSPECTOR
        [BoxGroup("기본")]
        [LabelText("부제")]
#endif
        [SerializeField] private string _subName;

#if ODIN_INSPECTOR
        [HorizontalGroup("속성", 0.33f)]
        [BoxGroup("속성/속성")]
        [LabelText("속성")]
        [EnumToggleButtons]
#endif
        [SerializeField] private ElementType _element;

#if ODIN_INSPECTOR
        [HorizontalGroup("속성", 0.33f)]
        [BoxGroup("속성/타입")]
        [LabelText("공격 타입")]
        [EnumToggleButtons]
#endif
        [SerializeField] private AttackType _attackType;

#if ODIN_INSPECTOR
        [HorizontalGroup("속성", 0.34f)]
        [BoxGroup("속성/등급")]
        [LabelText("등급")]
        [ProgressBar(1, 5, ColorGetter = "GetGradeColor")]
#endif
        [SerializeField] private int _grade;

#if ODIN_INSPECTOR
        [HorizontalGroup("리소스", 0.5f)]
        [BoxGroup("리소스/썸네일")]
        [LabelText("썸네일")]
        [PreviewField(60, ObjectFieldAlignment.Left)]
#endif
        [SerializeField] private Sprite _thumbnail;

#if ODIN_INSPECTOR
        [HorizontalGroup("리소스", 0.5f)]
        [BoxGroup("리소스/모델")]
        [LabelText("모델 프리팹")]
        [AssetsOnly]
        [PreviewField(60, ObjectFieldAlignment.Left)]
#endif
        [SerializeField] private GameObject _modelPrefab;

#if ODIN_INSPECTOR
        [BoxGroup("스킬")]
        [LabelText("스킬 목록")]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = false)]
#endif
        [SerializeField] private SkillRow[] _skills;

#if ODIN_INSPECTOR
        [Title("기본 스탯", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("기본스탯", 0.2f)]
        [BoxGroup("기본스탯/HP")]
        [LabelText("HP")]
        [ProgressBar(0, 1000, ColorGetter = "GetHpColor")]
#endif
        [Header("기본 스탯")]
        [SerializeField] private float _baseHp;

#if ODIN_INSPECTOR
        [HorizontalGroup("기본스탯", 0.2f)]
        [BoxGroup("기본스탯/ATK")]
        [LabelText("ATK")]
        [ProgressBar(0, 100, ColorGetter = "GetAtkColor")]
#endif
        [SerializeField] private float _baseAtk;

#if ODIN_INSPECTOR
        [HorizontalGroup("기본스탯", 0.2f)]
        [BoxGroup("기본스탯/DEF")]
        [LabelText("DEF")]
        [ProgressBar(0, 50, ColorGetter = "GetDefColor")]
#endif
        [SerializeField] private float _baseDef;

#if ODIN_INSPECTOR
        [HorizontalGroup("기본스탯", 0.2f)]
        [BoxGroup("기본스탯/치명")]
        [LabelText("치명률")]
        [ProgressBar(0, 1, ColorGetter = "GetCritColor")]
#endif
        [SerializeField] private float _critRate;

#if ODIN_INSPECTOR
        [HorizontalGroup("기본스탯", 0.2f)]
        [BoxGroup("기본스탯/배율")]
        [LabelText("치명배율")]
        [Min(1f)]
#endif
        [SerializeField] private float _critMult = 1.5f;

#if ODIN_INSPECTOR
        [Title("성장 배율", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("성장", 0.33f)]
        [BoxGroup("성장/HP")]
        [LabelText("HP 성장")]
        [ProgressBar(0, 0.2, ColorGetter = "GetGrowthColor")]
        [SuffixLabel("%", true)]
#endif
        [Header("성장 배율")]
        [SerializeField] private float _hpGrowth = 0.08f;

#if ODIN_INSPECTOR
        [HorizontalGroup("성장", 0.33f)]
        [BoxGroup("성장/ATK")]
        [LabelText("ATK 성장")]
        [ProgressBar(0, 0.2, ColorGetter = "GetGrowthColor")]
        [SuffixLabel("%", true)]
#endif
        [SerializeField] private float _atkGrowth = 0.06f;

#if ODIN_INSPECTOR
        [HorizontalGroup("성장", 0.34f)]
        [BoxGroup("성장/DEF")]
        [LabelText("DEF 성장")]
        [ProgressBar(0, 0.2, ColorGetter = "GetGrowthColor")]
        [SuffixLabel("%", true)]
#endif
        [SerializeField] private float _defGrowth = 0.04f;

#if ODIN_INSPECTOR
        private static Color GetHpColor() => new Color(1f, 0.3f, 0.3f);
        private static Color GetAtkColor() => new Color(1f, 0.6f, 0.2f);
        private static Color GetDefColor() => new Color(0.3f, 0.6f, 1f);
        private static Color GetCritColor() => new Color(0.8f, 0.4f, 0.8f);
        private static Color GetGradeColor() => new Color(1f, 0.8f, 0.2f);
        private static Color GetGrowthColor() => new Color(0.3f, 0.8f, 0.3f);
#endif

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

        /// <summary>레벨에 따른 HP를 반환합니다.</summary>
        public float GetHp(int level) => _baseHp * (1 + _hpGrowth * (level - 1));

        /// <summary>레벨에 따른 ATK를 반환합니다.</summary>
        public float GetAtk(int level) => _baseAtk * (1 + _atkGrowth * (level - 1));

        /// <summary>레벨에 따른 DEF를 반환합니다.</summary>
        public float GetDef(int level) => _baseDef * (1 + _defGrowth * (level - 1));

        /// <summary>레벨 기반 종합 전투력을 반환합니다.</summary>
        public int GetPower(int level) => (int)(GetHp(level) * 0.5f + GetAtk(level) * 3f + GetDef(level) * 2f);
    }
}
