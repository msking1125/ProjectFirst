using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Project
{

/// <summary>
/// ?덉씪嫄?諛쒖궗 而⑦듃濡ㅻ윭
/// </summary>
#if ODIN_INSPECTOR
    [HideMonoScript]
#endif
    public class Firerailgun : MonoBehaviour
    {
#if ODIN_INSPECTOR
        [Title("怨듯넻 ?ㅼ젙", TitleAlignment = TitleAlignments.Left)]
        [HorizontalGroup("怨듯넻", 0.5f)]
        [BoxGroup("怨듯넻/諛쒖궗")]
        [LabelText("諛쒖궗 吏??)]
        [Tooltip("?ъ궗泥닿? ?앹꽦???꾩튂")]
        [SceneObjectsOnly]
#endif
        [Header("怨듯넻 ?ㅼ젙")]
        public Transform firePoint;

#if ODIN_INSPECTOR
        [HorizontalGroup("怨듯넻", 0.5f)]
        [BoxGroup("怨듯넻/踰붿쐞")]
        [LabelText("?먯깋 踰붿쐞")]
        [PropertyRange(1f, 50f)]
        [SuffixLabel("m", true)]
        [Tooltip("???먯깋 踰붿쐞")]
#endif
        public float searchRange = 10f;

#if ODIN_INSPECTOR
        [BoxGroup("怨듯넻")]
        [LabelText("?꾩쭊 ?ㅽ봽??)]
        [Tooltip("諛쒖궗 ?꾩튂 ?꾩쭊 ?ㅽ봽??)]
        [PropertyRange(0f, 2f)]
#endif
        public float forwardOffset = 0.5f;

        // ?? ?됲? ????????????????????????????????????????????????????????????????
#if ODIN_INSPECTOR
        [Title("?됲? (湲곕낯 怨듦꺽)", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("?됲?")]
        [LabelText("?됲? ?꾨━??)]
        [AssetsOnly]
        [PreviewField(60, ObjectFieldAlignment.Left)]
#endif
        [Header("?됲? (湲곕낯 怨듦꺽)")]
        public GameObject railgunPrefab;

#if ODIN_INSPECTOR
        [HorizontalGroup("?됲??ㅼ젙", 0.5f)]
        [BoxGroup("?됲??ㅼ젙/?띾룄")]
        [LabelText("諛쒖궗 ?띾룄")]
        [PropertyRange(1f, 100f)]
        [SuffixLabel("m/s", true)]
#endif
        public float launchSpeed = 30f;

#if ODIN_INSPECTOR
        [HorizontalGroup("?됲??ㅼ젙", 0.5f)]
        [BoxGroup("?됲??ㅼ젙/?뚯쟾")]
        [LabelText("?뚯쟾 ?ㅽ봽??)]
        [Tooltip("?됲? ?댄럺???뚯쟾 ?ㅽ봽??)]
#endif
        public Vector3 normalRotationOffset = new Vector3(0, 90, 0);

#if ODIN_INSPECTOR
        [BoxGroup("?됲?")]
        [LabelText("吏???쒓컙")]
        [Tooltip("?됲? ?댄럺?멸? ?섏씠?쇳궎?먯꽌 吏?뚯????쒓컙(珥?")]
        [SuffixLabel("珥?, true)]
        [PropertyRange(0.1f, 10f)]
#endif
        public float normalDestroyTime = 2f;

        // ?? ?ㅽ궗 ????????????????????????????????????????????????????????????????
#if ODIN_INSPECTOR
        [Title("?ㅽ궗 (沅곴레湲?", TitleAlignment = TitleAlignments.Left)]
        [BoxGroup("?ㅽ궗")]
        [LabelText("?ㅽ궗 ?꾨━??)]
        [AssetsOnly]
        [PreviewField(60, ObjectFieldAlignment.Left)]
#endif
        [Header("?ㅽ궗 (沅곴레湲?")]
        public GameObject skillPrefab;

#if ODIN_INSPECTOR
        [HorizontalGroup("?ㅽ궗?ㅼ젙", 0.5f)]
        [BoxGroup("?ㅽ궗?ㅼ젙/?띾룄")]
        [LabelText("諛쒖궗 ?띾룄")]
        [PropertyRange(1f, 100f)]
        [SuffixLabel("m/s", true)]
#endif
        public float skillLaunchSpeed = 30f;

#if ODIN_INSPECTOR
        [HorizontalGroup("?ㅽ궗?ㅼ젙", 0.5f)]
        [BoxGroup("?ㅽ궗?ㅼ젙/?뚯쟾")]
        [LabelText("?뚯쟾 ?ㅽ봽??)]
        [Tooltip("?ㅽ궗 ?댄럺???뚯쟾 ?ㅽ봽??)]
#endif
        public Vector3 skillRotationOffset = new Vector3(90, 0, 0);

#if ODIN_INSPECTOR
        [BoxGroup("?ㅽ궗")]
        [LabelText("吏???쒓컙")]
        [Tooltip("?ㅽ궗 ?댄럺?멸? ?섏씠?쇳궎?먯꽌 吏?뚯????쒓컙(珥?")]
        [SuffixLabel("珥?, true)]
        [PropertyRange(0.1f, 10f)]
        [GUIColor(1f, 0.6f, 0.2f)]
#endif
        public float skillDestroyTime = 3f;

    // ?? ?됲? 諛쒖궗 ??
    public void FireRailgun()
    {
        SpawnProjectile(railgunPrefab, launchSpeed, normalRotationOffset, normalDestroyTime);
    }

    // ?? ?ㅽ궗 諛쒖궗 ??
    public void FireSkillRailgun()
    {
        SpawnProjectile(skillPrefab, skillLaunchSpeed, skillRotationOffset, skillDestroyTime);
    }

    // ?? 諛쒖궗 諛???젣 怨듯넻 濡쒖쭅 ??
    private void SpawnProjectile(GameObject prefab, float speed, Vector3 rotationOffset, float destroyTime)
    {
        if (prefab != null && firePoint != null)
        {
            Vector3 shootDirection = firePoint.forward;

            if (EnemyManager.Instance != null)
            {
                Enemy target = EnemyManager.Instance.GetClosest(transform.position, searchRange);
                if (target != null)
                {
                    // 紐ъ뒪?곗쓽 以묒떖(?댁쭩 ?꾩そ) 議곗?
                    Vector3 targetPos = target.transform.position + Vector3.up * 1f;
                    shootDirection = (targetPos - firePoint.position).normalized;
                }
            }

            Vector3 spawnPos = firePoint.position + (shootDirection * forwardOffset);

            Quaternion targetRotation = Quaternion.LookRotation(shootDirection);
            Quaternion correction = Quaternion.Euler(rotationOffset);

            GameObject projectile = Instantiate(prefab, spawnPos, targetRotation * correction);

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = shootDirection * speed;
            }

            // [?듭떖] ?ㅼ젙???쒓컙(destroyTime)??吏?섎㈃ ?대줎???꾨꼍?섍쾶 ?뚭눼?⑸땲??
            Destroy(projectile, destroyTime);
        }
        else
        {
            Debug.LogWarning("[Firerailgun] ?꾨━?뱀씠??FirePoint媛 鍮꾩뼱?덉뼱 諛쒖궗?????놁뒿?덈떎!");
        }
    }
}
} // namespace Project

