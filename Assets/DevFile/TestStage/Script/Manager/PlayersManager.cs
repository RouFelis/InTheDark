using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;
using DilmerGames.Core.Singletons;
using Dissonance.Integrations.Unity_NFGO;
using Unity.Netcode;


public class PlayersManager : NetworkSingleton<PlayersManager>
{
	[Header("네트워크 정보.")]
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
		// 네트워크에 연결된 모든 클라이언트 순회
		foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
		{
			if (client.PlayerObject != null)
			{
				Player playerComponent = client.PlayerObject.GetComponent<Player>();
				NfgoPlayer nfgoComponent = client.PlayerObject.GetComponent<NfgoPlayer>();

				// 중복되지 않은 경우에만 playersList에 추가
				if (!playersList.Contains(playerComponent))
					playersList.Add(playerComponent);

				// 중복되지 않은 경우에만 nfgoPlayer에 추가
				if (!nfgoPlayer.Contains(nfgoComponent))
					nfgoPlayer.Add(nfgoComponent);
			}
		}

		// 현재 로컬 플레이어의 네트워크 오브젝트
		NetworkObject localPlayerObject = NetworkManager.Singleton.LocalClient.PlayerObject;

		Player localPlayer = localPlayerObject.GetComponent<Player>();
		NfgoPlayer localNfgo = localPlayerObject.GetComponent<NfgoPlayer>();

		// 로컬 플레이어를 리스트에서 제거한 뒤 맨 앞에 다시 추가 (우선순위 부여)
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
			Debug.Log($"테스트 죽음 {player.IsDead}");
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
			OnAllPlayersDeadClientRpc(); // 클라이언트 알림
			Debug.Log("모든 플레이어가 사망했습니다!");
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

		yield return null; // 전체 코루틴 끝나는 건 기다리지 않음
	}


	private IEnumerator RespawnSinglePlayer(Player player, ulong originalPlayer, bool isFadeAnime)
	{
		bool isInsideRespawnZone = IsPlayerInsideRespawnZone(player);

		if (!isInsideRespawnZone || player.IsDead)
		{
			Debug.Log($"[Respawn] {player.PlayerName} 리스폰 존 밖 - 텔레포트 및 부활 시도");

			bool isMyPlayer = player.IsOwner;

			if (isFadeAnime && isMyPlayer)
			{
				yield return new WaitForSeconds(4f);
				animanager.ReviveAnimation();
				yield return new WaitForSeconds(2f);
			}

			if (IsServer)
				setter.SetUserPosServerRPC(originalPlayer);

			StartCoroutine(player.ReviveSequence());

			if (IsServer)
				PlayersManager.Instance.allPlayersDead.Value = false;
		}
		else
		{
			Debug.Log($"[Respawn] {player.PlayerName} 은 리스폰 존 안에 있음 - 이동 생략");
		}
	}


	// 클라이언트에게 결과 전파
	[ClientRpc]
	private void OnAllPlayersDeadClientRpc()
	{
		// 게임 오버 UI 표시 등
		Debug.Log("게임 오버!");
	}

	private IEnumerator OnPlayerJoined(ulong clientId)
	{
		yield return new WaitForSeconds(0.1f); // 네트워크 객체 초기화 대기

		if (!IsServer)
		{
			yield return null;
		}

		if (IsClient && !IsServer)
		{
			// 클라이언트 기준: 리스트가 비어있으면 전체 초기화
			if (playersList.Count == 0)
			{
				yield return new WaitForSeconds(0.1f);
				SetPlayerInLoby();
			}
			// 아니면 새로 접속한 clientId만 개별 추가
			else if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var playerClient))
			{
				Player playerComponent = playerClient.PlayerObject.GetComponent<Player>();
				NfgoPlayer nfgoComponent = playerClient.PlayerObject.GetComponent<NfgoPlayer>();

				// 중복되지 않은 경우에만 playersList에 추가
				if (!playersList.Contains(playerComponent))
					playersList.Add(playerComponent);

				// 중복되지 않은 경우에만 nfgoPlayer에 추가
				if (!nfgoPlayer.Contains(nfgoComponent))
					nfgoPlayer.Add(nfgoComponent);
			}
		}

		// 아직 등록되지 않은 clientId만 players에 추가
		if (!players.Contains(clientId))
		{
			players.Add(clientId);                 // clientId 저장
			playersInGame.Value++;                 // 플레이어 수 증가

			if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var playerClient))
			{
				Player playerComponent = playerClient.PlayerObject.GetComponent<Player>();
				NfgoPlayer nfgoComponent = playerClient.PlayerObject.GetComponent<NfgoPlayer>();

				// 중복되지 않은 경우에만 playersList에 추가
				if (!playersList.Contains(playerComponent))
					playersList.Add(playerComponent);

				// 중복되지 않은 경우에만 nfgoPlayer에 추가
				if (!nfgoPlayer.Contains(nfgoComponent))
					nfgoPlayer.Add(nfgoComponent);
			}

			// 이벤트 호출
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
			Debug.Log($"플레이어 {clientId} 퇴장. 총 플레이어 수: {playersInGame.Value}");
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
		return 0; // 잘못된 경우 기본값 반환
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
