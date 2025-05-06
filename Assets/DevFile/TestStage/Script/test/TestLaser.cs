using UnityEngine;
using UnityEngine.VFX;

public class TestLaser : MonoBehaviour
{
    public VisualEffect vfx;          // VFX Graph 컴포넌트
    public Transform origin;          // 레이저 시작 지점
    public float maxDistance = 100f;  // 레이저 최대 거리
    public LayerMask hitLayers;       // 충돌 감지 레이어

    public Transform target;

    private bool isFiring = false;

    // Update is called once per frame
    void Update()
    {

        // 마우스 좌클릭 중일 때만 발사
        if (Input.GetMouseButton(0))
        {
            if (!isFiring)
            {
                isFiring = true;
                vfx.Reinit();
                vfx.SetBool("isFiring", isFiring);
                vfx.Play(); // VFX Graph 시작
            }

            Vector3 direction = origin.forward;
            float distance = maxDistance;

            // 1. 거리 계산
            distance = Vector3.Distance(transform.position, target.position);
            Debug.Log("Distance: " + distance);

            // 2. 방향 계산 (현재 오브젝트 → 대상 오브젝트)
            direction = (target.position - origin.transform.position);
            //direction = handAimTarget.position;

            Vector3 TargetPos = origin.transform.InverseTransformPoint(target.position);

            // VFX Graph 파라미터 설정
            vfx.SetVector3("Direction", direction.normalized);
            vfx.SetFloat("Length", distance);
            vfx.SetVector3("TargetPos", TargetPos);
        }
        else
        {
            if (isFiring)
            {
                isFiring = false;
                vfx.SetBool("isFiring", isFiring);
                vfx.Stop(); // VFX Graph 정지
            }
        }
    }
}
