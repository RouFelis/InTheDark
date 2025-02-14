using UnityEngine;
using Cinemachine;
using System.Collections;

public class SparkLookCam : MonoBehaviour
{
    [SerializeField]private CinemachineBrain cinemachineBrain;

    void Start()
    {
        // 씬에 있는 CinemachineBrain을 찾음 (주로 메인 카메라에 부착됨)
        StartCoroutine(FindCinemachineBrain());
    }


    IEnumerator FindCinemachineBrain()
    {
        while (cinemachineBrain == null)
        {
            cinemachineBrain = FindAnyObjectByType<CinemachineBrain>();
            if (cinemachineBrain == null)
            {
                Debug.Log("찾아보기");
                yield return new WaitForSeconds(1f); // 1초마다 다시 시도
            }
        }
    }


    void Update()
    {
        if (cinemachineBrain != null && cinemachineBrain.ActiveVirtualCamera != null)
        {
            // 현재 활성화된 시네머신 가상 카메라의 Transform 가져오기
            CinemachineVirtualCamera virtualCamera = cinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCamera;
            if (virtualCamera != null)
            {
                Transform cameraTransform = virtualCamera.transform;

                // 카메라의 방향을 바라보도록 회전 (Y축 회전만 적용하고 싶다면 Vector3.up을 사용)
                transform.rotation = Quaternion.LookRotation(cameraTransform.forward);
            }
        }
    }
}
