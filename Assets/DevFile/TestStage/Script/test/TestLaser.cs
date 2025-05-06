using UnityEngine;
using UnityEngine.VFX;

public class TestLaser : MonoBehaviour
{
    public VisualEffect vfx;          // VFX Graph ������Ʈ
    public Transform origin;          // ������ ���� ����
    public float maxDistance = 100f;  // ������ �ִ� �Ÿ�
    public LayerMask hitLayers;       // �浹 ���� ���̾�

    public Transform target;

    private bool isFiring = false;

    // Update is called once per frame
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

            // 1. �Ÿ� ���
            distance = Vector3.Distance(transform.position, target.position);
            Debug.Log("Distance: " + distance);

            // 2. ���� ��� (���� ������Ʈ �� ��� ������Ʈ)
            direction = (target.position - origin.transform.position);
            //direction = handAimTarget.position;

            Vector3 TargetPos = origin.transform.InverseTransformPoint(target.position);

            // VFX Graph �Ķ���� ����
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
                vfx.Stop(); // VFX Graph ����
            }
        }
    }
}
