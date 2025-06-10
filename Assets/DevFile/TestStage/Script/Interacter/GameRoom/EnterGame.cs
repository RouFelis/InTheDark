using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class EnterGame : InteractableObject
{
    [Header("Door Settings")]
    [SerializeField] private Transform leftDoorAxis;
    [SerializeField] private Transform rightDoorAxis;
    [SerializeField] private float doorOpenAngle = 90f;
    [SerializeField] private float doorAnimationSpeed = 2f;
    [SerializeField] private AudioSource doorSound;
    [SerializeField] private BoxCollider doorCollider;

    private Coroutine doorAnimationCoroutine;
    private NetworkVariable<bool> doorState = new NetworkVariable<bool>(false); // false: closed, true: open

    private Dictionary<ulong, string> clientLoadedScenes = new Dictionary<ulong, string>();
    private Dictionary<string, int> sceneRefCount = new Dictionary<string, int>();

    public override void Interact(ulong userID, Transform interactingObjectTransform)
    {
        base.Interact(userID, interactingObjectTransform);
        RequestSceneChangeServerRpc("TestScene");
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSceneChangeServerRpc(string sceneName, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        // 1. 이전 씬 언로드
        if (clientLoadedScenes.TryGetValue(clientId, out string prevScene))
        {
            UnloadSceneForClient(prevScene);
        }

        // 2. 씬 로드 및 참조 관리
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

        clientLoadedScenes[clientId] = sceneName;

        if (!sceneRefCount.ContainsKey(sceneName))
            sceneRefCount[sceneName] = 0;
        sceneRefCount[sceneName]++;

        // 3. 문 연출 처리
        doorCollider.enabled = false;
        RequestSceneChangeClientRpc(sceneName);

        if (doorAnimationCoroutine == null)
        {
            doorAnimationCoroutine = StartCoroutine(AnimateDoorsWithSound(!doorState.Value));
            doorState.Value = !doorState.Value;
        }
    }

    [ClientRpc]
    private void RequestSceneChangeClientRpc(string sceneName)
    {
        doorCollider.enabled = false;
    }

    public void OnPlayerDeath(ulong clientId)
    {
        if (IsServer && clientLoadedScenes.TryGetValue(clientId, out string sceneName))
        {
            UnloadSceneForClient(sceneName);
        }
    }

    private void UnloadSceneForClient(string sceneName)
    {
        if (sceneRefCount.ContainsKey(sceneName))
        {
            sceneRefCount[sceneName]--;

            if (sceneRefCount[sceneName] <= 0)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
                if (scene.IsValid())
                {
                    NetworkManager.Singleton.SceneManager.UnloadScene(scene);
                }

                sceneRefCount.Remove(sceneName);
            }
        }
    }

    private IEnumerator AnimateDoorsWithSound(bool open)
    {
        doorSound.Play();
        float animationDuration = doorSound.clip.length;

        Quaternion leftStart = leftDoorAxis.localRotation;
        Quaternion rightStart = rightDoorAxis.localRotation;

        Quaternion leftTarget = open ? Quaternion.Euler(0, doorOpenAngle, 0) : Quaternion.identity;
        Quaternion rightTarget = open ? Quaternion.Euler(0, -doorOpenAngle, 0) : Quaternion.identity;

        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration);

            leftDoorAxis.localRotation = Quaternion.Slerp(leftStart, leftTarget, t);
            rightDoorAxis.localRotation = Quaternion.Slerp(rightStart, rightTarget, t);
            yield return null;
        }

        leftDoorAxis.localRotation = leftTarget;
        rightDoorAxis.localRotation = rightTarget;

        doorAnimationCoroutine = null;
    }


}
