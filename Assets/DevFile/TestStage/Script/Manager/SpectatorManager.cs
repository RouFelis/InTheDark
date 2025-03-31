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
			Debug.Log($"�� �÷��̾� ������Ʈ: {userPlayer.name}");
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

		// �� �÷��̾� ã��
		NetworkObject localPlayerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
		if (playersNetworkObject.Remove(localPlayerObject))
		{
			playersNetworkObject.Insert(0, localPlayerObject); // ����Ʈ �� �տ� ��ġ
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
			Debug.Log($"�߰� �� ������Ʈ: {temptPlayer.name} , Ulong {networkLong}");
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
			currentCameraIndex = 1; // �÷��̾� ������ ù ��° ī�޶�� ����
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
		// �÷��̾ ��Ȱ�ϸ� �ٽ� 0�� ī�޶�(�÷��̾� ī�޶�)�� ���ư���.
		cinemachineCamera.Follow = players[currentCameraIndex].FirstPersonCamera.transform;
		//cinemachineCamera.LookAt = players[currentCameraIndex].FirstPersonCamera.transform;
	}

	private void Update()
	{
		if (!isUserDie || players.Count == 0)
		{
			return;
		}


		if (Input.GetMouseButtonDown(0)) // ��Ŭ��
		{
			ChangeToNextCamera();
		}
	}

	private void ChangeToNextCamera()
	{
		if (players.Count <= 1) return; // �÷��̾� �����ϰ� ī�޶� ������ ���� �� ��

		players[currentCameraIndex].SetPlayerDieView(false);
		currentCameraIndex = (currentCameraIndex + 1) % players.Count;

		// �÷��̾� ī�޶�(0��)�� �ǳʶڴ�.
		if (currentCameraIndex == 0)
		{
			currentCameraIndex = 1;
		}

		cinemachineCamera.Follow = players[currentCameraIndex].FirstPersonCamera.transform;
		//cinemachineCamera.LookAt = players[currentCameraIndex].FirstPersonCamera.transform;
		players[currentCameraIndex].SetPlayerDieView(true);

	}

}
