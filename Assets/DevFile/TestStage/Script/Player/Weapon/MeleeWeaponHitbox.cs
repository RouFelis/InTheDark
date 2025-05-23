using UnityEngine;
using System.Collections.Generic;


public class MeleeWeaponHitbox : MonoBehaviour
{
    public int damage = 10;
    public Camera camera;
    public Vector3 boxSize = new Vector3(1f, 1f, 2f); // 히트박스 크기 (폭, 높이, 깊이)
    public float boxDistance = 1.5f;                  // 카메라 앞으로의 거리
    public LayerMask enemyLayer;

    private List<Collider> alreadyHit = new List<Collider>();
    
    public void ApplyDamage()
    {
        alreadyHit.Clear();
        // 카메라 기준 위치 계산
        Vector3 center = camera.transform.position + camera.transform.forward * boxDistance;

        // 회전은 카메라 기준으로 설정
        Quaternion rotation = camera.transform.rotation;

        // 박스 오버랩
        Collider[] hits = Physics.OverlapBox(center, boxSize * 0.5f, rotation, enemyLayer);


        Debug.Log("테스트: 실행");

        foreach (Collider hit in hits)
        {
            if (!alreadyHit.Contains(hit))
            {
                Debug.Log("테스트: 타격됨");
                alreadyHit.Add(hit);
                hit.GetComponent<EnemyPrototypePawn>()?.TakeDamage(damage, null);
            }
        }
    }

    // 디버그용 히트박스 시각화
    private void OnDrawGizmosSelected()
    {
        if (camera == null) return;

        Gizmos.color = Color.red;
        Vector3 center = camera.transform.position + camera.transform.forward * boxDistance;
        Gizmos.matrix = Matrix4x4.TRS(center, camera.transform.rotation, boxSize);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }

}
