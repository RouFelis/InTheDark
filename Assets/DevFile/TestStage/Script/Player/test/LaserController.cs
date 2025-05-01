using UnityEngine;
using UnityEngine.VFX;

public class LaserController : MonoBehaviour
{
    public VisualEffect vfx;          // VFX Graph ������Ʈ
    public Transform origin;          // ������ ���� ����
    public float maxDistance = 100f;  // ������ �ִ� �Ÿ�
    public LayerMask hitLayers;       // �浹 ���� ���̾�

    private bool isFiring = false;

    void Update()
    {
        // ���콺 ��Ŭ�� ���� ���� �߻�
        if (Input.GetMouseButton(0))
        {
            if (!isFiring)
            {
                isFiring = true;
                vfx.Reinit();
                vfx.SetBool("isFiring", isFiring);
                vfx.Play(); // VFX Graph ����
            }

            Vector3 direction = origin.forward;
            float distance = maxDistance;

            if (Physics.Raycast(origin.position, direction, out RaycastHit hit, maxDistance, hitLayers))
            {
                distance = hit.distance;
            }

            // VFX Graph �Ķ���� ����
            vfx.SetVector3("Direction", direction);
            vfx.SetFloat("Length", distance);
        }
        else
        {
            if (isFiring)
            {
                isFiring = false;
                vfx.SetBool("isFiring", isFiring);
                vfx.Stop(); // VFX Graph ����
            }
        }
    }
}
