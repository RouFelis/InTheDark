using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Animations.Rigging;
using Cinemachine;

public class playerMoveController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float runSpeedMultiplier = 1.5f;
    [SerializeField] private float rotationSpeed = 2.0f;
    [SerializeField] private float gravity = 20.0f;
    [SerializeField] private float jumpForce = 8.0f;

    [Header("Camera & Head Rotation")]
    [SerializeField] protected Camera playerCamera;
    [SerializeField] protected CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform headTarget;
    [SerializeField] private Transform camTarget;
    [SerializeField] private float lookSpeed = 2.0f;
    [SerializeField] private float lookXLimit = 60.0f;

    [Header("Animation")]
    [SerializeField] protected Animator animator;

    [Header("Networking")]
  //  [SerializeField] private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
  //  [SerializeField] private NetworkVariable<Vector3> networkRotation = new NetworkVariable<Vector3>();
 //   [SerializeField] private NetworkVariable<Quaternion> networkHeadRotation = new NetworkVariable<Quaternion>();
    [SerializeField] private NetworkVariable<bool> isEventPlaying = new NetworkVariable<bool>(false);
    [SerializeField] private NetworkVariable<bool> isWalking = new NetworkVariable<bool>(false , writePerm:NetworkVariableWritePermission.Owner);
    [SerializeField] private NetworkVariable<bool> isRunning = new NetworkVariable<bool>(false , writePerm:NetworkVariableWritePermission.Owner);

    [Header("GroundChecker")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheckPosition;

    [Header("PlayerAim")]
    [SerializeField] private GameObject handAimTargetPrefab;
    [SerializeField] private GameObject spawnedObject;
    [SerializeField] private Transform handAimTarget;
    [SerializeField] private List<ConstraintConfig> playerConstraints = new List<ConstraintConfig>();
    [SerializeField] private float distanceFromCamera = 1f;
    [SerializeField] private float aimMaxDistance = 100f;
    [SerializeField] private RigBuilder rigBuilder_firstPerson;
    [SerializeField] private RigBuilder rigBuilder_thridPerson;
    [SerializeField] private float smoothTime = 0.1f; // 이동 감속 속도

    public LayerMask layerMask;

    private Vector3 aimTargetPosition; // 에임타겟 포지션
    private Vector3 velocity = Vector3.zero;
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0.0f;
    private RaycastHit hit;

    [Header("Head Bobbing Settings")]
    public float bobbingSpeed = 14f; // 머리 흔들림 속도
    public float bobbingAmount = 0.05f; // 머리 흔들림 강도
    public float walkBobbingAmount = 0.05f; // 머리 흔들림 강도
    public float RunningBobbingAmount = 0.05f; // 머리 흔들림 강도
    public float midpoint = 0f; // 기본 카메라 높이 (플레이어 머리 위치)

    private float timer = 0f; // 시간 값을 추적
    private bool pause = false; //퍼즈


    private bool mouseControl = true;
    private bool isJumping = false;
    private bool isFalling = false;
    private Quaternion savedHeadRotation;

    private Interacter interacter;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerCamera.gameObject.SetActive(true);
            virtualCamera.gameObject.SetActive(true);
            FixedMouse();
            interacter = GetComponent<Interacter>();
            MenuManager.Instance.OnPause += FreeMouse;
            MenuManager.Instance.OnResume += FixedMouse;
        }
        else
        {
            playerCamera.gameObject.SetActive(false);
            virtualCamera.gameObject.SetActive(false);
        }
    }

    public virtual void Start()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (IsOwner)
        {
            SpawnAndNotifyServerRpc(OwnerClientId);
		}
		else
		{
            InitRagdolls();
        }
    }

    private void LateUpdate()
    {
        if (IsOwner)
        {
            if (!isEventPlaying.Value && !pause)
            {
                HandleInput();
				// HandleInputServerRpc(InputMoveNormal() , InputMouseNormal());
			}
			else
			{
                EventPlayingStop();
			}
        }
		else
		{

        }
      /*  if(handAimTarget == null)
            FindAimTargetObject();*/
        UpdateAnimator();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Lobby")
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (IsOwner)
        {
            MenuManager.Instance.OnPause -= FreeMouse;
            MenuManager.Instance.OnResume -= FixedMouse;
        }
    }

    private Vector2 InputMoveNormal()
	{
        // 이동 입력
        float horizontal = Input.GetAxis("Horizontal");
        Debug.Log("horizontal : " + horizontal);
        float vertical = Input.GetAxis("Vertical");
        Debug.Log("vertical : " + vertical);
        return new Vector2(horizontal, vertical);
    }
    private Vector2 InputMouseNormal()
    {
        // 이동 입력
        float horizontal = Input.GetAxis("Mouse X");
        float vertical = Input.GetAxis("Mouse Y");
        return new Vector2(horizontal, vertical);
    }

    private void HandleInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        Vector3 inputDirection = forward * vertical + right * horizontal;

        if (inputDirection.magnitude > 1)
            inputDirection.Normalize();

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float speedMultiplier = isRunning ? runSpeedMultiplier : 1.0f;
        moveDirection.x = inputDirection.x * walkSpeed * speedMultiplier;
        moveDirection.z = inputDirection.z * walkSpeed * speedMultiplier;

        characterController.Move(moveDirection * Time.deltaTime);

        // 점프 처리
        // if (characterController.isGrounded)
        if (IsGrounded())
        {
            if (isJumping)
            {
                moveDirection.y = 0;
                isJumping = false;
            }

            if (Input.GetButtonDown("Jump"))
            {
                moveDirection.y = jumpForce;
                isJumping = true;
                animator.speed = 1.0f; // 점프 중에는 기본 속도
            }
        }
        else
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        if (mouseControl)
        {
            // 마우스 회전 입력
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;

            camTarget.transform.localRotation = Quaternion.Euler(new Vector3(rotationX, 0, 0));
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, 90f);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        HandleAim();
        HeadBobbing();
    }

    private void EventPlayingStop()
	{
        moveDirection = Vector3.zero;
        SetPlayerAimPos(new Vector3(0, 2f, 10f));
    }


    private void HeadBobbing()
	{
        if (characterController != null && IsGrounded() && characterController.velocity.magnitude > 0.1f)
        {
            // 걷는 중일 때만 흔들림
            timer += Time.deltaTime * bobbingSpeed;
            float bobbingOffset = Mathf.Sin(timer) * bobbingAmount; // 사인파로 흔들림 계산
            Vector3 newPosition = new Vector3(playerCamera.transform.localPosition.x, midpoint + bobbingOffset, playerCamera.transform.localPosition.z);
            playerCamera.transform.localPosition = newPosition;
        }
        else
        {
            // 멈출 때는 Y축 위치를 초기화
            timer = 0f;
            Vector3 resetPosition = new Vector3(playerCamera.transform.localPosition.x, midpoint, playerCamera.transform.localPosition.z);
            playerCamera.transform.localPosition = resetPosition;
        }
    }


    private void HandleAim()
	{
        //허리 각도 조절이랑 에임 조절용
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, aimMaxDistance, layerMask))
        {
            Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * hit.distance, Color.red, 0.1f);

            // 충돌한 경우 hit.point로 설정
            aimTargetPosition = hit.point;
            // 충돌하지 않은 경우, maxDistance 지점으로 설정
            //handAimTarget.transform.position = playerCamera.transform.position + playerCamera.transform.forward * aimMaxDistance;
        }
        else
        {
            Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * hit.distance, Color.red, 0.1f);
            // 충돌하지 않은 경우, maxDistance 지점으로 설정
            aimTargetPosition = playerCamera.transform.position + playerCamera.transform.forward * aimMaxDistance;
        }

        handAimTarget.transform.position = Vector3.SmoothDamp(handAimTarget.transform.position, aimTargetPosition, ref velocity, smoothTime);

        Vector3 cameraForward = playerCamera.transform.forward.normalized;
        Vector3 cameraPosition = playerCamera.transform.position;
    }

    private void UpdateAnimator()
    {
		if (IsOwner)
        {
            isWalking.Value = moveDirection.x != 0 || moveDirection.z != 0;
            isRunning.Value = isWalking.Value && Input.GetKey(KeyCode.LeftShift);
        }

        animator.SetBool("IsWalking", isWalking.Value);
        //  animator.SetBool("IsRunning", isRunning);
        animator.SetBool("IsJumping", isJumping);
        //animator.SetBool("IsJumping", isFalling);

        // 애니메이션 속도 및 사운드 동기화
        if (isWalking.Value)
        {
			if (isRunning.Value)
			{
                animator.speed = 1.4f;
                bobbingSpeed = 18f; // 걷기 속도
                bobbingAmount = RunningBobbingAmount; // 걷기 흔들림 강도
            }
			else
			{
                animator.speed = 1f;
                bobbingSpeed = 13f; // 달리기 속도
                bobbingAmount = walkBobbingAmount; // 달리기 흔들림 강도
            }
        }
        else
        {
            animator.speed = 1.0f; // 기본 속도
        }
    }


    public void EventToggle(bool value, GameObject target)
    {
        isEventPlaying.Value = value;
        //enabled = !value;
        interacter.enabled = !value;

        if (value)
        {
            FreeMouse();
            SetAimMode(true , target);
        }
        else
        {
            FixedMouse();
            SetAimMode();
        }
    }

    //특수상황에 사용하는 에임 포즈.
    public void SetPlayerAimPos(Vector3 vector)
	{
        handAimTarget.transform.position = vector;
	}

    public void SetAimMode(bool enableComposer = false, GameObject target = null)
    {
        if (virtualCamera == null) return;

        if (enableComposer)
        {
            // Composer 설정 (LookAt 사용 가능)
            var composer = virtualCamera.GetCinemachineComponent<CinemachineComposer>();
            if (composer == null)
            {
                virtualCamera.AddCinemachineComponent<CinemachineComposer>();
            }
            virtualCamera.LookAt = target.transform; // 카메라가 타겟을 바라보도록 설정

        }
        else
        {
            // Do Nothing 설정 (LookAt 무시)
            virtualCamera.DestroyCinemachineComponent<CinemachineComposer>();
            virtualCamera.LookAt = null; // 비우기
        }
    }



    private void FixedMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pause = false;
    }

    private void FreeMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pause = true;
    }

    public void SetMouseControl(bool enable)
    {
        mouseControl = enable;
    }

    public bool IsGrounded()
    {
        bool value = Physics.OverlapSphere(groundCheckPosition.position, groundCheckRadius, groundLayer).Length > 0;
        return value;
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        lookSpeed = sensitivity;
    }

    #region SpawnTargetPointer

    //Ragdoll 초기화
    public void InitRagdolls()
    {
        handAimTarget = this.transform.Find("HandTarget(Clone)");

        // 모든 MultiAimConstraint에 대해 소스 오브젝트 추가
        foreach (var config in playerConstraints)
        {
            AddSourceObject(config.constraint, handAimTarget, config.weight);
        }

        //빌더 초기화.
        if (rigBuilder_firstPerson != null)
        {
            rigBuilder_firstPerson.Build();
        }
        //빌더 초기화.
        if (rigBuilder_thridPerson != null)
        {
            rigBuilder_thridPerson.Build();
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void SpawnAndNotifyServerRpc(ulong clientId)
    {
        spawnedObject = Instantiate(handAimTargetPrefab);

        NetworkObject networkObject = spawnedObject.GetComponent<NetworkObject>();

        // 부모 설정: 서버에서 변경
        spawnedObject.transform.SetParent(this.transform);

        networkObject.SpawnWithOwnership(clientId);

        // 오브젝트를 관리 리스트에 추가
        handAimTarget = spawnedObject.transform;


        // 클라이언트들에게 스폰 정보 전달
        NotifyClientsOfSpawnClientRpc(networkObject.NetworkObjectId, clientId);


        Debug.Log($"Spawned object for client {clientId}");
    }

    [ClientRpc]
    private void NotifyClientsOfSpawnClientRpc(ulong networkObjectId, ulong ownerId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var networkObject))
        {
            handAimTarget = networkObject.transform;
            // 부모 설정: 서버에서 변경
            networkObject.transform.SetParent(this.transform);

            Debug.Log($"Client notified: Object {networkObject.gameObject.name} owned by {ownerId}");
        }

        // 모든 MultiAimConstraint에 대해 소스 오브젝트 추가
        foreach (var config in playerConstraints)
        {
            AddSourceObject(config.constraint, handAimTarget, config.weight);
        }

        //빌더 초기화.
        if (rigBuilder_firstPerson != null)
        {
            rigBuilder_firstPerson.Build();
        }
        //빌더 초기화.
        if (rigBuilder_thridPerson != null)
        {
            rigBuilder_thridPerson.Build();
        }
    }

    private void AddSourceObject(MultiAimConstraint constraint, Transform source, float weight)
    {
        if (constraint == null || source == null)
        {
            Debug.LogError("Constraint or Source Object is null!");
            return;
        }

        // 현재 설정된 소스 오브젝트 리스트 가져오기
        var sourceObjects = constraint.data.sourceObjects;

        // 소스 오브젝트 추가
        sourceObjects.Add(new WeightedTransform(source, weight));

        // 업데이트된 소스 오브젝트 리스트 설정
        constraint.data.sourceObjects = sourceObjects;

        Debug.Log($"Added source object '{source.name}' with weight {weight} to constraint on '{constraint.name}'");
    }

    #endregion
}

[System.Serializable]
public class ConstraintConfig
{
    public MultiAimConstraint constraint; // MultiAimConstraint 컴포넌트
    public float weight;                 // 설정된 가중치
}