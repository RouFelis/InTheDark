using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;
using DilmerGames.Core.Singletons;
using Dissonance.Integrations.Unity_NFGO;
using Unity.Netcode;


public class PlayersManager : NetworkSingleton<PlayersManager>
{
	[SerializeField] private NetworkVariable<int> playersInGame = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	[SerializeField] private NetworkList<ulong> players = new NetworkList<ulong>();
	[SerializeField] private UIAnimationManager uiAniManager;
	[SerializeField] public List<NfgoPlayer> nfgoPlayer = new List<NfgoPlayer>();

	public List<Player> playersList = new List<Player>();
	public bool allPlayersDead = false;

	public event Action<ulong> OnPlayerAdded;
	public event Action<ulong> OnPlayerRemoved;

	public int PlayersInGame { get { return playersInGame.Value; } }

	private void Start()
	{
		if (NetworkManager.Singleton == null) return;

		NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
		{
			Logger.Instance.LogInfo($"{id} is Connected...");
			StartCoroutine(OnPlayerJoined(id));
		};

		NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
		{
			Logger.Instance.LogInfo($"{id} is Disconnected...");
			OnPlayerLeft(id);
		};

		Player.OnDie += OnDieChecker;
	}

	private void SetPlayerInLoby()
	{
		// ��Ʈ��ũ�� ����� ��� Ŭ���̾�Ʈ ��ȸ
		foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
		{
			// Ŭ���̾�Ʈ�� �÷��̾� ������Ʈ�� �����ϰ� �̹� ����Ʈ�� ���Ե��� �ʾҴٸ� �߰��մϴ�.
			if (client.PlayerObject != null)
			{
				playersList.Add(client.PlayerObject.GetComponent<Player>());
				nfgoPlayer.Add(client.PlayerObject.GetComponent<NfgoPlayer>());
			}
		}

		// ���� ���� �÷��̾��� ��Ʈ��ũ ������Ʈ�� ã���ϴ�.
		NetworkObject localPlayerObject = NetworkManager.Singleton.LocalClient.PlayerObject;

		if (playersList.Remove(localPlayerObject.GetComponent<Player>()))
		{
			playersList.Insert(0, localPlayerObject.GetComponent<Player>());
			nfgoPlayer.Insert(0, localPlayerObject.GetComponent<NfgoPlayer>());
		}

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
			OnAllPlayersDeadClientRpc(); // Ŭ���̾�Ʈ �˸�
			Debug.Log("��� �÷��̾ ����߽��ϴ�!");
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
			//player.Respawn(GetRespawnPointForPlayer(player)); // ��ġ ���� ������
		}
	}

	// Ŭ���̾�Ʈ���� ��� ����
	[ClientRpc]
	private void OnAllPlayersDeadClientRpc()
	{
		// ���� ���� UI ǥ�� ��
		Debug.Log("���� ����!");
	}


	private IEnumerator OnPlayerJoined(ulong clientId)
	{
		yield return new WaitForSeconds(0.1f); // �� ������ ��ٸ���
		
		if (!IsServer)
		{
			yield return null;
		}

		if (IsClient && !IsServer)
		{
			if (playersList.Count == 0)
			{
				yield return new WaitForSeconds(0.1f); // �� ������ ��ٸ���
				SetPlayerInLoby();
			}
			else if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var playerClient))
			{
				playersList.Add(playerClient.PlayerObject.GetComponent<Player>());
				nfgoPlayer.Add(playerClient.PlayerObject.GetComponent<NfgoPlayer>());
			}
		}


		if (!players.Contains(clientId))
		{
			players.Add(clientId);
			playersInGame.Value++;
			if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var playerClient))
			{
				playersList.Add(playerClient.PlayerObject.GetComponent<Player>());
				nfgoPlayer.Add(playerClient.PlayerObject.GetComponent<NfgoPlayer>());
			}

			OnPlayerAdded?.Invoke(clientId);
		}
	}




	/*private void OnPlayerJoined(ulong clientId)
	{
		if (!IsServer)
		{
			return;
		}

		if (IsClient && !IsServer)
		{
			if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var camTargetNetObj))
			{
				playersList.Add(camTargetNetObj.PlayerObject.GetComponent<Player>());
			}
		}

		if (!players.Contains(clientId))
		{
			players.Add(clientId);
			playersInGame.Value++;
			if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var camTargetNetObj))
			{
				playersList.Add(camTargetNetObj.PlayerObject.GetComponent<Player>());
			}

			OnPlayerAdded?.Invoke(clientId);
		}
	}*/


	private void OnPlayerLeft(ulong clientId)
	{
		if (!IsServer) return;
		else
		{
			if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var playerClient))
			{
				playersList.Remove(playerClient.PlayerObject.GetComponent<Player>());
				nfgoPlayer.Remove(playerClient.PlayerObject.GetComponent<NfgoPlayer>());
			}
		}


		if (players.Contains(clientId))
		{
			players.Remove(clientId);
			playersInGame.Value--; 
			if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var camTargetNetObj))
			{
				playersList.Remove(camTargetNetObj.PlayerObject.GetComponent<Player>());
			}
			Debug.Log($"�÷��̾� {clientId} ����. �� �÷��̾� ��: {playersInGame.Value}");
		}

		OnPlayerRemoved.Invoke(clientId);
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
		return 0; // �߸��� ��� �⺻�� ��ȯ
	}
}
