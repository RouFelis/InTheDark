using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class GameManagerAndInteractor : InteractableObject
{
    [Header("Door Settings")]
    [SerializeField] private Transform leftDoorAxis;
    [SerializeField] private Transform rightDoorAxis;
    [SerializeField] private Vector3 originalPosition_R;
    [SerializeField] private Vector3 originalPosition_L;
    [SerializeField] private Vector3 targetPosition_R;
    [SerializeField] private Vector3 targetPosition_L;
    [SerializeField] private AudioSource doorSound;
    [SerializeField] private Animator buttonAnim;

    private Coroutine doorAnimationCoroutine;
    private bool isFailSequenceRunning = false;

    [Header("Managers")]
    private RoundManager roundManager;
    private UIAnimationManager uiAnimManager;

    // 씬 로딩 상태 추적
    private readonly Dictionary<ulong, string> clientLoadedScenes = new Dictionary<ulong, string>();
    private readonly Dictionary<string, int> sceneRefCount = new Dictionary<string, int>();

    public NetworkVariable<bool> doorState = new NetworkVariable<bool>(false); // false: closed, true: open
    public NetworkVariable<bool> isSequenceRunning = new NetworkVariable<bool>(false);

    public override void Start()
    {
        base.Start();
        originalPosition_R = rightDoorAxis.localPosition;
        originalPosition_L = leftDoorAxis.localPosition;

        roundManager = FindAnyObjectByType<RoundManager>();
        uiAnimManager = FindAnyObjectByType<UIAnimationManager>();

        if (IsServer)
		{
            PlayersManager.Instance.allPlayersDead.OnValueChanged += OnAllPlayersDead;
            PlayersManager.Instance.allPlayersDead.OnValueChanged += SceneUnload;
        }
    }

    private void OnDisable()
    {
        if (IsServer)
        {
            PlayersManager.Instance.allPlayersDead.OnValueChanged -= OnAllPlayersDead;
            PlayersManager.Instance.allPlayersDead.OnValueChanged -= SceneUnload;
        }
    }

    public override bool Interact(ulong userID, Transform interactingObjectTransform)
    {
        if (isSequenceRunning.Value)
            return false;
        if (!base.Interact(userID, interactingObjectTransform))
            return false;

        buttonAnim.SetTrigger("Push");

        if (!doorState.Value)
        {
            ToggleDooServerRpc(true);
            RequestSceneChangeServerRpc("TestScene");
        }
        else
            RequestGameClearServerRpc();

        return true;
    }

    #region Game Flow

    private void OnAllPlayersDead(bool _, bool newValue)
    {
        if (newValue && !isFailSequenceRunning)
        {
            isFailSequenceRunning = true;
            StartCoroutine(HandleGameFail());
        }
    }
    private void SceneUnload(bool _, bool newValue)
    {
        if (!newValue)
        {
            RequestGameFailServerRpc();
        }
    }

    private IEnumerator HandleGameFail()
    {
        AlldieAnimeServerRpc();

        yield return RunGameSequence(
            beforeDelay: 2f,
            afterDelay: 10f
        );
        isFailSequenceRunning = false;
    }

    private IEnumerator HandleGameClear()
    {
        yield return RunGameSequence(
            beforeDelay: 0f,
            afterDelay: 20f,
            onServerStart: () => roundManager.GameClearServerRPC(),
            onServerEnd: () => RequestSceneChangeServerRpc("TestScene")
        );
    }

    private IEnumerator RunGameSequence(float beforeDelay, float afterDelay, System.Action onServerStart = null, System.Action onServerEnd = null)
    {
        isSequenceRunning.Value = true;
        KeySettingsManager.Instance.isEveryEvent = true;

        // 2. 문 닫기 (이미 열려있다면만 닫기)
        if (doorState.Value)
        {
            ToggleDooServerRpc(false);
            yield return doorAnimationCoroutine;
        }


        // 3. 문 닫기 끝난 시점에서 서버 이벤트 실행
        onServerStart?.Invoke();

        // 4. 게임 클리어 애니메이션
        roundManager.GameClearAnimeServerRpc();

        // 5. 플레이어 부활
        yield return PlayersManager.Instance.RespawnPlayers(true);

        // 6. 추가 대기
        yield return new WaitForSeconds(afterDelay);

        // 7. 씬 언로드/변경
        KeySettingsManager.Instance.isEveryEvent = false;
        if (IsServer) onServerEnd?.Invoke();


        isSequenceRunning.Value = false;
    }

    #endregion

    #region RPCs

    [ServerRpc]
    private void RequestGameFailServerRpc()
    {
        RequestSceneChangeServerRpc("TestScene");
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestGameClearServerRpc()
    {
        StartCoroutine(HandleGameClear());
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSceneChangeServerRpc(string sceneName, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        UnloadAllClientScenes();
        if (!string.IsNullOrEmpty(sceneName))
            LoadSceneForClient(clientId, sceneName);
    }

    #endregion

    #region Scene Management

    private void LoadSceneForClient(ulong clientId, string sceneName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        clientLoadedScenes[clientId] = sceneName;

        if (!sceneRefCount.ContainsKey(sceneName))
            sceneRefCount[sceneName] = 0;
        sceneRefCount[sceneName]++;
    }

    private void UnloadAllClientScenes()
    {
        foreach (var sceneName in new HashSet<string>(clientLoadedScenes.Values))
            UnloadScene(sceneName);
    }

    private void UnloadScene(string sceneName)
    {
        if (!sceneRefCount.ContainsKey(sceneName)) return;

        sceneRefCount[sceneName]--;
        if (sceneRefCount[sceneName] <= 0)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene.IsValid())
                NetworkManager.Singleton.SceneManager.UnloadScene(scene);
            sceneRefCount.Remove(sceneName);
        }
    }

    public void OnPlayerDeath(ulong clientId)
    {
        if (IsServer && clientLoadedScenes.TryGetValue(clientId, out var sceneName))
            UnloadScene(sceneName);
    }

    #endregion

    #region Door Control

    [ServerRpc(RequireOwnership = false)]
    private void ToggleDooServerRpc(bool open)
    {
        if (doorAnimationCoroutine != null)
            StopCoroutine(doorAnimationCoroutine);

        doorAnimationCoroutine = StartCoroutine(AnimateDoors(open));
        PlayDoorEffectsClientRpc();
        doorState.Value = open;
    }

    [ServerRpc]
    private void AlldieAnimeServerRpc()
	{
        AlldieAnimeClientRpc();
    }

    [ClientRpc]
    private void AlldieAnimeClientRpc()
	{
        UIAnimationManager.Instance.AllDieAnimation();
    }

    [ClientRpc]
    private void PlayDoorEffectsClientRpc()
    {
        doorSound.Play();
    }


    private IEnumerator AnimateDoors(bool open)
    {
        // 이미 목표 위치라면 바로 종료
        if (open && doorState.Value) yield break;
        if (!open && !doorState.Value) yield break;

        float duration = doorSound.clip.length;
        Vector3 startR = rightDoorAxis.localPosition;
        Vector3 startL = leftDoorAxis.localPosition;
        Vector3 targetR = open ? targetPosition_R : originalPosition_R;
        Vector3 targetL = open ? targetPosition_L : originalPosition_L;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            leftDoorAxis.localPosition = Vector3.Slerp(startL, targetL, t);
            rightDoorAxis.localPosition = Vector3.Slerp(startR, targetR, t);
            yield return null;
        }

        doorState.Value = open;
        doorAnimationCoroutine = null;
    }

    #endregion
}
