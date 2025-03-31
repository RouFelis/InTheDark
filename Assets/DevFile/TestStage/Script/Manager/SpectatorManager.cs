using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Cinemachine;

public class SpectatorManager : MonoBehaviour
{
	[SerializeField] private Player userPlayer;
	[SerializeField] private List<NetworkObject> playersNetworkObject = new List<NetworkObject>();
	[SerializeField] private List<Player> players = new List<Player>();
	[SerializeField] private bool isUserDie = false;

	[SerializeField] private CinemachineVirtualCamera cinemachineCamera;
	[SerializeField] private int currentCameraIndex = 0;


	private void Start()
	{
		ulong localClientId = NetworkManager.Singleton.LocalClientId;

		if (NetworkManager.Singleton.ConnectedClients.TryGetValue(localClientId, out var client))
		{
			userPlayer = client.PlayerObject.GetComponent<Player>();
			cinemachineCamera = userPlayer.VirtualCamera;
			Debug.Log($"내 플레이어 오브젝트: {userPlayer.name}");
		}

		userPlayer.OnDieLocal += SetDie;
		userPlayer.OnReviveLocal += SetRevive;

		if (players.Count == 0)
		{
			SetPlayersCamInLoby();
		}

		PlayersManager.Instance.OnPlayerAdded += SetPlayersCam;
	}

	private void SetPlayersCamInLoby()
	{
		foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
		{
			if (client.PlayerObject != null && !playersNetworkObject.Contains(client.PlayerObject))
			{
				playersNetworkObject.Add(client.PlayerObject);
			}
		}

		// 내 플레이어 찾기
		NetworkObject localPlayerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
		if (playersNetworkObject.Remove(localPlayerObject))
		{
			playersNetworkObject.Insert(0, localPlayerObject); // 리스트 맨 앞에 배치
		}

		foreach (var client in playersNetworkObject)
		{
			players.Add(client.GetComponent<Player>());
		}
	}

	private void SetPlayersCam(ulong networkLong)
	{
		NetworkObject temptPlayer = null;

		if (NetworkManager.Singleton.ConnectedClients.TryGetValue(networkLong, out var netobject))
		{
			temptPlayer = netobject.PlayerObject;
			Debug.Log($"추가 할 오브젝트: {temptPlayer.name} , Ulong {networkLong}");
		}

		playersNetworkObject.Add(temptPlayer);

		Player temptCam = temptPlayer.GetComponent<Player>();

		players.Add(temptCam);		
	}
	
	private void SetDie()
	{
		isUserDie = true;

		if (players.Count > 1)
		{
			players[0].SetPlayerDieView(false);
			currentCameraIndex = 1; // 플레이어 제외한 첫 번째 카메라로 변경
			cinemachineCamera.Follow = players[currentCameraIndex].FirstPersonCamera.transform;
			//cinemachineCamera.LookAt = players[currentCameraIndex].transform;
			players[currentCameraIndex].SetPlayerDieView(true);
		}
	}

	private void SetRevive()
	{
		isUserDie = false;

		players[currentCameraIndex].SetPlayerDieView(false);

		currentCameraIndex = 0;
		// 플레이어가 부활하면 다시 0번 카메라(플레이어 카메라)로 돌아간다.
		cinemachineCamera.Follow = players[currentCameraIndex].FirstPersonCamera.transform;
		//cinemachineCamera.LookAt = players[currentCameraIndex].FirstPersonCamera.transform;
	}

	private void Update()
	{
		if (!isUserDie || players.Count == 0)
		{
			return;
		}


		if (Input.GetMouseButtonDown(0)) // 좌클릭
		{
			ChangeToNextCamera();
		}
	}

	private void ChangeToNextCamera()
	{
		if (players.Count <= 1) return; // 플레이어 제외하고 카메라가 없으면 실행 안 함

		players[currentCameraIndex].SetPlayerDieView(false);
		currentCameraIndex = (currentCameraIndex + 1) % players.Count;

		// 플레이어 카메라(0번)는 건너뛴다.
		if (currentCameraIndex == 0)
		{
			currentCameraIndex = 1;
		}

		cinemachineCamera.Follow = players[currentCameraIndex].FirstPersonCamera.transform;
		//cinemachineCamera.LookAt = players[currentCameraIndex].FirstPersonCamera.transform;
		players[currentCameraIndex].SetPlayerDieView(true);

	}

}
