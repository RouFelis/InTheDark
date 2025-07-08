using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Cinemachine;

public class SpectatorManager : MonoBehaviour
{
    // ���� �÷��̾�(Player) ��ü�� �����մϴ�.
    [SerializeField] private Player userPlayer;
    // ��Ʈ��ũ ���� �÷��̾�(NetworkObject)���� �����ϴ� ����Ʈ�Դϴ�.
    [SerializeField] private List<NetworkObject> playersNetworkObject = new List<NetworkObject>();
    // Player ��ũ��Ʈ�� ���� �÷��̾���� �����ϴ� ����Ʈ�Դϴ�.
    [SerializeField] private List<Player> players = new List<Player>();
    // ���� �÷��̾��� ���� ���θ� ��Ÿ���� �����Դϴ�.
    [SerializeField] private bool isUserDie = false;

    // �ó׸ӽ� ���� ī�޶� ���� �����Դϴ�.
    [SerializeField] private CinemachineVirtualCamera cinemachineCamera;
    // ���� Ȱ��ȭ�� ī�޶�(�Ǵ� �÷��̾�)�� �ε����� ��Ÿ���ϴ�.
    [SerializeField] private int currentCameraIndex = 0;

    [SerializeField] private float camDistance = 6f;

    [SerializeField] private Transform rotationTransform;

    private float mouseSensitivity = 100f;
    float xRotation = 0f;

