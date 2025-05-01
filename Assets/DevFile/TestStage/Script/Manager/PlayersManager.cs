using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;
using DilmerGames.Core.Singletons;
using Unity.Netcode;


public class PlayersManager : NetworkSingleton<PlayersManager>
{
	[SerializeField] private NetworkVariable<int> playersInGame = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	[SerializeField] private NetworkList<ulong> players = new NetworkList<ulong>();
	[SerializeField] private List<Player> playersList = new List<Player>();

	[SerializeField] private UIAnimationManager uiAniManager;
	public bool allPlayersDead = false;

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

		Player.OnDie += OnDieChecker;
	}

	private void OnDieChecker()
	{
		if (!IsServer || playersList.Count == 0) return;

		bool allDead = true;
		foreach (Player player in playersList)
		{
			if (!player.IsDead)
			{
				allDead = false;
				break;
			}
		}

		if (allDead && !allPlayersDead)
		{
			allPlayersDead = true;
			OnAllPlayersDeadClientRpc(); // 클라이언트 알림
			Debug.Log("모든 플레이어가 사망했습니다!");
		}
		else if (!allDead && allPlayersDead)
		{
			allPlayersDead = false;
		}
	}

	private IEnumerator RespawnPlayersAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);

		foreach (Player player in playersList)
		{
			//player.Respawn(GetRespawnPointForPlayer(player)); // 위치 포함 리스폰
		}
	}

	// 클라이언트에게 결과 전파
	[ClientRpc]
	private void OnAllPlayersDeadClientRpc()
	{
		// 게임 오버 UI 표시 등
		Debug.Log("게임 오버!");
	}

	private void OnPlayerJoined(ulong clientId)
	{
		if (!IsServer) return;

		if (!players.Contains(clientId))
		{
			players.Add(clientId);
			playersInGame.Value++;
			if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(clientId, out NetworkObject camTargetNetObj))
			{
				playersList.Add(camTargetNetObj.GetComponent<Player>());
			}
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
			if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(clientId, out NetworkObject camTargetNetObj))
			{
				playersList.Remove(camTargetNetObj.GetComponent<Player>());
			}
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
