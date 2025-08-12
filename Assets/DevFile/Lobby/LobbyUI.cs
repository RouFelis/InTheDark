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


	//�׽�Ʈ
	[Header("�ε�")]
	public GameObject loadingUI;
	public TMP_Text loadingText; // ������� ǥ���� UI �ؽ�Ʈ

	[Header("����")]
	public GameObject errorUI;
	public TMP_Text errorText; // ������� ǥ���� UI �ؽ�Ʈ

	private AsyncOperation currentOp;
	private bool isLoading = false;
	NetworkSceneManager networkSceneManager;

	public LocalizedString localizedString;

	// �� �ε� ����
	private void OnSceneLoadStarted()
	{
		loadingUI?.SetActive(true);
		isLoading = true;
		Debug.Log($"[�ε� ����]");

	}

	// �� �ε� �Ϸ�
	private void OnSceneLoadCompleted(ulong clientId, string sceneName, LoadSceneMode mode)
	{
		if (clientId == NetworkManager.Singleton.LocalClientId)
		{
			isLoading = false;
			loadingUI?.SetActive(false);
			Debug.Log($"[�ε� �Ϸ�] scene: {sceneName}");
		}
	}


	private void OnSceneLoadFaill(string error)
	{
		errorUI?.SetActive(true);

		localizedString.TableReference = "ErrorTable"; // ����ϰ��� �ϴ� ���̺�
		localizedString.TableEntryReference = error; // ����ϰ��� �ϴ� Ű
		errorText.text = $"{localizedString.GetLocalizedString()}";


		isLoading = false;
		loadingUI?.SetActive(false);
		Debug.Log($"[�ε� ����]");
	}
}
