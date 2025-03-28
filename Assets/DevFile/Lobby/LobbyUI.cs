using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LobbyUI : NetworkBehaviour
{
	[SerializeField] private Button serverBtn;
	[SerializeField] private Button hostBtn;
	[SerializeField] private Button clientBtn;
	[SerializeField] private Text text;
	[SerializeField] private TMP_InputField joinCodeInput;

	private void FixedUpdate()
	{
		text.text = $"players in game: {PlayersManager.Instance.PlayersInGame}";
	}

	private void Awake()
	{
		serverBtn.onClick.AddListener(() => {
			if (NetworkManager.Singleton.StartServer())
			{
				Logger.Instance?.LogInfo("Server started...");	
			}
			else
			{
				Logger.Instance?.LogInfo("Server could not be started...");
			}
		});
		hostBtn.onClick.AddListener(async () => {
			if (TestRelay.Instance.IsRelayEnabled)
			{
				await TestRelay.Instance.SetupRelay();
			}

			if (NetworkManager.Singleton.StartHost())
			{
				Logger.Instance?.LogInfo("Host started...");
				NetworkManager.Singleton.SceneManager.LoadScene("GameRoom", LoadSceneMode.Single);
			}
			else
			{
				Logger.Instance?.LogInfo("Host could not be started...");
			}
		});
		clientBtn.onClick.AddListener(async () => {

			if (TestRelay.Instance.IsRelayEnabled && !string.IsNullOrEmpty(joinCodeInput.text))
			{
				await TestRelay.Instance.JoinRelay(joinCodeInput.text);
			}

			if (NetworkManager.Singleton.StartClient())
			{
				Logger.Instance?.LogInfo("Client started...");
				NetworkManager.Singleton.SceneManager.LoadScene("GameRoom", LoadSceneMode.Single);

				//PlayersManager.Instance.AddEvent();
			}
			else
			{
				Logger.Instance?.LogInfo("Client could not be started...");
			}
		});
	}
}
