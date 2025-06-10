using UnityEngine;

public class SpotlightDamage : MonoBehaviour
{
    public float damageAmount = 10f; // ������ ����� ��
    public float coneAngle = 30f; // ������ ����
    public float coneRange = 10f; // ������ ����
    public LayerMask targetLayer; // Ÿ�� ���̾� ����
    public Vector3 coneStartOffset = Vector3.zero; // ���� ������ ������
    public Vector3 coneRotation = Vector3.zero; // ������ ȸ����
    public float coneRadiusMultiplier = 1.0f; // ���� �ظ� ���� ũ�⸦ ������ �� �ִ� ����

    private void Update()
    {
        // ���� ���� ���� ��ü�� �����Ͽ� ������� ����
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

    // ���� ������ Gizmos�� �ð������� ǥ��
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

        // ������ �ظ� ���� ����Ʈ ���
        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = -angle / 2 + angleStep * i;
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * (rotation * Vector3.forward) * range;
            basePoints[i] = coneStartPosition + direction;
        }

        // ������ ���� �׸���
        for (int i = 0; i < segments; i++)
        {
            Gizmos.DrawLine(coneStartPosition, basePoints[i]);
            Gizmos.DrawLine(basePoints[i], basePoints[i + 1]);
        }
        Gizmos.DrawLine(coneStartPosition, basePoints[segments]);

        // ������ �ظ� �׸���
        for (int i = 0; i < segments; i++)
        {
            Gizmos.DrawLine(basePoints[i], basePoints[i + 1]);
        }
        Gizmos.DrawLine(basePoints[segments], basePoints[0]);
    }
}
