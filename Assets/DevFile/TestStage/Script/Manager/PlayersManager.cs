using System.Collections.Generic;
using System;
using UnityEngine;
using DilmerGames.Core.Singletons;
using Unity.Netcode;


public class PlayersManager : NetworkSingleton<PlayersManager>
{
	[SerializeField]
	private NetworkVariable<int> playersInGame = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	[SerializeField] private NetworkList<ulong> players = new NetworkList<ulong>();

	public event Action<ulong> OnPlayerAdded;

	public int PlayersInGame { get { return playersInGame.Value; } }

	private void Start()
	{
		if (NetworkManager.Singleton == null) return;

		NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
		{
			Logger.Instance.LogInfo($"{id} is Connected...");
			OnPlayerJoined(id);
		};

		NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
		{
			Logger.Instance.LogInfo($"{id} is Disconnected...");
			OnPlayerLeft(id);
		};
	}

	private void OnPlayerJoined(ulong clientId)
	{
		if (!IsServer) return;

		if (!players.Contains(clientId))
		{
			players.Add(clientId);
			playersInGame.Value++;
			Debug.Log($"플레이어 {clientId} 입장. 총 플레이어 수: {playersInGame.Value}");

			OnPlayerAdded?.Invoke(clientId);
		}
	}

	private void OnPlayerLeft(ulong clientId)
	{
		if (!IsServer) return;

		if (players.Contains(clientId))
		{
			players.Remove(clientId);
			playersInGame.Value--;
			Debug.Log($"플레이어 {clientId} 퇴장. 총 플레이어 수: {playersInGame.Value}");
		}
	}

	public List<ulong> GetAllPlayers()
	{
		List<ulong> playerList = new List<ulong>();
		foreach (var playerId in players)
		{
			playerList.Add(playerId);
		}
		return playerList;
	}


	public ulong GetPlayerFromNum(int index)
	{
		if (index >= 0 && index < players.Count)
		{
			return players[index];
		}
		return 0; // 잘못된 경우 기본값 반환
	}

}
