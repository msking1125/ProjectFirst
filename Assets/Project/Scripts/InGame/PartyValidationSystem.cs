using System;
using System.Collections.Generic;
using System.Linq;
using ProjectFirst.Data;
using UnityEngine;

namespace ProjectFirst.InGame
{
    /// <summary>
    /// 파티 구성 유효성을 검증하는 시스템
    /// </summary>
    [System.Serializable]
    public class PartyValidationResult
    {
        public bool IsValid;
        public string WarningMessage;
        public string ErrorMessage;
        public List<string> Suggestions = new List<string>();
        
        public bool HasWarning => !string.IsNullOrEmpty(WarningMessage);
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    }
    
    [System.Serializable]
    public class PartyRecommendation
    {
        public int AgentId;
        public float Score;
        public string Reason;
        public ElementType ElementAdvantage;
    }
    
    public class PartyValidationSystem : MonoBehaviour
    {
        [Header("Validation Settings")]
        [SerializeField] private int minPartySize = 1;
        [SerializeField] private float powerThresholdRatio = 0.8f; // 추천 전투력의 80% 미만이면 경고
        [SerializeField] private bool enableElementValidation = true;
        [SerializeField] private bool enableDuplicationCheck = true;
        
        [Header("Data References")]
        [SerializeField] private AgentTable agentTable;
        [SerializeField] private StageData stageData;
        [SerializeField] private PlayerData playerData;
        
        /// <summary>
        /// 파티 유효성 검증
        /// </summary>
        public PartyValidationResult ValidateParty(int[] selectedAgentIds, StageData.StageInfo targetStage)
        {
            var result = new PartyValidationResult { IsValid = true };
            
            // 기본 검증
            ValidateBasicRequirements(selectedAgentIds, result);
            
            // 전투력 검증
            ValidateCombatPower(selectedAgentIds, targetStage, result);
            
            // 속성 검증
            if (enableElementValidation)
                ValidateElementComposition(selectedAgentIds, targetStage, result);
            
            // 중복 검증
            if (enableDuplicationCheck)
                ValidateDuplication(selectedAgentIds, result);
            
            // 캐릭터 상태 검증
            ValidateCharacterStatus(selectedAgentIds, result);
            
            // 추천 파티 제안
            GenerateSuggestions(selectedAgentIds, targetStage, result);
            
            return result;
        }
        
        /// <summary>
        /// 기본 요구사항 검증
        /// </summary>
        private void ValidateBasicRequirements(int[] selectedAgentIds, PartyValidationResult result)
        {
            int validCount = selectedAgentIds.Count(id => id > 0);
            
            if (validCount < minPartySize)
            {
                result.IsValid = false;
                result.ErrorMessage = $"최소 {minPartySize}명의 캐릭터가 필요합니다.";
                return;
            }
            
            if (validCount == 0)
            {
                result.IsValid = false;
                result.ErrorMessage = "캐릭터를 선택해주세요.";
                return;
            }
        }
        
        /// <summary>
        /// 전투력 검증
        /// </summary>
        private void ValidateCombatPower(int[] selectedAgentIds, StageData.StageInfo targetStage, PartyValidationResult result)
        {
            int totalPower = CalculatePartyPower(selectedAgentIds);
            int recommendedPower = targetStage?.recommendedPower ?? 0;
            
            if (recommendedPower > 0)
            {
                float powerRatio = (float)totalPower / recommendedPower;
                
                if (powerRatio < powerThresholdRatio)
                {
                    result.WarningMessage = $"파티 전투력이 추천치의 {(powerRatio * 100):F0}%입니다. " +
                                          "전투가 어려울 수 있습니다.";
                }
            }
        }
        
        /// <summary>
        /// 속성 구성 검증
        /// </summary>
        private void ValidateElementComposition(int[] selectedAgentIds, StageData.StageInfo targetStage, PartyValidationResult result)
        {
            if (targetStage == null || agentTable == null) return;
            
            ElementType enemyElement = targetStage.enemyElement;
            var partyElements = GetPartyElements(selectedAgentIds);
            
            // 적에게 불리한 속성만 있는 경우 경고
            bool hasAdvantage = false;
            foreach (ElementType partyElement in partyElements)
            {
                if (ElementTypeHelper.HasAdvantage(partyElement, enemyElement))
                {
                    hasAdvantage = true;
                    break;
                }
            }
            
            if (!hasAdvantage && partyElements.Count > 0)
            {
                result.WarningMessage = $"적의 {enemyElement} 속성에 불리한 파티 구성입니다. " +
                                      "속성 우위 캐릭터를 포함하는 것을 추천합니다.";
            }
        }
        
