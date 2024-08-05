using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DilmerGames.Core.Singletons;
using Unity.Netcode;
using System.IO;


public class PlayersManager : NetworkSingleton<PlayersManager>
{
	private NetworkVariable<int> playersInGame = new NetworkVariable<int>(); //NetworkVariableReadPermission.Owner

	public int PlayersInGame
	{
		get
		{
			return playersInGame.Value;
		}
	}

	private void Start()
	{
		NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
		{
			if (IsServer)
			{
				Logger.Instance.LogInfo($"{id} is Connected...");
				playersInGame.Value++;
			}

		};

		NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
		{
			if (IsServer)
			{
				Logger.Instance.LogInfo($"{id} is Disconnected...");
				playersInGame.Value--;
			}

		};
	}


}
