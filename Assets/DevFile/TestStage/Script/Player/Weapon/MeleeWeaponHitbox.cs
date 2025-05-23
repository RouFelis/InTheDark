using UnityEngine;
using System.Collections.Generic;


public class MeleeWeaponHitbox : MonoBehaviour
{
    public int damage = 10;
    public Camera camera;
    public Vector3 boxSize = new Vector3(1f, 1f, 2f); // ��Ʈ�ڽ� ũ�� (��, ����, ����)
    public float boxDistance = 1.5f;                  // ī�޶� �������� �Ÿ�
    public LayerMask enemyLayer;

    private List<Collider> alreadyHit = new List<Collider>();
    
    public void ApplyDamage()
    {
        alreadyHit.Clear();
        // ī�޶� ���� ��ġ ���
        Vector3 center = camera.transform.position + camera.transform.forward * boxDistance;

        // ȸ���� ī�޶� �������� ����
        Quaternion rotation = camera.transform.rotation;

        // �ڽ� ������
        Collider[] hits = Physics.OverlapBox(center, boxSize * 0.5f, rotation, enemyLayer);


        Debug.Log("�׽�Ʈ: ����");

        foreach (Collider hit in hits)
        {
            if (!alreadyHit.Contains(hit))
            {
                Debug.Log("�׽�Ʈ: Ÿ�ݵ�");
                alreadyHit.Add(hit);
                hit.GetComponent<EnemyPrototypePawn>()?.TakeDamage(damage, null);
            }
        }
    }

    // ����׿� ��Ʈ�ڽ� �ð�ȭ
    private void OnDrawGizmosSelected()
    {
        if (camera == null) return;

        Gizmos.color = Color.red;
        Vector3 center = camera.transform.position + camera.transform.forward * boxDistance;
        Gizmos.matrix = Matrix4x4.TRS(center, camera.transform.rotation, boxSize);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }

}
