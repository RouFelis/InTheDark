using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class EnterGame : InteractableObject
{
    public Transform leftDoorAxis; // ���� �� �� (Inspector���� ����)
    public Transform rightDoorAxis; // ������ �� �� (Inspector���� ����)
    public BoxCollider colider;
    public float doorOpenAngle = 90f; // ���� ���� ����
    public float doorAnimationSpeed = 2f; // �� �ִϸ��̼� �ӵ�

    private GameObject spawnPoint;
    public AudioSource doorSound; // �� �Ҹ� 

    private NetworkVariable<bool> doorState = new NetworkVariable<bool>(false); // �� ���� (false: ����, true: ����)
    private Coroutine doorAnimationCoroutine;


    public override void Interact(ulong uerID ,Transform interactingObjectTransform)
    {
        base.Interact(uerID , interactingObjectTransform);
        // �������� �� ��ȯ

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



    // �� �ִϸ��̼ǰ� ���� ���Ḧ ����ȭ�ϴ� �ڷ�ƾ
    private IEnumerator AnimateDoorsWithSound(bool open)
    {
        // ���� ���
        doorSound.Play();

        // ���� ���� ��������
        float animationDuration = doorSound.clip.length;

        // �ʱ� �� ��ǥ ȸ���� ����
        Quaternion leftDoorStartRotation = leftDoorAxis.localRotation;
        Quaternion rightDoorStartRotation = rightDoorAxis.localRotation;

        Quaternion leftDoorTargetRotation = open
            ? Quaternion.Euler(0, doorOpenAngle, 0)
            : Quaternion.identity;

        Quaternion rightDoorTargetRotation = open
            ? Quaternion.Euler(0, -doorOpenAngle, 0)
            : Quaternion.identity;

        // �ִϸ��̼� ����
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration); // 0~1 ������ ���� ���
            leftDoorAxis.localRotation = Quaternion.Slerp(leftDoorStartRotation, leftDoorTargetRotation, t);
            rightDoorAxis.localRotation = Quaternion.Slerp(rightDoorStartRotation, rightDoorTargetRotation, t);
            yield return null;
        }

        // ���������� ��ǥ ȸ������ ����
        leftDoorAxis.localRotation = leftDoorTargetRotation;
        rightDoorAxis.localRotation = rightDoorTargetRotation;

        // �� ���� ������Ʈ
        doorState.Value = open;

        // �ڷ�ƾ ����
        doorAnimationCoroutine = null;
    }
}
