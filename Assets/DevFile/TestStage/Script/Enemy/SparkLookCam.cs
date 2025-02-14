using UnityEngine;
using Cinemachine;
using System.Collections;

public class SparkLookCam : MonoBehaviour
{
    [SerializeField]private CinemachineBrain cinemachineBrain;

    void Start()
    {
        // ���� �ִ� CinemachineBrain�� ã�� (�ַ� ���� ī�޶� ������)
        StartCoroutine(FindCinemachineBrain());
    }


    IEnumerator FindCinemachineBrain()
    {
        while (cinemachineBrain == null)
        {
            cinemachineBrain = FindAnyObjectByType<CinemachineBrain>();
            if (cinemachineBrain == null)
            {
                Debug.Log("ã�ƺ���");
                yield return new WaitForSeconds(1f); // 1�ʸ��� �ٽ� �õ�
            }
        }
    }


    void Update()
    {
        if (cinemachineBrain != null && cinemachineBrain.ActiveVirtualCamera != null)
        {
            // ���� Ȱ��ȭ�� �ó׸ӽ� ���� ī�޶��� Transform ��������
            CinemachineVirtualCamera virtualCamera = cinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCamera;
            if (virtualCamera != null)
            {
                Transform cameraTransform = virtualCamera.transform;

                // ī�޶��� ������ �ٶ󺸵��� ȸ�� (Y�� ȸ���� �����ϰ� �ʹٸ� Vector3.up�� ���)
                transform.rotation = Quaternion.LookRotation(cameraTransform.forward);
            }
        }
    }
}
