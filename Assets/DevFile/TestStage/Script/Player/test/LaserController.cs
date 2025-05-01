using UnityEngine;
using UnityEngine.VFX;

public class LaserController : MonoBehaviour
{
    public VisualEffect vfx;          // VFX Graph 컴포넌트
    public Transform origin;          // 레이저 시작 지점
    public float maxDistance = 100f;  // 레이저 최대 거리
    public LayerMask hitLayers;       // 충돌 감지 레이어

    private bool isFiring = false;

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

            if (Physics.Raycast(origin.position, direction, out RaycastHit hit, maxDistance, hitLayers))
            {
                distance = hit.distance;
            }

            // VFX Graph 파라미터 설정
            vfx.SetVector3("Direction", direction);
            vfx.SetFloat("Length", distance);
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