    private void FixedUpdate()
	{
		if (isUserDie)
		{
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f); // ���Ʒ� ȸ���� �����ؼ� �������� �ʰ� ��

            rotationTransform.localRotation = Quaternion.Euler(xRotation, rotationTransform.localEulerAngles.y, 0f);
            rotationTransform.Rotate(Vector3.up * mouseX, Space.World);
        }
		
	}

	private void Start()
    {
        // ���� Ŭ���̾�Ʈ�� ID�� �����ɴϴ�.
        ulong localClientId = NetworkManager.Singleton.LocalClientId;

        // ��Ʈ��ũ �Ŵ������� ���� Ŭ���̾�Ʈ�� �÷��̾� ��ü�� ������ userPlayer�� �Ҵ��մϴ�.
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(localClientId, out var client))
        {
            // Ŭ���̾�Ʈ�� ��Ʈ��ũ ������Ʈ���� Player ������Ʈ�� �����ɴϴ�.
            userPlayer = client.PlayerObject.GetComponent<Player>();
            // ���� �÷��̾��� ���� ī�޶� ������ �Ҵ��մϴ�.
            cinemachineCamera = userPlayer.VirtualCamera;
            Debug.Log($"�� �÷��̾� ������Ʈ: {userPlayer.PlayerName}");
        }

        // ���� �̺�Ʈ ���: �÷��̾� ��� �� ��Ȱ �� ȣ��Ǵ� �޼��� ���
        userPlayer.OnDieLocal += SetDie;
        userPlayer.OnReviveLocal += SetRevive;

        // ���� �÷��̾� ����Ʈ�� ��� �ִٸ� �κ� ��Ȳ�� ���� �÷��̾� ī�޶� ������ �����մϴ�.
        if (players.Count == 0)
        {
            SetPlayersCamInLoby();
        }

        // ���ο� �÷��̾ �߰��Ǿ��� �� ȣ��� ��������Ʈ�� SetPlayersCam �޼��� ���
        PlayersManager.Instance.OnPlayerAdded += SetPlayersCam;
        PlayersManager.Instance.OnPlayerRemoved += RemovePlayersCam;
        KeySettingsManager.Instance.sensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
    }

    // �κ� ���¿��� �÷��̾���� ī�޶� ������ ���� �ʱ�ȭ �Լ��Դϴ�.
    private void SetPlayersCamInLoby()
    {
        // ��Ʈ��ũ�� ����� ��� Ŭ���̾�Ʈ ��ȸ
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            // Ŭ���̾�Ʈ�� �÷��̾� ������Ʈ�� �����ϰ� �̹� ����Ʈ�� ���Ե��� �ʾҴٸ� �߰��մϴ�.
            if (client.PlayerObject != null && !playersNetworkObject.Contains(client.PlayerObject))
            {
                playersNetworkObject.Add(client.PlayerObject);
            }
        }

        // ���� ���� �÷��̾��� ��Ʈ��ũ ������Ʈ�� ã���ϴ�.
        NetworkObject localPlayerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
        // ���� �÷��̾��� ������Ʈ�� ����Ʈ���� ���ŵǸ� �� �տ� �ٽ� �����Ͽ� �켱������ ���Դϴ�.
        if (playersNetworkObject.Remove(localPlayerObject))
        {
            playersNetworkObject.Insert(0, localPlayerObject); // ���� �÷��̾�� �ε��� 0�� ��ġ
        }

        // �� ��Ʈ��ũ ������Ʈ�� ���� Player ������Ʈ�� ������ players ����Ʈ�� �߰��մϴ�.
        foreach (var client in playersNetworkObject)
        {
            players.Add(client.GetComponent<Player>());
        }
    }

    // �� �÷��̾ �߰��Ǿ��� �� ȣ��Ǵ� �Լ��Դϴ�.
    // networkLong: �߰��� �÷��̾��� ��Ʈ��ũ ���̵�
    private void SetPlayersCam(ulong networkLong)
    {
        NetworkObject temptPlayer = null;

        // �߰��� �÷��̾��� ��Ʈ��ũ ��ü�� ã�� temptPlayer�� �Ҵ�
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(networkLong, out var netobject))
        {
            temptPlayer = netobject.PlayerObject;
            Debug.Log($"�߰� �� ������Ʈ: {temptPlayer.name} , Ulong {networkLong}");
        }

        // ��Ʈ��ũ ��ü ����Ʈ�� �ߺ��� ���� ���� �߰�
        if (!playersNetworkObject.Contains(temptPlayer))
        {
            playersNetworkObject.Add(temptPlayer);
        }

        // �÷��̾� ����Ʈ�� �ߺ��� ���� ���� �߰�
        Player temptCam = temptPlayer.GetComponent<Player>();
        if (!players.Contains(temptCam))
        {
            players.Add(temptCam);
        }
    }


    // �� �÷��̾ �����Ǿ��� �� ȣ��Ǵ� �Լ��Դϴ�.
    // networkLong: ������ �÷��̾��� ��Ʈ��ũ ���̵�
    private void RemovePlayersCam(ulong networkLong)
    {
        NetworkObject temptPlayer = null;

        // �߰��� �÷��̾��� ��Ʈ��ũ ��ü�� ã�� temptPlayer�� �Ҵ�
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(networkLong, out var netobject))
        {
            temptPlayer = netobject.PlayerObject;
            Debug.Log($"�߰� �� ������Ʈ: {temptPlayer.name} , Ulong {networkLong}");
        }

        // ��Ʈ��ũ ��ü ����Ʈ�� �÷��̾� ����Ʈ�� �� �÷��̾� �߰�
        playersNetworkObject.Remove(temptPlayer);
        Player temptCam = temptPlayer.GetComponent<Player>();
        players.Remove(temptCam);
    }


    // ���� �÷��̾� ��� �� ȣ��Ǵ� �޼���
    private void SetDie()
    {     
        // �÷��̾� ����Ʈ�� 2�� �̻� ���� ���(��, �ٸ� �÷��̾ ���� ��)
        if (players.Count > 1)
        {
            // ���� �÷��̾�(�ε��� 0)�� ��� �並 ��Ȱ��ȭ�մϴ�.
            players[0].SetPlayerDieView(false);
            // ���� ī�޶� �ε����� ù ��° ��� �÷��̾��� ī�޶�� ���� (�ε��� 1)
            currentCameraIndex = 1;

            Cinemachine3rdPersonFollow followComponent = cinemachineCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            if (followComponent != null)
            {
                // CameraDistance ���� ������.
                followComponent.CameraDistance = camDistance;
            }
            else
            {
                Debug.LogError("Cinemachine3rdPersonFollow ������Ʈ�� ã�� �� �����ϴ�.");
            }

            // �ó׸ӽ� ī�޶��� Follow ���(��, ����)�� �ٲߴϴ�.
            cinemachineCamera.Follow = players[currentCameraIndex].ThirdPersonTransform;
            //ȸ����ü����
            rotationTransform =  players[currentCameraIndex].ThirdPersonTransform;
            // �� LookAt ������ ���������� ���� �ּ� ó����
            //players[currentCameraIndex].SetPlayerDieView(true)�� ȣ���Ͽ� ��� ���� ��(��: ���̾ƿ� �Ǵ� UI)�� Ȱ��ȭ�մϴ�.
            players[currentCameraIndex].SetPlayerDieView(false);

            isUserDie = true;
        }
    }

	// �÷��̾� ��Ȱ �� ȣ��Ǵ� �޼���
	private void SetRevive()
	{
		isUserDie = false;

		// ���� ���� �ִ� �÷��̾��� ��� �並 ��Ȱ��ȭ�մϴ�.
		players[currentCameraIndex].SetPlayerDieView(false);

		// ��Ȱ�ϸ� �ٽ� ���� �÷��̾�(�ε��� 0)�� ī�޶� �����մϴ�.
		currentCameraIndex = 0;

		cinemachineCamera.Follow = players[currentCameraIndex].FirstPersonCamera.transform;

		//ȸ����ü����
		rotationTransform = null;
		// LookAt ������ �ʿ��� ��� �ּ� ���� ����

		Cinemachine3rdPersonFollow followComponent = cinemachineCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

		cinemachineCamera.LookAt = null;


		if (followComponent != null)
		{
			// CameraDistance ���� ������.
			followComponent.CameraDistance = 0;
		}
		else
		{
			Debug.LogError("Cinemachine3rdPersonFollow ������Ʈ�� ã�� �� �����ϴ�. ��ū�ϳ� ����");
		}

		players[currentCameraIndex].SetPlayerDieView(true);
    }

    private void Update()
    {
        // ���� �÷��̾ ����ְų� �÷��̾� ����Ʈ�� ������ �Ʒ� ������ �������� ����
        if (!isUserDie || players.Count == 0)
        {
            return;
        }

        // ���콺 ��Ŭ�� �Է��� ������ ī�޶� ��ȯ�� �����մϴ�.
        if (Input.GetMouseButtonDown(0)) // ��Ŭ��
        {
            ChangeToNextCamera();
        }
    }

    // �ٸ� �÷��̾� ī�޶�� ��ȯ�ϴ� �Լ�
    private void ChangeToNextCamera()
    {
        // ���� �ٸ� �÷��̾�(���� �÷��̾� ����)�� ī�޶� ���ٸ� �������� ����
        if (players.Count <= 1) return;

        // ���� ī�޶��� ��� �並 ��Ȱ��ȭ
        players[currentCameraIndex].SetPlayerDieView(false);
        // ���� ī�޶� �ε����� ��ȯ�Ͽ� �������� ��ȯ (����Ʈ�� �������� ��ȯ)
        currentCameraIndex = (currentCameraIndex + 1) % players.Count;

        // �ε��� 0�� ���� �÷��̾��� ī�޶��̹Ƿ� �ǳʶݴϴ�.
        if (currentCameraIndex == 0)
        {
            currentCameraIndex = 1;
        }

        // �ó׸ӽ� ī�޶��� Follow ����� �����մϴ�.
        cinemachineCamera.Follow = players[currentCameraIndex].ThirdPersonTransform;
        // ȸ����ü����
        rotationTransform = players[currentCameraIndex].ThirdPersonTransform;
        // LookAt ������ �ʿ��� ��� �ּ� ���� ����
        cinemachineCamera.LookAt = players[currentCameraIndex].FirstPersonCamera.transform;
        // ��ȯ�� ī�޶��� ��� �並 Ȱ��ȭ�մϴ�.
        players[currentCameraIndex].SetPlayerDieView(false);
    }

    private void SetMouseSensitivity(float Value)
	{
        mouseSensitivity = Value;
    }
}
