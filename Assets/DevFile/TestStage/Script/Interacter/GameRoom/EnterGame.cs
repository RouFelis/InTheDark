using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class EnterGame : InteractableObject
{
    public Transform leftDoorAxis; // 왼쪽 문 축 (Inspector에서 설정)
    public Transform rightDoorAxis; // 오른쪽 문 축 (Inspector에서 설정)
    public BoxCollider colider;
    public float doorOpenAngle = 90f; // 문이 열릴 각도
    public float doorAnimationSpeed = 2f; // 문 애니메이션 속도

    private GameObject spawnPoint;
    public AudioSource doorSound; // 문 소리 

    private NetworkVariable<bool> doorState = new NetworkVariable<bool>(false); // 문 상태 (false: 닫힘, true: 열림)
    private Coroutine doorAnimationCoroutine;


    public override void Interact(ulong uerID ,Transform interactingObjectTransform)
    {
        base.Interact(uerID , interactingObjectTransform);
        // 서버에서 씬 전환

        if (doorAnimationCoroutine == null)
        {
            doorAnimationCoroutine = StartCoroutine(AnimateDoorsWithSound(!doorState.Value));
        }
        RequestSceneChangeServerRpc("TestScene");
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSceneChangeServerRpc(string sceneName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        colider.enabled = false;
        RequestSceneChangeClientRpc();
    }

    [ClientRpc]
    private void RequestSceneChangeClientRpc()
    {
        colider.enabled = false;
    }



    // 문 애니메이션과 사운드 종료를 동기화하는 코루틴
    private IEnumerator AnimateDoorsWithSound(bool open)
    {
        // 사운드 재생
        doorSound.Play();

        // 사운드 길이 가져오기
        float animationDuration = doorSound.clip.length;

        // 초기 및 목표 회전값 설정
        Quaternion leftDoorStartRotation = leftDoorAxis.localRotation;
        Quaternion rightDoorStartRotation = rightDoorAxis.localRotation;

        Quaternion leftDoorTargetRotation = open
            ? Quaternion.Euler(0, doorOpenAngle, 0)
            : Quaternion.identity;

        Quaternion rightDoorTargetRotation = open
            ? Quaternion.Euler(0, -doorOpenAngle, 0)
            : Quaternion.identity;

        // 애니메이션 실행
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration); // 0~1 사이의 비율 계산
            leftDoorAxis.localRotation = Quaternion.Slerp(leftDoorStartRotation, leftDoorTargetRotation, t);
            rightDoorAxis.localRotation = Quaternion.Slerp(rightDoorStartRotation, rightDoorTargetRotation, t);
            yield return null;
        }

        // 최종적으로 목표 회전값에 도달
        leftDoorAxis.localRotation = leftDoorTargetRotation;
        rightDoorAxis.localRotation = rightDoorTargetRotation;

        // 문 상태 업데이트
        doorState.Value = open;

        // 코루틴 해제
        doorAnimationCoroutine = null;
    }
}
