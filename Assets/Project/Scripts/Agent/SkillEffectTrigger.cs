using UnityEngine;
using Project;

// �� ��ũ��Ʈ�� MonoBehaviour�� �ƴ϶� StateMachineBehaviour�� ��ӹ޽��ϴ�!
public class SkillEffectTrigger : StateMachineBehaviour
{
    [Tooltip("��ų �ִϸ��̼��� ���۵ǰ� �� % ������ �߻����� ���� (0 = ���� ���, 0.5 = �߰�)")]
    [Range(0f, 1f)] public float triggerTime = 0f;

    private bool hasFired = false;

    // ����(�ִϸ��̼�)�� ������ �� 1ȸ ����
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasFired = false;

        // 0�� ��� �ߵ� �����̶�� �ٷ� �߻�
        if (triggerTime <= 0f)
        {
            Fire(animator);
        }
    }

    // ����(�ִϸ��̼�)�� ����Ǵ� ���� �� ������ ����
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // ������ �ð��� ������, ���� �� ���ٸ� �߻�
        if (!hasFired && triggerTime > 0f && stateInfo.normalizedTime >= triggerTime)
        {
            Fire(animator);
        }
    }

    private void Fire(Animator animator)
    {
        // Firerailgun ��ũ��Ʈ�� ã�Ƽ� ��ų �߻� �Լ� ����!
        Firerailgun railgun = animator.GetComponent<Firerailgun>();
        if (railgun != null)
        {
            railgun.FireSkillRailgun();
        }
        hasFired = true;
    }
}