        /// <summary>
        /// 중복 검증
        /// </summary>
        private void ValidateDuplication(int[] selectedAgentIds, PartyValidationResult result)
        {
            var duplicates = selectedAgentIds
                .Where(id => id > 0)
                .GroupBy(id => id)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);
            
            foreach (int duplicateId in duplicates)
            {
                var agentInfo = agentTable?.GetAgentInfo(duplicateId);
                string agentName = agentInfo?.agentName ?? $"#{duplicateId}";
                
                result.ErrorMessage = $"{agentName} 캐릭터가 중복되었습니다.";
                result.IsValid = false;
                return;
            }
        }
        
        /// <summary>
        /// 캐릭터 상태 검증
        /// </summary>
        private void ValidateCharacterStatus(int[] selectedAgentIds, PartyValidationResult result)
        {
            foreach (int agentId in selectedAgentIds)
            {
                if (agentId <= 0) continue;
                
                var agentInfo = agentTable?.GetAgentInfo(agentId);
                if (agentInfo == null)
                {
                    result.ErrorMessage = $"유효하지 않은 캐릭터(ID: {agentId})가 포함되어 있습니다.";
                    result.IsValid = false;
                    return;
                }
                
                // 레벨 검증
                int level = GetCharacterLevel(agentId);
                if (level <= 0)
                {
                    result.WarningMessage = $"{agentInfo.agentName} 캐릭터의 레벨이 유효하지 않습니다.";
                }
                
                // 추가 상태 검증 (피로, 부상 등)
                if (playerData != null)
                {
                    // TODO: 캐릭터 상태 시스템 구현 시 추가
                    // if (playerData.IsCharacterFatigued(agentId))
                    //     result.WarningMessage = $"{agentInfo.agentName} 캐릭터가 피로 상태입니다.";
                }
            }
        }
        
        /// <summary>
        /// 파티 추천 사항 생성
        /// </summary>
        private void GenerateSuggestions(int[] selectedAgentIds, StageData.StageInfo targetStage, PartyValidationResult result)
        {
            if (targetStage == null || agentTable == null || playerData == null) return;
            
            var recommendations = GetRecommendedCharacters(selectedAgentIds, targetStage);
            
            foreach (var recommendation in recommendations.Take(3))
            {
                var agentInfo = agentTable.GetAgentInfo(recommendation.AgentId);
                if (agentInfo != null)
                {
                    string suggestion = $"{agentInfo.agentName}: {recommendation.Reason}";
                    result.Suggestions.Add(suggestion);
                }
            }
        }
        
        /// <summary>
        /// 추천 캐릭터 목록 가져오기
        /// </summary>
        public List<PartyRecommendation> GetRecommendedCharacters(int[] currentParty, StageData.StageInfo targetStage)
        {
            var recommendations = new List<PartyRecommendation>();
            
            if (targetStage == null || agentTable == null || playerData == null) 
                return recommendations;
            
            var currentPartySet = new HashSet<int>(currentParty.Where(id => id > 0));
            var ownedCharacters = playerData.ownedCharacterIds
                .Except(currentPartySet)
                .ToList();
            
            ElementType enemyElement = targetStage.enemyElement;
            int recommendedPower = targetStage.recommendedPower;
            int currentPartyPower = CalculatePartyPower(currentParty);
            
            foreach (int agentId in ownedCharacters)
            {
                var agentInfo = agentTable.GetAgentInfo(agentId);
                if (agentInfo == null) continue;
                
                int level = GetCharacterLevel(agentId);
                int agentPower = agentInfo.GetPower(level);
                
                float score = CalculateRecommendationScore(agentInfo, level, enemyElement, 
                                                         recommendedPower, currentPartyPower);
                
                string reason = GetRecommendationReason(agentInfo, level, enemyElement, 
                                                       recommendedPower, currentPartyPower);
                
                recommendations.Add(new PartyRecommendation
                {
                    AgentId = agentId,
                    Score = score,
                    Reason = reason,
                    ElementAdvantage = agentInfo.element
                });
            }
            
            return recommendations.OrderByDescending(r => r.Score).ToList();
        }
        
