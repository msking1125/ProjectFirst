using System.Collections;
using UnityEngine;

namespace Project
{

/// <summary>
/// Documentation cleaned.
/// Documentation cleaned.
/// </summary>
    public class AgentBuffSystem : MonoBehaviour
    {
        private Project.Agent agent;

        // Note: cleaned comment.
        private Coroutine atkBuffCoroutine;
        private Coroutine defBuffCoroutine;
        private Coroutine spdBuffCoroutine;

        // Note: cleaned comment.
        public float AtkBuffMultiplier { get; private set; } = 1f;
        public float DefBuffMultiplier { get; private set; } = 1f;
        public float SpdBuffMultiplier { get; private set; } = 1f;

        public event System.Action OnBuffChanged;

        private void Awake()
        {
            agent = GetComponent<Agent>();
        }

        // Note: cleaned comment.

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
            Debug.Log("[Log] Message cleaned.");

            yield return new WaitForSecondsRealtime(duration);

            SetBuff(stat, 1f);
            Debug.Log("[Log] Message cleaned.");
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

        /// Documentation cleaned.
        public float GetBuffedAttackPower()
        {
            if (agent == null) return 0f;
            return agent.AttackPower * AtkBuffMultiplier;
        }
        public bool HasAnyBuff => AtkBuffMultiplier > 1f || DefBuffMultiplier > 1f || SpdBuffMultiplier > 1f;
    }
} // namespace Project

