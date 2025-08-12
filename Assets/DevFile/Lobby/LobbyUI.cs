using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LobbyUI : NetworkBehaviour
{
	[SerializeField] private Button serverBtn;
	[SerializeField] private Button hostBtn;
	[SerializeField] private Button clientBtn;
	[SerializeField] private Button testBtn;
	[SerializeField] private Text text;
	[SerializeField] private TMP_InputField joinCodeInput;

	private void FixedUpdate()
	{
		text.text = $"players in game: {PlayersManager.Instance.PlayersInGame}";
	}

	private void Awake()
	{
		serverBtn.onClick.AddListener(() =>
		{
			if (NetworkManager.Singleton.StartServer())
			{
				Logger.Instance?.LogInfo("Server started...");
			}
			else
			{
				Logger.Instance?.LogInfo("Server could not be started...");
			}
		});
		hostBtn.onClick.AddListener(async () =>
		{
			OnSceneLoadStarted();

			if (TestRelay.Instance.IsRelayEnabled)
			{
				await TestRelay.Instance.SetupRelay();
			}

			if (NetworkManager.Singleton.StartHost())
			{

				Logger.Instance?.LogInfo("Host started...");
				NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadCompleted;
				NetworkManager.Singleton.SceneManager.LoadScene("GameRoom", LoadSceneMode.Single);
			}
			else
			{
				OnSceneLoadFaill("GameError_0");
				Logger.Instance?.LogInfo("Host could not be started...");
			}
		});
		clientBtn.onClick.AddListener(async () =>
		{
			OnSceneLoadStarted();

			if (TestRelay.Instance.IsRelayEnabled && !string.IsNullOrEmpty(joinCodeInput.text))
			{
				await TestRelay.Instance.JoinRelay(joinCodeInput.text);
			}

			if (NetworkManager.Singleton.StartClient())
			{
				Logger.Instance?.LogInfo("Client started...");
				NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadCompleted;
				NetworkManager.Singleton.SceneManager.LoadScene("GameRoom", LoadSceneMode.Single);

				//PlayersManager.Instance.AddEvent();
			}
			else
			{
				OnSceneLoadFaill("GameError_1");
				Logger.Instance?.LogInfo("Client could not be started...");
			}
		});

		testBtn.onClick.AddListener(async () =>
		{
			OnSceneLoadStarted();

			if (TestRelay.Instance.IsRelayEnabled && !string.IsNullOrEmpty(joinCodeInput.text))
			{
				await TestRelay.Instance.JoinRelay(joinCodeInput.text);
			}

			if (NetworkManager.Singleton.StartClient())
			{
				Logger.Instance?.LogInfo("Client started...");
				NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadCompleted;
				NetworkManager.Singleton.SceneManager.LoadScene("GameRoom", LoadSceneMode.Single);

				//PlayersManager.Instance.AddEvent();
			}
			else
			{
				OnSceneLoadFaill("GameError_1");
				Logger.Instance?.LogInfo("Client could not be started...");
			}
		});
	}


	//테스트
	[Header("로딩")]
	public GameObject loadingUI;
	public TMP_Text loadingText; // 진행률을 표시할 UI 텍스트

	[Header("에러")]
	public GameObject errorUI;
	public TMP_Text errorText; // 진행률을 표시할 UI 텍스트

	private AsyncOperation currentOp;
	private bool isLoading = false;
	NetworkSceneManager networkSceneManager;

	public LocalizedString localizedString;

	// 씬 로딩 시작
	private void OnSceneLoadStarted()
	{
		loadingUI?.SetActive(true);
		isLoading = true;
		Debug.Log($"[로딩 시작]");

	}

	// 씬 로딩 완료
	private void OnSceneLoadCompleted(ulong clientId, string sceneName, LoadSceneMode mode)
	{
		if (clientId == NetworkManager.Singleton.LocalClientId)
		{
			isLoading = false;
			loadingUI?.SetActive(false);
			Debug.Log($"[로딩 완료] scene: {sceneName}");
		}
	}


	private void OnSceneLoadFaill(string error)
	{
		errorUI?.SetActive(true);

		localizedString.TableReference = "ErrorTable"; // 사용하고자 하는 테이블
		localizedString.TableEntryReference = error; // 사용하고자 하는 키
		errorText.text = $"{localizedString.GetLocalizedString()}";


		isLoading = false;
		loadingUI?.SetActive(false);
		Debug.Log($"[로딩 실패]");
	}
}