        /// <summary>
        /// 추천 점수 계산
        /// </summary>
        private float CalculateRecommendationScore(AgentInfo agentInfo, int level, 
                                                  ElementType enemyElement, int recommendedPower, 
                                                  int currentPartyPower)
        {
            float score = 0f;
            int agentPower = agentInfo.GetPower(level);
            
            // 전투력 기여도
            float powerContribution = (float)agentPower / Math.Max(1, recommendedPower);
            score += powerContribution * 40f;
            
            // 속성 우위 보너스
            if (ElementTypeHelper.HasAdvantage(agentInfo.element, enemyElement))
            {
                score += 30f;
            }
            else if (ElementTypeHelper.HasAdvantage(enemyElement, agentInfo.element))
            {
                score -= 20f;
            }
            
            // 파티 균형 보너스
            if (currentPartyPower < recommendedPower)
            {
                score += powerContribution * 20f; // 부족할 때 더 높은 가중치
            }
            
            // 등급 보너스
            score += agentInfo.grade * 5f;
            
            return Math.Max(0f, score);
        }
        
        /// <summary>
        /// 추천 이유 생성
        /// </summary>
        private string GetRecommendationReason(AgentInfo agentInfo, int level, ElementType enemyElement, 
                                              int recommendedPower, int currentPartyPower)
        {
            int agentPower = agentInfo.GetPower(level);
            var reasons = new List<string>();
            
            // 속성 우위
            if (ElementTypeHelper.HasAdvantage(agentInfo.element, enemyElement))
            {
                reasons.Add($"{enemyElement} 속성에 우위");
            }
            
            // 전투력 기여
            if (currentPartyPower < recommendedPower)
            {
                int neededPower = recommendedPower - currentPartyPower;
                if (agentPower >= neededPower)
                {
                    reasons.Add("추천 전투력 달성 가능");
                }
                else
                {
                    reasons.Add($"전투력 +{agentPower:N0}");
                }
            }
            
            // 등급
            if (agentInfo.grade >= 4)
            {
                reasons.Add("고등급 캐릭터");
            }
            
            return reasons.Count > 0 ? string.Join(", ", reasons) : "균형 잡힌 선택";
        }
        
        /// <summary>
        /// 파티 전투력 계산
        /// </summary>
        public int CalculatePartyPower(int[] selectedAgentIds)
        {
            int totalPower = 0;
            
            foreach (int agentId in selectedAgentIds)
            {
                if (agentId <= 0) continue;
                
                var agentInfo = agentTable?.GetAgentInfo(agentId);
                if (agentInfo != null)
                {
                    int level = GetCharacterLevel(agentId);
                    totalPower += agentInfo.GetPower(level);
                }
            }
            
            return totalPower;
        }
        
        /// <summary>
        /// 파티 속성 목록 가져오기
        /// </summary>
        private List<ElementType> GetPartyElements(int[] selectedAgentIds)
        {
            var elements = new List<ElementType>();
            
            foreach (int agentId in selectedAgentIds)
            {
                if (agentId <= 0) continue;
                
                var agentInfo = agentTable?.GetAgentInfo(agentId);
                if (agentInfo != null && !elements.Contains(agentInfo.element))
                {
                    elements.Add(agentInfo.element);
                }
            }
            
            return elements;
        }
        
        /// <summary>
        /// 캐릭터 레벨 가져오기
        /// </summary>
        private int GetCharacterLevel(int agentId)
        {
            return playerData?.GetCharacterLevel(agentId) ?? 1;
        }
        
        /// <summary>
        /// 자동 파티 구성 (개선된 버전)
        /// </summary>
        public int[] GetAutoPartyComposition(StageData.StageInfo targetStage, int partySize = 4)
        {
            if (targetStage == null || agentTable == null || playerData == null)
            {
                int[] emptyParty = new int[partySize];
                for (int i = 0; i < partySize; i++) emptyParty[i] = -1;
                return emptyParty;
            }
            
            int[] currentParty = new int[partySize];
            for (int i = 0; i < partySize; i++) currentParty[i] = -1;
            
            var recommendations = GetRecommendedCharacters(currentParty, targetStage);
            var autoParty = new int[partySize];
            
            for (int i = 0; i < partySize; i++)
            {
                if (i < recommendations.Count) autoParty[i] = recommendations[i].AgentId;
                else autoParty[i] = -1;
            }
            
            return autoParty;
        }
    }
}
