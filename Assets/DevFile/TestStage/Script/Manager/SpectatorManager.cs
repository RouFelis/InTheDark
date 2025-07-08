using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Cinemachine;

public class SpectatorManager : MonoBehaviour
{
    // 로컬 플레이어(Player) 객체를 참조합니다.
    [SerializeField] private Player userPlayer;
    // 네트워크 상의 플레이어(NetworkObject)들을 저장하는 리스트입니다.
    [SerializeField] private List<NetworkObject> playersNetworkObject = new List<NetworkObject>();
    // Player 스크립트를 갖는 플레이어들을 저장하는 리스트입니다.
    [SerializeField] private List<Player> players = new List<Player>();
    // 로컬 플레이어의 생존 여부를 나타내는 변수입니다.
    [SerializeField] private bool isUserDie = false;

    // 시네머신 가상 카메라 참조 변수입니다.
    [SerializeField] private CinemachineVirtualCamera cinemachineCamera;
    // 현재 활성화된 카메라(또는 플레이어)의 인덱스를 나타냅니다.
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
            xRotation = Mathf.Clamp(xRotation, -90f, 90f); // 위아래 회전을 제한해서 뒤집히지 않게 함

            rotationTransform.localRotation = Quaternion.Euler(xRotation, rotationTransform.localEulerAngles.y, 0f);
            rotationTransform.Rotate(Vector3.up * mouseX, Space.World);
        }
		
	}

	private void Start()
    {
        // 로컬 클라이언트의 ID를 가져옵니다.
        ulong localClientId = NetworkManager.Singleton.LocalClientId;

        // 네트워크 매니저에서 로컬 클라이언트의 플레이어 객체를 가져와 userPlayer에 할당합니다.
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(localClientId, out var client))
        {
            // 클라이언트의 네트워크 오브젝트에서 Player 컴포넌트를 가져옵니다.
            userPlayer = client.PlayerObject.GetComponent<Player>();
            // 로컬 플레이어의 가상 카메라 참조를 할당합니다.
            cinemachineCamera = userPlayer.VirtualCamera;
            Debug.Log($"내 플레이어 오브젝트: {userPlayer.PlayerName}");
        }

        // 로컬 이벤트 등록: 플레이어 사망 및 부활 시 호출되는 메서드 등록
        userPlayer.OnDieLocal += SetDie;
        userPlayer.OnReviveLocal += SetRevive;

        // 만약 플레이어 리스트가 비어 있다면 로비 상황을 위한 플레이어 카메라 설정을 진행합니다.
        if (players.Count == 0)
        {
            SetPlayersCamInLoby();
        }

        // 새로운 플레이어가 추가되었을 때 호출될 델리게이트에 SetPlayersCam 메서드 등록
        PlayersManager.Instance.OnPlayerAdded += SetPlayersCam;
        PlayersManager.Instance.OnPlayerRemoved += RemovePlayersCam;
        KeySettingsManager.Instance.sensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
    }

    // 로비 상태에서 플레이어들의 카메라 설정을 위한 초기화 함수입니다.
    private void SetPlayersCamInLoby()
    {
        // 네트워크에 연결된 모든 클라이언트 순회
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            // 클라이언트의 플레이어 오브젝트가 존재하고 이미 리스트에 포함되지 않았다면 추가합니다.
            if (client.PlayerObject != null && !playersNetworkObject.Contains(client.PlayerObject))
            {
                playersNetworkObject.Add(client.PlayerObject);
            }
        }

        // 현재 로컬 플레이어의 네트워크 오브젝트를 찾습니다.
        NetworkObject localPlayerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
        // 로컬 플레이어의 오브젝트가 리스트에서 제거되면 맨 앞에 다시 삽입하여 우선순위를 높입니다.
        if (playersNetworkObject.Remove(localPlayerObject))
        {
            playersNetworkObject.Insert(0, localPlayerObject); // 로컬 플레이어는 인덱스 0에 위치
        }

        // 각 네트워크 오브젝트에 대해 Player 컴포넌트를 가져와 players 리스트에 추가합니다.
        foreach (var client in playersNetworkObject)
        {
            players.Add(client.GetComponent<Player>());
        }
    }

    // 새 플레이어가 추가되었을 때 호출되는 함수입니다.
    // networkLong: 추가된 플레이어의 네트워크 아이디
    private void SetPlayersCam(ulong networkLong)
    {
        NetworkObject temptPlayer = null;

        // 추가된 플레이어의 네트워크 객체를 찾아 temptPlayer에 할당
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(networkLong, out var netobject))
        {
            temptPlayer = netobject.PlayerObject;
            Debug.Log($"추가 할 오브젝트: {temptPlayer.name} , Ulong {networkLong}");
        }

        // 네트워크 객체 리스트에 중복이 없을 때만 추가
        if (!playersNetworkObject.Contains(temptPlayer))
        {
            playersNetworkObject.Add(temptPlayer);
        }

        // 플레이어 리스트에 중복이 없을 때만 추가
        Player temptCam = temptPlayer.GetComponent<Player>();
        if (!players.Contains(temptCam))
        {
            players.Add(temptCam);
        }
    }


    // 새 플레이어가 삭제되었을 때 호출되는 함수입니다.
    // networkLong: 삭제된 플레이어의 네트워크 아이디
    private void RemovePlayersCam(ulong networkLong)
    {
        NetworkObject temptPlayer = null;

        // 추가된 플레이어의 네트워크 객체를 찾아 temptPlayer에 할당
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(networkLong, out var netobject))
        {
            temptPlayer = netobject.PlayerObject;
            Debug.Log($"추가 할 오브젝트: {temptPlayer.name} , Ulong {networkLong}");
        }

        // 네트워크 객체 리스트와 플레이어 리스트에 새 플레이어 추가
        playersNetworkObject.Remove(temptPlayer);
        Player temptCam = temptPlayer.GetComponent<Player>();
        players.Remove(temptCam);
    }


    // 로컬 플레이어 사망 시 호출되는 메서드
    private void SetDie()
    {     
        // 플레이어 리스트에 2명 이상 있을 경우(즉, 다른 플레이어가 있을 때)
        if (players.Count > 1)
        {
            // 로컬 플레이어(인덱스 0)의 사망 뷰를 비활성화합니다.
            players[0].SetPlayerDieView(false);
            // 현재 카메라 인덱스를 첫 번째 사망 플레이어의 카메라로 변경 (인덱스 1)
            currentCameraIndex = 1;

            Cinemachine3rdPersonFollow followComponent = cinemachineCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            if (followComponent != null)
            {
                // CameraDistance 값을 변경함.
                followComponent.CameraDistance = camDistance;
            }
            else
            {
                Debug.LogError("Cinemachine3rdPersonFollow 컴포넌트를 찾을 수 없습니다.");
            }

            // 시네머신 카메라의 Follow 대상(즉, 시점)을 바꿉니다.
            cinemachineCamera.Follow = players[currentCameraIndex].ThirdPersonTransform;
            //회전객체설정
            rotationTransform =  players[currentCameraIndex].ThirdPersonTransform;
            // ※ LookAt 설정도 가능하지만 현재 주석 처리됨
            //players[currentCameraIndex].SetPlayerDieView(true)를 호출하여 사망 시의 뷰(예: 레이아웃 또는 UI)를 활성화합니다.
            players[currentCameraIndex].SetPlayerDieView(false);

            isUserDie = true;
        }
    }

	// 플레이어 부활 시 호출되는 메서드
	private void SetRevive()
	{
		isUserDie = false;

		// 현재 보고 있던 플레이어의 사망 뷰를 비활성화합니다.
		players[currentCameraIndex].SetPlayerDieView(false);

		// 부활하면 다시 로컬 플레이어(인덱스 0)로 카메라를 변경합니다.
		currentCameraIndex = 0;

		cinemachineCamera.Follow = players[currentCameraIndex].FirstPersonCamera.transform;

		//회전객체설정
		rotationTransform = null;
		// LookAt 설정이 필요한 경우 주석 해제 가능

		Cinemachine3rdPersonFollow followComponent = cinemachineCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

		cinemachineCamera.LookAt = null;


		if (followComponent != null)
		{
			// CameraDistance 값을 변경함.
			followComponent.CameraDistance = 0;
		}
		else
		{
			Debug.LogError("Cinemachine3rdPersonFollow 컴포넌트를 찾을 수 없습니다. 님큰일남 ㅋㅋ");
		}

		players[currentCameraIndex].SetPlayerDieView(true);
    }

    private void Update()
    {
        // 로컬 플레이어가 살아있거나 플레이어 리스트가 없으면 아래 로직을 실행하지 않음
        if (!isUserDie || players.Count == 0)
        {
            return;
        }

        // 마우스 좌클릭 입력이 들어오면 카메라 전환을 진행합니다.
        if (Input.GetMouseButtonDown(0)) // 좌클릭
        {
            ChangeToNextCamera();
        }
    }

    // 다른 플레이어 카메라로 전환하는 함수
    private void ChangeToNextCamera()
    {
        // 만약 다른 플레이어(로컬 플레이어 제외)의 카메라가 없다면 실행하지 않음
        if (players.Count <= 1) return;

        // 현재 카메라의 사망 뷰를 비활성화
        players[currentCameraIndex].SetPlayerDieView(false);
        // 현재 카메라 인덱스를 순환하여 다음으로 전환 (리스트의 끝에서는 순환)
        currentCameraIndex = (currentCameraIndex + 1) % players.Count;

        // 인덱스 0은 로컬 플레이어의 카메라이므로 건너뜁니다.
        if (currentCameraIndex == 0)
        {
            currentCameraIndex = 1;
        }

        // 시네머신 카메라의 Follow 대상을 변경합니다.
        cinemachineCamera.Follow = players[currentCameraIndex].ThirdPersonTransform;
        // 회전객체설정
        rotationTransform = players[currentCameraIndex].ThirdPersonTransform;
        // LookAt 설정이 필요한 경우 주석 해제 가능
        cinemachineCamera.LookAt = players[currentCameraIndex].FirstPersonCamera.transform;
        // 전환된 카메라의 사망 뷰를 활성화합니다.
        players[currentCameraIndex].SetPlayerDieView(false);
    }

    private void SetMouseSensitivity(float Value)
	{
        mouseSensitivity = Value;
    }
}
