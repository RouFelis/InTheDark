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
    [SerializeField] private float doorOpenAngle = 90f;
    [SerializeField] private float doorAnimationSpeed = 2f;
    [SerializeField] private AudioSource doorSound;
    [SerializeField] private BoxCollider doorCollider;
    [SerializeField] private Animator buttonAnim;
    [SerializeField] private RoundManager roundManager;

    [Header("Door Positions")]
    [SerializeField] private Vector3 originalPosition_R;
    [SerializeField] private Vector3 originalPosition_L;
    [SerializeField] private Vector3 targetPosition_R;
    [SerializeField] private Vector3 targetPosition_L;

    private Coroutine doorAnimationCoroutine;
    private UIAnimationManager animanager;
    private bool isFailSequenceRunning = false;

    private Dictionary<ulong, string> clientLoadedScenes = new Dictionary<ulong, string>();
    private Dictionary<string, int> sceneRefCount = new Dictionary<string, int>();

    public NetworkVariable<bool> doorState = new NetworkVariable<bool>(false); // false: closed, true: open


    public override void Start()
	{
        originalPosition_R = rightDoorAxis.localPosition;
        originalPosition_L = leftDoorAxis.localPosition;

        base.Start();
		roundManager = FindAnyObjectByType<RoundManager>();
		animanager = FindAnyObjectByType<UIAnimationManager>();
	}

    private void OnEnable()
	{
        PlayersManager.Instance.allPlayersDead.OnValueChanged += GameFailSequnce;
    }

	private void OnDisable()
	{
        PlayersManager.Instance.allPlayersDead.OnValueChanged -= GameFailSequnce;
    }


	public override bool Interact(ulong userID, Transform interactingObjectTransform)
	{
		if (!base.Interact(userID, interactingObjectTransform))
			return false;

        buttonAnim.SetTrigger("Push");

        if (!doorState.Value)
			RequestSceneChangeServerRpc("TestScene");
		else
			RequestGameClearServerRpc();

        return true;
    }



    private void GameFailSequnce(bool oldValue, bool newValue)
    {
        if (newValue && !isFailSequenceRunning)
        {
            isFailSequenceRunning = true;
            StartCoroutine(HandleGameFail());
        }
    }

    #region Game Flow

    private IEnumerator HandleGameFail()
    {
        yield return RunCommonGameSequence(
            beforeDelay: 6f,
            afterDelay: 7f,
            onServerEnd: () => RequestGameFailServerRpc()
        );
        isFailSequenceRunning = false;
    }

    private IEnumerator HandleGameClear()
    {
        yield return RunCommonGameSequence(
            beforeDelay: 0f,
            afterDelay: 7f,
            onServerStart: () => roundManager.GameClearServerRPC(),
            onServerEnd: () => RequestSceneChangeServerRpc("TestScene")
        );
    }

    /// <summary>
    /// 실패/성공 공통 시퀀스 처리
    /// </summary>
    private IEnumerator RunCommonGameSequence(float beforeDelay, float afterDelay, System.Action onServerStart = null, System.Action onServerEnd = null)
    {
        KeySettingsManager.Instance.isEveryEvent = true;

        yield return new WaitForSeconds(beforeDelay);

        onServerStart?.Invoke();
        roundManager.GameClearAnime();
        StartCoroutine(PlayersManager.Instance.RespawnPlayers(true));

        yield return new WaitForSeconds(afterDelay);

        KeySettingsManager.Instance.isEveryEvent = false;
        if (IsServer) onServerEnd?.Invoke();
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
        GameClearClientRPC();
    }

    [ClientRpc]
    private void GameClearClientRPC()
    {
        StartCoroutine(HandleGameClear());
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSceneChangeServerRpc(string sceneName, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        // 현재 클라이언트가 가진 씬 모두 언로드
        UnloadAllClientScenes();

        if (!string.IsNullOrEmpty(sceneName))
            LoadSceneForClient(clientId, sceneName);

        // 문 상태 전환
        ToggleDoor(!doorState.Value);
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
        HashSet<string> uniqueScenes = new HashSet<string>(clientLoadedScenes.Values);
        foreach (var scene in uniqueScenes)
            UnloadScene(scene);
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

    private void ToggleDoor(bool open)
    {
        if (doorAnimationCoroutine != null)
            StopCoroutine(doorAnimationCoroutine);

        doorAnimationCoroutine = StartCoroutine(AnimateDoors(open));
        PlayDoorEffectsClientRpc(open);
        doorState.Value = open;
    }

    [ClientRpc]
    private void PlayDoorEffectsClientRpc(bool opening)
    {
        doorSound.Play();
    }

    private IEnumerator AnimateDoors(bool open)
    {
        float duration = doorSound.clip.length;
        Vector3 startR = rightDoorAxis.localPosition;
        Vector3 startL = leftDoorAxis.localPosition;
        Vector3 targetR = open ? targetPosition_R : originalPosition_R;
        Vector3 targetL = open ? targetPosition_L : originalPosition_L;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float frac = Mathf.Clamp01(t / duration);
            leftDoorAxis.localPosition = Vector3.Slerp(startL, targetL, frac);
            rightDoorAxis.localPosition = Vector3.Slerp(startR, targetR, frac);
            yield return null;
        }

        doorAnimationCoroutine = null;
    }

    #endregion

}
