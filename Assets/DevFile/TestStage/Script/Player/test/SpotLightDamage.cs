using UnityEngine;

public class SpotlightDamage : MonoBehaviour
{
    public float damageAmount = 10f; // 적용할 대미지 양
    public float coneAngle = 30f; // 원뿔의 각도
    public float coneRange = 10f; // 원뿔의 범위
    public LayerMask targetLayer; // 타겟 레이어 설정
    public Vector3 coneStartOffset = Vector3.zero; // 원뿔 시작점 오프셋
    public Vector3 coneRotation = Vector3.zero; // 원뿔의 회전값
    public float coneRadiusMultiplier = 1.0f; // 원뿔 밑면 원의 크기를 조절할 수 있는 변수

    private void Update()
    {
        // 원뿔 영역 내의 객체를 감지하여 대미지를 적용
        DetectAndDamageTargets();
    }

    private void DetectAndDamageTargets()
    {
        Vector3 coneStartPosition = transform.position + coneStartOffset;
        Quaternion rotation = Quaternion.Euler(coneRotation) * transform.rotation;
        Collider[] targets = Physics.OverlapSphere(coneStartPosition, coneRange, targetLayer);
        foreach (Collider target in targets)
        {
            Vector3 directionToTarget = target.transform.position - coneStartPosition;
            float angleToTarget = Vector3.Angle(rotation * Vector3.forward, directionToTarget);

            if (angleToTarget <= coneAngle / 2)
            {
                RaycastHit hit;
                if (Physics.Raycast(coneStartPosition, directionToTarget, out hit, coneRange))
                {
                   /* if (hit.collider == target)
                    {
                        CurrentHealth health = target.GetComponent<CurrentHealth>();
                        if (health != null)
                        {
                            health.TakeDamage(damageAmount);
                        }
                    }*/
                }
            }
        }
    }

    // 원뿔 범위를 Gizmos로 시각적으로 표시
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 coneStartPosition = transform.position + coneStartOffset;
        Quaternion rotation = Quaternion.Euler(coneRotation) * transform.rotation;

        DrawCone(coneStartPosition, coneAngle, coneRange, rotation);
    }

    private void DrawCone(Vector3 coneStartPosition, float angle, float range, Quaternion rotation)
    {
        int segments = 50;
        float angleStep = angle / segments;
        Vector3[] basePoints = new Vector3[segments + 1];
        float coneRadius = Mathf.Tan(Mathf.Deg2Rad * angle / 2) * range * coneRadiusMultiplier;

        // 원뿔의 밑면 원형 포인트 계산
        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = -angle / 2 + angleStep * i;
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * (rotation * Vector3.forward) * range;
            basePoints[i] = coneStartPosition + direction;
        }

        // 원뿔의 옆면 그리기
        for (int i = 0; i < segments; i++)
        {
            Gizmos.DrawLine(coneStartPosition, basePoints[i]);
            Gizmos.DrawLine(basePoints[i], basePoints[i + 1]);
        }
        Gizmos.DrawLine(coneStartPosition, basePoints[segments]);

        // 원뿔의 밑면 그리기
        for (int i = 0; i < segments; i++)
        {
            Gizmos.DrawLine(basePoints[i], basePoints[i + 1]);
        }
        Gizmos.DrawLine(basePoints[segments], basePoints[0]);
    }
}
