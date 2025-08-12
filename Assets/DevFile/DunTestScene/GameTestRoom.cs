using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class GameTestRoom : InteractableObject
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

    private Coroutine doorAnimationCoroutine;
    public NetworkVariable<bool> doorState = new NetworkVariable<bool>(false); // false: closed, true: open

    private Dictionary<ulong, string> clientLoadedScenes = new Dictionary<ulong, string>();
	private Dictionary<string, int> sceneRefCount = new Dictionary<string, int>();

    [SerializeField] private Vector3 originalPosition_R;
    [SerializeField] private Vector3 originalPosition_L;
    [SerializeField] private Vector3 targetPosition_R;
    [SerializeField] private Vector3 targetPosition_L;

    private UIAnimationManager animanager;
    private bool isFailSequenceRunning = false;

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

		if (!doorState.Value)
			RequestSceneChangeServerRpc("TestRoom");
		else
			RequestGameClearServerRpc();

        return true;
    }



    private void GameFailSequnce(bool oldValue, bool newValue)
    {
        if (newValue && !isFailSequenceRunning)
        {
            isFailSequenceRunning = true;
            StartCoroutine(GameFail());
        }
    }

    [ServerRpc]
    private void RequestGameFailServerRpc()
	{
            RequestSceneChangeServerRpc("TestRoom");
    }

	private IEnumerator GameFail()
	{
		KeySettingsManager.Instance.isEveryEvent = true;

		yield return new WaitForSeconds(6f);

		roundManager.GameClearAnime();

		StartCoroutine(PlayersManager.Instance.RespawnPlayers(true));
        Debug.Log("테스트 Fail");

		yield return new WaitForSeconds(7f);

		KeySettingsManager.Instance.isEveryEvent = false;

		if (IsServer)
			RequestGameFailServerRpc();

        isFailSequenceRunning = false; // 반드시 복구
    }




	private IEnumerator GameClear()
	{
		KeySettingsManager.Instance.isEveryEvent = true;

        if(IsServer)
        //라운드 숫자 증가
        roundManager.GameClearServerRPC();


		//1. 게임 정보 취합 (돈) 애니메이션 재생
		roundManager.GameClearAnime();


        //2. 안에있는 플레이어들 위치 텔레포트 (부활)     
        StartCoroutine(PlayersManager.Instance.RespawnPlayers(true));
        Debug.Log("테스트 GameClear");

        yield return new WaitForSeconds(7f);

        KeySettingsManager.Instance.isEveryEvent = false;

        //3. 오브젝트들 삭제.
        if (IsServer)
            RequestSceneChangeServerRpc("TestRoom");

        yield return null;
	}


    [ServerRpc(RequireOwnership = false)]
    private void RequestGameClearServerRpc()
    {
        GameClearClientRPC();
    }

    [ClientRpc]
    private void GameClearClientRPC()
	{
        StartCoroutine(GameClear());
    }


    [ServerRpc(RequireOwnership = false)]
    private void RequestSceneChangeServerRpc(string sceneName, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        // 이미 로드된 씬들 중 중복 없이 언로드
        HashSet<string> uniqueLoadedScenes = new HashSet<string>(clientLoadedScenes.Values);
        foreach (string loadedScene in uniqueLoadedScenes)
        {
            UnloadSceneForClient(loadedScene);
        }

        if(sceneName != "")
		{
            // 새로운 씬 로드
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            clientLoadedScenes[clientId] = sceneName;

            // 참조 카운트 관리
            sceneRefCount.TryAdd(sceneName, 0);
            sceneRefCount[sceneName]++;
        }

        // 문 상태 반전 및 연출
        PlayDoorEffectsClientRpc(!doorState.Value);
        doorState.Value = !doorState.Value;
    }

    [ClientRpc]
    private void PlayDoorEffectsClientRpc(bool opening)
    {
        buttonAnim.SetTrigger("Push");
		//doorCollider.enabled = false;
		doorSound.Play();
		if (doorAnimationCoroutine != null)
		{
			StopCoroutine(doorAnimationCoroutine);
        }

        doorAnimationCoroutine = StartCoroutine(AnimateDoors(opening));
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

    private IEnumerator AnimateDoors(bool open)
    {
        float dur = doorSound.clip.length;
        Vector3 start_R = rightDoorAxis.localPosition;
        Vector3 start_L = leftDoorAxis.localPosition;
        Vector3 target_R = open ? targetPosition_R : originalPosition_R;
        Vector3 target_L = open ? targetPosition_L : originalPosition_L;

        float t = 0;
        while (t < dur)
        {
            t += Time.deltaTime;
            float frac = Mathf.Clamp01(t / dur);
            leftDoorAxis.localPosition = Vector3.Slerp(start_L, target_L, frac);
            rightDoorAxis.localPosition = Vector3.Slerp(start_R, target_R, frac);
            yield return null;
        }

        doorAnimationCoroutine = null;
    }

    public void OnPlayerDeath(ulong clientId)
    {
        if (IsServer && clientLoadedScenes.TryGetValue(clientId, out string sceneName))
        {
            UnloadSceneForClient(sceneName);
        }
    }

}
