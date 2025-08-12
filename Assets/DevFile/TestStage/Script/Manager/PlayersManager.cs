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
	[SerializeField] private UIAnimationManager uiAniManager;
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
	private const float BlackoutLeadSeconds = 0.6f; // �۸�ġ �����ְ� ���ƿ������� ����Ÿ��
	private const float FadeInDelaySeconds = 3f;    // ���� �� � ȭ�� ���� �ð�

	//REF
	private WaitRoomSetter setter;
	private UIAnimationManager animanager;


	public int PlayersInGame { get { return playersInGame.Value; } }

	private void Start()
	{
		if (NetworkManager.Singleton == null) return;

		StartCoroutine(findScript());

		NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
		{
			Logger.Instance.LogInfo($"{id} is Connected...");
			StartCoroutine(OnPlayerJoined(id));
			if (IsServer)
			{
				Player.OnDie += OnDieCheckerServerRPC;
			}
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
		}

		yield return null; // ��ü �ڷ�ƾ ������ �� ��ٸ��� ����
	}


	private IEnumerator RespawnSinglePlayer(Player player, ulong originalPlayer, bool isFadeAnime)
	{
		bool isInsideRespawnZone = IsPlayerInsideRespawnZone(player);

		if (!isInsideRespawnZone || player.IsDead)
		{
			Debug.Log($"[Respawn] {player.Name} ������ �� �� - �ڷ���Ʈ �� ��Ȱ �õ�");

			bool isMyPlayer = player.IsOwner;

			// 1) ����(����) �÷��̾�Ը� �۸�ġ + ���ƿ� ����
			if (isFadeAnime && isMyPlayer)
				StartCoroutine(PlayGlitchAndBlackoutLocal(BlackoutLeadSeconds));

			// 2) ��� �ν��Ͻ����� ������ ����Ÿ�� ��ŭ ��� -> ���� �ڷ���Ʈ Ÿ�̹� ����ȭ
			yield return new WaitForSeconds(BlackoutLeadSeconds);

			// 3) ������ ���� ��ġ�� ����(�ڷ���Ʈ)
			if (IsServer)
				setter.SetUserPosServerRPC(originalPlayer);

			// 4) ��Ȱ ó��(Ŭ�� ����� ����)
			StartCoroutine(player.ReviveSequence());

			// 5) �������� allPlayersDead ����
			if (IsServer)
				PlayersManager.Instance.allPlayersDead.Value = false;

			// 6) ����(����) �÷��̾�Ը� ���� �ð� �� ���̵� ��
			if (isFadeAnime && isMyPlayer)
			{
				yield return new WaitForSeconds(FadeInDelaySeconds);
				FadeInFromBlackLocal();
				uiAniManager.ReviveSet();
			}
		}
		else
		{
			Debug.Log($"[Respawn] {player.Name} �� ������ �� �ȿ� ���� - �̵� ����");
		}
	}

	// ���� ����Ʈ ��ƿ(������Ʈ�� UI/����Ʈ���μ��� ���� ����Ʈ)
	// �ʿ� �� UIAnimationManager �������� ��ü�ϼ���.
	private IEnumerator PlayGlitchAndBlackoutLocal(float leadSeconds)
	{
		TriggerGlitchEffectLocal(); // TODO: ����Ʈ���μ���/��Ƽ����/ĵ���� ����Ʈ ȣ��
		yield return new WaitForSeconds(leadSeconds);
		FadeToBlackLocal();         // TODO: Ǯ��ũ�� �г� ���� 1��
	}

	private void FadeInFromBlackLocal()
	{
		// TODO: Ǯ��ũ�� �г� ���� 0���� ������ (3�� ������ �� ȣ���)
		// ��: animanager?.FadeInFromBlack();
	}

	// �Ʒ� 3���� ���� ���� �ý��۰� ������ �ڸ� (�ӽ� �� ����)
	private void TriggerGlitchEffectLocal() { /* TODO: animanager?.PlayGlitch(); */ }
	private void FadeToBlackLocal()
	{ /* TODO: animanager?.FadeToBlack(); */}

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
