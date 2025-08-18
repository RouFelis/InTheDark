using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;
using DilmerGames.Core.Singletons;
using Dissonance.Integrations.Unity_NFGO;
using Unity.Netcode;


public class PlayersManager : NetworkSingleton<PlayersManager>
{
	[Header("��Ʈ��ũ ����.")]
	[SerializeField] private NetworkVariable<int> playersInGame = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	[SerializeField] private NetworkList<ulong> players = new NetworkList<ulong>();
	public NetworkVariable<bool> allPlayersDead = new NetworkVariable<bool>(writePerm: NetworkVariableWritePermission.Server);
	[SerializeField] public List<NfgoPlayer> nfgoPlayer = new List<NfgoPlayer>();

	public List<Player> playersList = new List<Player>();
	public bool myPlayerDead = false;

	public event Action<ulong> OnPlayerAdded;
	public event Action<ulong> OnPlayerRemoved;


	[Header("Gizmo")]
	public Vector3 boxCenter = Vector3.zero;
	public Vector3 boxSize = new Vector3(5, 3, 5);
	public LayerMask playerLayer;

	// Ŭ���� ���: Ÿ�̹� ��� �߰�
	[SerializeField] private float BlackoutBeforLeadSeconds = 5f; // �۸�ġ �����ְ� ���ƿ������� ����Ÿ��
	[SerializeField] private float BlackoutLeadSeconds = 3f; // �۸�ġ �����ְ� ���ƿ������� ����Ÿ��
	[SerializeField] private float FadeInDelaySeconds = 3f;    // ���� �� � ȭ�� ���� �ð�

	//REF
	private WaitRoomSetter setter;
	private UIAnimationManager animanager;


	public int PlayersInGame { get { return playersInGame.Value; } }

	private void Start()
	{
		if (NetworkManager.Singleton == null) return;

		StartCoroutine(findScript());


		NetworkManager.Singleton.OnServerStarted += () =>
		{
			if (IsServer)
			{
				//Player.OnDie += OnDieCheckerServerRPC;
				Player.OnDie += () =>
				{
					Debug.Log("OnDie event triggered!");
					OnDieCheckerServerRPC();
				};

			};
		};



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

	}


	private IEnumerator findScript()
	{
		while (setter == null)
		{
			setter = FindAnyObjectByType<WaitRoomSetter>();
			yield return new WaitForSeconds(1f);
		}
		while (animanager == null)
		{
			animanager = FindAnyObjectByType<UIAnimationManager>();
			yield return new WaitForSeconds(1f);
		}
	}

	private void SetPlayerInLoby()
	{
		// ��Ʈ��ũ�� ����� ��� Ŭ���̾�Ʈ ��ȸ
		foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
		{
			if (client.PlayerObject != null)
			{
				Player playerComponent = client.PlayerObject.GetComponent<Player>();
				NfgoPlayer nfgoComponent = client.PlayerObject.GetComponent<NfgoPlayer>();

				// �ߺ����� ���� ��쿡�� playersList�� �߰�
				if (!playersList.Contains(playerComponent))
					playersList.Add(playerComponent);

				// �ߺ����� ���� ��쿡�� nfgoPlayer�� �߰�
				if (!nfgoPlayer.Contains(nfgoComponent))
					nfgoPlayer.Add(nfgoComponent);
			}
		}

		// ���� ���� �÷��̾��� ��Ʈ��ũ ������Ʈ
		NetworkObject localPlayerObject = NetworkManager.Singleton.LocalClient.PlayerObject;

		Player localPlayer = localPlayerObject.GetComponent<Player>();
		NfgoPlayer localNfgo = localPlayerObject.GetComponent<NfgoPlayer>();

		// ���� �÷��̾ ����Ʈ���� ������ �� �� �տ� �ٽ� �߰� (�켱���� �ο�)
		if (playersList.Remove(localPlayer))
			playersList.Insert(0, localPlayer);

		if (nfgoPlayer.Remove(localNfgo))
			nfgoPlayer.Insert(0, localNfgo);

	}

	[ServerRpc]
	private void OnDieCheckerServerRPC()
	{
		if (!IsServer || playersList.Count == 0) return;

		bool allDead = true;
		foreach (Player player in playersList)
		{
			Debug.Log($"�׽�Ʈ ���� {player.IsDead}");
			if (!player.IsDead)
			{

				allDead = false;
				break;
			}
		}

		foreach (Player player in playersList)
		{
			if (player.isMyCharacter)
			{
				{
					myPlayerDead = true;
					break;
				}
			}
		}
		if (allDead && !allPlayersDead.Value)
		{
			allPlayersDead.Value = true;
			OnAllPlayersDeadClientRpc(); // Ŭ���̾�Ʈ �˸�
			Debug.Log("��� �÷��̾ ����߽��ϴ�!");
		}
		else if (!allDead && allPlayersDead.Value)
		{
			allPlayersDead.Value = false;
		}

	}


	private bool IsPlayerInsideRespawnZone(Player player)
	{
		Collider[] hits = Physics.OverlapBox(transform.position + boxCenter, boxSize * 0.5f, Quaternion.identity, playerLayer);

		foreach (var hit in hits)
		{
			if (hit.transform == player.transform || hit.GetComponent<Player>() == player)
			{
				return true;
			}
		}

		return false;
	}

