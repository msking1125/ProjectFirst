using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirst.Data
{
    [Serializable]
    public class ExpItemDefinition
    {
        public ExpItemType type;
        public string itemName;
        public Sprite icon;
    }

    [CreateAssetMenu(menuName = "Game/Mission Catalog", fileName = "MissionCatalog")]
    public class MissionCatalogSO : ScriptableObject
    {
        public List<MissionEntry> missions = new();
        public List<PointRewardTier> pointRewardTiers = new();
    }

    [CreateAssetMenu(menuName = "Game/Mail Catalog", fileName = "MailCatalog")]
    public class MailCatalogSO : ScriptableObject
    {
        public List<MailData> mockMails = new();
    }

    [CreateAssetMenu(menuName = "Game/Character Growth Catalog", fileName = "CharacterGrowthCatalog")]
    public class CharacterGrowthCatalogSO : ScriptableObject
    {
        public List<ExpItemDefinition> expItems = new();
    }
}

