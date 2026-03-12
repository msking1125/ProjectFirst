using System.Collections;
using UnityEngine;

namespace Project
{

/// <summary>
/// Agent??踰꾪봽/?붾쾭???④낵瑜??쇱떆?곸쑝濡??곸슜?섎뒗 而댄룷?뚰듃.
/// Agent ?ㅻ툕?앺듃???먮룞?쇰줈 異붽??⑸땲??
/// </summary>
    public class AgentBuffSystem : MonoBehaviour
    {
        private Project.Agent agent;

        // ?꾩옱 ?쒖꽦 踰꾪봽 肄붾（??(媛숈? ????щ컻????湲곗〈 ??뼱?곌린)
        private Coroutine atkBuffCoroutine;
        private Coroutine defBuffCoroutine;
        private Coroutine spdBuffCoroutine;

        // 踰꾪봽 ?섏튂 異붿쟻 (UI ?쒖떆??
        public float AtkBuffMultiplier { get; private set; } = 1f;
        public float DefBuffMultiplier { get; private set; } = 1f;
        public float SpdBuffMultiplier { get; private set; } = 1f;

        public event System.Action OnBuffChanged;

        private void Awake()
        {
            agent = GetComponent<Agent>();
        }

        // ?? 踰꾪봽 ?곸슜 API ?????????????????????????????????????????????????????????

        public void ApplyBuff(BuffStatType stat, float multiplier, float duration)
        {
            switch (stat)
            {
                case BuffStatType.AttackPower:
                    if (atkBuffCoroutine != null) StopCoroutine(atkBuffCoroutine);
                    atkBuffCoroutine = StartCoroutine(BuffRoutine(stat, multiplier, duration));
                    break;
                case BuffStatType.Defense:
                    if (defBuffCoroutine != null) StopCoroutine(defBuffCoroutine);
                    defBuffCoroutine = StartCoroutine(BuffRoutine(stat, multiplier, duration));
                    break;
                case BuffStatType.AttackSpeed:
                    if (spdBuffCoroutine != null) StopCoroutine(spdBuffCoroutine);
                    spdBuffCoroutine = StartCoroutine(BuffRoutine(stat, multiplier, duration));
                    break;
            }
        }

        private IEnumerator BuffRoutine(BuffStatType stat, float multiplier, float duration)
        {
            SetBuff(stat, 1f + multiplier);
            Debug.Log($"[AgentBuff] {stat} +{multiplier*100:F0}% 踰꾪봽 ?쒖옉 ({duration}s)");

            yield return new WaitForSecondsRealtime(duration);

            SetBuff(stat, 1f);
            Debug.Log($"[AgentBuff] {stat} 踰꾪봽 醫낅즺");
        }

        private void SetBuff(BuffStatType stat, float value)
        {
            switch (stat)
            {
                case BuffStatType.AttackPower: AtkBuffMultiplier = value; break;
                case BuffStatType.Defense:     DefBuffMultiplier = value; break;
                case BuffStatType.AttackSpeed: SpdBuffMultiplier = value; break;
            }
            OnBuffChanged?.Invoke();
        }

        /// <summary>踰꾪봽 諛곗쑉???곸슜???ㅼ젣 怨듦꺽??諛섑솚</summary>
        public float GetBuffedAttackPower()
        {
            if (agent == null) return 0f;
            return agent.AttackPower * AtkBuffMultiplier;
        }
        public bool HasAnyBuff => AtkBuffMultiplier > 1f || DefBuffMultiplier > 1f || SpdBuffMultiplier > 1f;
    }
} // namespace Project