	public IEnumerator RespawnPlayers(bool isFadeAnime)
	{
		int index = 0;
		foreach (Player player in playersList)
		{
			StartCoroutine(RespawnSinglePlayer(player, players[index], isFadeAnime));
			index++;
			Debug.Log($"�׽�Ʈ : {index}");
		}

		yield return null; // ��ü �ڷ�ƾ ������ �� ��ٸ��� ����
	}

	private IEnumerator RespawnSinglePlayer(Player player, ulong originalPlayer, bool isFadeAnime)
	{
		bool isInsideRespawnZone = IsPlayerInsideRespawnZone(player);

		if (!isInsideRespawnZone || player.IsDead)
		{
			Debug.Log($"[Respawn] {player.Name} ������ �� �� - �ڷ���Ʈ �� ��Ȱ �õ�");
			PlayerUIHandler handler = player.GetComponent<PlayerUIHandler>();

			// �߰�. ���� ���
			yield return new WaitForSeconds(BlackoutBeforLeadSeconds);

			/*// 1) ����(����) �÷��̾�Ը� �۸�ġ + ���ƿ� ���� ->> Ŭ���̾�Ʈ���� ���۾ȵ�.
			if (isFadeAnime)
				handler.GlitchServerRpc();*/
			// 2) �������� ���� �÷��̾�Ը� ClientRpc ����
			if (isFadeAnime)
			{
				var target = new ClientRpcParams
				{
					Send = new ClientRpcSendParams
					{
						TargetClientIds = new[] { player.OwnerClientId }
					}
				};
				handler.GlitchClientRpc(target);
			}


			// 2) ��� �ν��Ͻ����� ������ ����Ÿ�� ��ŭ ��� -> ���� �ڷ���Ʈ Ÿ�̹� ����ȭ
			yield return new WaitForSeconds(BlackoutLeadSeconds);


			// 3) ������ ���� ��ġ�� ����(�ڷ���Ʈ) -->> �̰� �־ȵ�?
			if (IsServer)
				setter.SetUserPosServerRPC(originalPlayer);


			// 4) ��Ȱ ó��(Ŭ�� ����� ����)
			StartCoroutine(player.ReviveSequence());


			// 5) �������� allPlayersDead ����
			if (IsServer)
				allPlayersDead.Value = false;


			// 6) ����(����) �÷��̾�Ը� ���� �ð� �� ���̵� ��
			if (isFadeAnime)
			{
				yield return new WaitForSeconds(FadeInDelaySeconds);
				var target = new ClientRpcParams
				{
					Send = new ClientRpcSendParams
					{
						TargetClientIds = new[] { player.OwnerClientId }
					}
				};
				handler.FadeinClientRpc(target);
			}
		}
		else
		{
			Debug.Log($"[Respawn] {player.Name} �� ������ �� �ȿ� ���� - �̵� ����");
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
		yield return new WaitForSeconds(0.1f); // ��Ʈ��ũ ��ü �ʱ�ȭ ���

		if (!IsServer)
		{
			yield return null;
		}

		if (IsClient && !IsServer)
		{
			// Ŭ���̾�Ʈ ����: ����Ʈ�� ��������� ��ü �ʱ�ȭ
			if (playersList.Count == 0)
			{
				yield return new WaitForSeconds(0.1f);
				SetPlayerInLoby();
			}
			// �ƴϸ� ���� ������ clientId�� ���� �߰�
			else if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var playerClient))
			{
				Player playerComponent = playerClient.PlayerObject.GetComponent<Player>();
				NfgoPlayer nfgoComponent = playerClient.PlayerObject.GetComponent<NfgoPlayer>();

				// �ߺ����� ���� ��쿡�� playersList�� �߰�
				if (!playersList.Contains(playerComponent))
					playersList.Add(playerComponent);

				// �ߺ����� ���� ��쿡�� nfgoPlayer�� �߰�
				if (!nfgoPlayer.Contains(nfgoComponent))
					nfgoPlayer.Add(nfgoComponent);
			}
		}

		// ���� ��ϵ��� ���� clientId�� players�� �߰�
		if (!players.Contains(clientId))
		{
			players.Add(clientId);                 // clientId ����
			playersInGame.Value++;                 // �÷��̾� �� ����

			if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var playerClient))
			{
				Player playerComponent = playerClient.PlayerObject.GetComponent<Player>();
				NfgoPlayer nfgoComponent = playerClient.PlayerObject.GetComponent<NfgoPlayer>();

				// �ߺ����� ���� ��쿡�� playersList�� �߰�
				if (!playersList.Contains(playerComponent))
					playersList.Add(playerComponent);

				// �ߺ����� ���� ��쿡�� nfgoPlayer�� �߰�
				if (!nfgoPlayer.Contains(nfgoComponent))
					nfgoPlayer.Add(nfgoComponent);
			}

			// �̺�Ʈ ȣ��
			OnPlayerAdded?.Invoke(clientId);
		}
	}


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


#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		//if (!drawGizmo) return;

		Gizmos.color = new Color(0, 1, 0, 0.3f);
		Gizmos.DrawCube(transform.position + boxCenter, boxSize);

		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(transform.position + boxCenter, boxSize);
	}
#endif
}
