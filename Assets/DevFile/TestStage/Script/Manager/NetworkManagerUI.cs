using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
	[SerializeField] private Text text;

	private void FixedUpdate()
	{
		text.text = $"players in game: {PlayersManager.Instance.PlayersInGame}";
	}

	private void Awake()
	{
		serverBtn.onClick.AddListener(() =>		{
			if (NetworkManager.Singleton.StartServer())
			{
				Logger.Instance.LogInfo("Server started...");
			}
			else
			{
				Logger.Instance.LogInfo("Server could not be started...");
			}
		});
		hostBtn.onClick.AddListener(() => {
			if (NetworkManager.Singleton.StartHost())
			{
				Logger.Instance.LogInfo("Host started...");
			}
			else
			{
				Logger.Instance.LogInfo("Host could not be started...");
			}
		});
		clientBtn.onClick.AddListener(() => {
			if (NetworkManager.Singleton.StartClient())
			{
				Logger.Instance.LogInfo("Client started...");
			}
			else
			{
				Logger.Instance.LogInfo("Client could not be started...");
			}							
		});
	}
}
