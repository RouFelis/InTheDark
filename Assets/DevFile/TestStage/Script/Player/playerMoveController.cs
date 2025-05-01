using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Animations.Rigging;
using Cinemachine;
using UnityEngine.UI;
using UnityEngine.Animations;
using SaintsField.Playa;
using SaintsField;

public class playerMoveController : SaintsNetworkBehaviour
{
    [Header("Movement Settings")]
    [LayoutStart("Movement Settings", ELayout.FoldoutBox)]
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float runSpeedMultiplier = 1.5f;
    [SerializeField] private float rotationSpeed = 2.0f;
    [SerializeField] private float gravity = 20.0f;
    [SerializeField] private float jumpForce = 8.0f;


    [Header("Camera & Head Rotation")]
    [LayoutStart("Camera & Head Rotation", ELayout.FoldoutBox)]
    [SerializeField] protected Camera firstPersonCamera;
    [SerializeField] protected Camera thirdPersonCamera;
    [SerializeField] protected Transform camTarget;
    [SerializeField] protected CinemachineVirtualCamera virtualCamera;
    [SerializeField] protected GameObject ThirdpersonConstraintPrefab;
    [SerializeField] protected Transform ThirdpersonCameraTransform;
    [SerializeField] private Vector3 positionOffset = new Vector3(0, 0, 0); // 이동 감속 속도


    public Camera FirstPersonCamera { get => firstPersonCamera; }
    public Transform ThirdPersonTransform
    { 
        get { 
            if(ThirdpersonCameraTransform == null)
                SpawnWithConstraint();
            return ThirdpersonCameraTransform; 
        } 
    }
    public CinemachineVirtualCamera VirtualCamera { get => virtualCamera; }

    [SerializeField] private float lookSpeed = 2.0f;
    [SerializeField] private float lookXLimit = 60.0f;
    [SerializeField] private float smoothSpeed = 1f;

    [Header("Animation")]
    [SerializeField] protected Animator animator;

    [Header("Networking")]
    [LayoutStart("Networking", ELayout.FoldoutBox)]
    [SerializeField] private NetworkVariable<bool> isEventPlaying = new NetworkVariable<bool>(false);
    [SerializeField] private NetworkVariable<bool> isWalking = new NetworkVariable<bool>(false , writePerm:NetworkVariableWritePermission.Owner);
    [SerializeField] private NetworkVariable<bool> isRunning = new NetworkVariable<bool>(false , writePerm:NetworkVariableWritePermission.Owner);
    [SerializeField] private NetworkVariable<bool> isGrabItem = new NetworkVariable<bool>(value: false, writePerm: NetworkVariableWritePermission.Owner);
    [SerializeField] private NetworkVariable<float> currentStamina = new NetworkVariable<float>(value: 100, writePerm: NetworkVariableWritePermission.Owner);

    [Header("GroundChecker")]
    [LayoutStart("GroundChecker", ELayout.FoldoutBox)]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheckPosition;

    [Header("PlayerAim")]
    [LayoutStart("PlayerAim", ELayout.FoldoutBox)]
    [SerializeField] private GameObject handAimTargetPrefab;
    [SerializeField] private Transform handAimTarget;
    [SerializeField] private NetworkVariable<ulong> handAimTargetulong = new NetworkVariable<ulong>();
    [SerializeField] private List<ConstraintConfig> playerConstraints = new List<ConstraintConfig>();
    [SerializeField] private float distanceFromCamera = 1f;
    [SerializeField] private float aimMaxDistance = 100f;
    [SerializeField] private RigBuilder rigBuilder_firstPerson;
    [SerializeField] private RigBuilder rigBuilder_thridPerson;
    [SerializeField] private float smoothTime = 0.1f; // 이동 감속 속도



    [LayoutStart("MoveReference", ELayout.FoldoutBox)]
    public LayerMask layerMask;
    private Vector3 aimTargetPosition; // 에임타겟 포지션
    private Vector3 velocity = Vector3.zero;
    protected CharacterController characterController;
    [SerializeField]protected CapsuleCollider bodyCollider;
    [SerializeField] private Vector3 moveDirection = Vector3.zero;
    [SerializeField] private float rotationX = 0.0f;
    private RaycastHit hit;

    [Header("Head Bobbing Settings")]
    [LayoutStart("Head Bobbing Settings", ELayout.FoldoutBox)]
    public float bobbingSpeed = 14f; // 머리 흔들림 속도
    public float bobbingAmount = 0.05f; // 머리 흔들림 강도
    public float walkBobbingAmount = 0.05f; // 머리 흔들림 강도
    public float RunningBobbingAmount = 0.05f; // 머리 흔들림 강도
    public float midpoint = 0f; // 기본 카메라 높이 (플레이어 머리 위치)
    [LayoutEnd]

    private float timer = 0f; // 시간 값을 추적
    private bool pause = false; //퍼즈


    private bool mouseControl = true;
    private bool isJumping = false;
    private bool isFalling = false;
    private Quaternion savedHeadRotation;
    private Interacter interacter;


    [Header("Stamina Settings")]
    [SerializeField] private float Stamina => currentStamina.Value;
    [LayoutStart("Stamina Settings", ELayout.FoldoutBox)]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDecreaseRate = 10f; // 초당 감소량
    [SerializeField] private float staminaRegenRate = 5f; // 초당 회복량
    [SerializeField] private float staminaRegenDelay = 2f; // 회복 대기 시간
    [SerializeField] private float lastRunTime;

    private Image staminaBar;


    private IEnumerator InitCamera()
    {
        // PlaceableItemManager 오브젝트 찾기
        while (firstPersonCamera == null)
        {
            if (firstPersonCamera = GetComponentInChildren<Camera>())
            {
                Debug.Log("Find firstPersonCamera");
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }


        while (camTarget == null)
        {
            if (camTarget = transform.Find("CamPos(Clone)"))
            {
                Debug.Log("Find CamTarget");
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }

		if (IsOwner)
		{
            virtualCamera.gameObject.SetActive(true);
            FirstPersonCamera.gameObject.SetActive(true);
        }
		else
		{
            FirstPersonCamera.gameObject.SetActive(false);
		}
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
       
        if (IsOwner)
        {
            FixedMouse();
            interacter = GetComponent<Interacter>();
            MenuManager.Instance.OnPause += FreeMouse;
            MenuManager.Instance.OnResume += FixedMouse;
        }
        else
        {/*
            firstPersonCamera.gameObject.SetActive(false);
            virtualCamera.gameObject.SetActive(false);
            thirdPersonCamera.gameObject.SetActive(false);*/
        }
    }

    void SpawnWithConstraint()
    {
        // 프리팹 인스턴스 생성
        GameObject spawnedObj = Instantiate(ThirdpersonConstraintPrefab);
        ThirdpersonCameraTransform = spawnedObj.transform;

        // ParentConstraint 컴포넌트 가져오기
        ParentConstraint parentConstraint = spawnedObj.GetComponent<ParentConstraint>();
        if (parentConstraint == null)
        {
            Debug.LogError("ParentConstraint 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        // 소스 만들기
        ConstraintSource source = new ConstraintSource
        {
            sourceTransform = this.transform,
            weight = 1f
        };

        // 소스 추가 (인덱스 저장해두기)
        int sourceIndex = parentConstraint.AddSource(source);

        // 오프셋 적용
        parentConstraint.SetTranslationOffset(sourceIndex, positionOffset);

        // 옵션: 유지 오프셋 false로 설정 (원하는 경우 true로 변경 가능)
        parentConstraint.translationAtRest = Vector3.zero;
        parentConstraint.rotationAtRest = Vector3.zero;
        parentConstraint.constraintActive = true;
        parentConstraint.locked = false; // 위치/회전 고정
    }

    /*   [ServerRpc]
       private void SpawnSetCameraServerRpc(ulong clientID)
       {
           if (!IsServer) return; // 서버에서만 실행

           camTarget = Instantiate(camTargetPrefab).transform;

           NetworkObject camTargetNetworkObject = camTarget.GetComponent<NetworkObject>();

           camTargetNetworkObject.SpawnWithOwnership(clientID);
           camTargetNetworkObject.transform.SetParent(this.transform);

           // 새로 들어온 클라이언트가 기존 카메라 정보를 받을 수 있도록 ClientRpc 호출
           SyncCameraClientRpc(camTargetNetworkObject.NetworkObjectId);
       }

       [ClientRpc]
       private void SyncCameraClientRpc(ulong camTargetId)
       {
           if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(camTargetId, out NetworkObject camTargetNetObj))
           {
               camTarget = camTargetNetObj.transform;
               *//*camTarget.transform.SetParent(firstPersonCamera.transform);
               camTarget.transform.localPosition = Vector3.zero;
               camTarget.transform.localRotation = Quaternion.identity;*//*


               if (IsOwner)
               {
                   virtualCamera.Follow = camTarget.transform;
                   firstPersonCamera.enabled = true;
                   camTarget.gameObject.SetActive(true);
                   virtualCamera.gameObject.SetActive(true);
                   thirdPersonCamera.gameObject.SetActive(false);
               }
               else
               {
                   firstPersonCamera.enabled = false;
                   camTarget.gameObject.SetActive(false);
                   virtualCamera.gameObject.SetActive(false);
                   thirdPersonCamera.gameObject.SetActive(false);
               }
           }
       }
   */



    public virtual void Start()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        characterController = GetComponent<CharacterController>();


        if (IsOwner)
        {
            SpawnAndNotifyServerRpc(OwnerClientId);
            StartCoroutine(InitCamera());
        }
		else
		{
            InitRagdolls();
        }
    }

    //public virtual void LateUpdate()
    public virtual void FixedUpdate()
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

    private void HandleInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        Vector3 inputDirection = forward * vertical + right * horizontal;

        if (inputDirection.magnitude > 1)
            inputDirection.Normalize();

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && (currentStamina.Value > 0);
        float speedMultiplier = isRunning ? runSpeedMultiplier : 1.0f;
        moveDirection.x = inputDirection.x * walkSpeed * speedMultiplier;
        moveDirection.z = inputDirection.z * walkSpeed * speedMultiplier;


        if (isRunning)
        {
            currentStamina.Value -= staminaDecreaseRate * Time.deltaTime;
            currentStamina.Value = Mathf.Clamp(currentStamina.Value, 0, maxStamina);
            lastRunTime = Time.time; // 마지막으로 달린 시간 업데이트
            UpdateSteaminaBar();
        }
        else if (Time.time > lastRunTime + staminaRegenDelay)
        {
            currentStamina.Value += staminaRegenRate * Time.deltaTime;
            currentStamina.Value = Mathf.Clamp(currentStamina.Value, 0, maxStamina);
            UpdateSteaminaBar();
        }



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

            //firstPersonCamera.transform.localRotation = Quaternion.Euler(new Vector3(rotationX, 0, 0));
            camTarget.transform.localRotation = Quaternion.Euler(new Vector3(rotationX, 0, 0));
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, 90f);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        HandleAim();
        HeadBobbing();
        UpdateSteaminaBar();
    }

    private void EventPlayingStop()
	{
        moveDirection = Vector3.zero;
        SetPlayerAimPos(new Vector3(0, 2f, 10f));
    }

    private void UpdateSteaminaBar()
    {
        if (staminaBar == null)
        {
            staminaBar = GameObject.Find("StaminaBar").GetComponent<Image>();
        }
        float healthRatio = currentStamina.Value / maxStamina; // 0 ~ 1
        staminaBar.fillAmount = healthRatio * 0.5f; // 0 ~ 0.5로 변환
    }

    //걸을때 머리 덜렁이기 ㅋㅋ
    private void HeadBobbing()
	{
        if (characterController != null && IsGrounded() && characterController.velocity.magnitude > 0.1f)
        {
            // 걷는 중일 때만 흔들림
            timer += Time.deltaTime * bobbingSpeed;
            float bobbingOffset = Mathf.Sin(timer) * bobbingAmount; // 사인파로 흔들림 계산
            Vector3 newPosition = new Vector3(firstPersonCamera.transform.localPosition.x, midpoint + bobbingOffset, firstPersonCamera.transform.localPosition.z);
            firstPersonCamera.transform.localPosition = newPosition;
        }
        else
        {
            // 멈출 때는 Y축 위치를 초기화
            timer = 0f;
            Vector3 resetPosition = new Vector3(firstPersonCamera.transform.localPosition.x, midpoint, firstPersonCamera.transform.localPosition.z);
            firstPersonCamera.transform.localPosition = resetPosition;
        }
    }


    private void HandleAim()
	{
        //허리 각도 조절이랑 에임 조절용
        if (Physics.Raycast(firstPersonCamera.transform.position, firstPersonCamera.transform.forward, out hit, aimMaxDistance, layerMask))
        {
            Debug.DrawRay(firstPersonCamera.transform.position, firstPersonCamera.transform.forward * hit.distance, Color.red, 0.1f);

            // 충돌한 경우 hit.point로 설정
            aimTargetPosition = hit.point;
            // 충돌하지 않은 경우, maxDistance 지점으로 설정
            //handAimTarget.transform.position = playerCamera.transform.position + playerCamera.transform.forward * aimMaxDistance;
        }
        else
        {
            Debug.DrawRay(firstPersonCamera.transform.position, firstPersonCamera.transform.forward * hit.distance, Color.red, 0.1f);
            // 충돌하지 않은 경우, maxDistance 지점으로 설정
            aimTargetPosition = firstPersonCamera.transform.position + firstPersonCamera.transform.forward * aimMaxDistance;
        }

        handAimTarget.transform.position = Vector3.SmoothDamp(handAimTarget.transform.position, aimTargetPosition, ref velocity, smoothTime);

        Vector3 cameraForward = firstPersonCamera.transform.forward.normalized;
        Vector3 cameraPosition = firstPersonCamera.transform.position;
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
            Debug.Log("에임 변경");

        }
        else
        {
            // Do Nothing 설정 (LookAt 무시)
            virtualCamera.DestroyCinemachineComponent<CinemachineComposer>();
            virtualCamera.LookAt = null; // 비우기
            Debug.Log("에임 초기화");
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
		if (!getAimTarget())
		{
            return;
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

    private bool getAimTarget()
	{
		if (handAimTarget != null)
		{
            return true;
		}

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(handAimTargetulong.Value , out var aimTargetNetobject))
		{
            handAimTarget = aimTargetNetobject.transform;
            return true;
		}


        return false;
    }


    [ServerRpc(RequireOwnership = false)]
    public void SpawnAndNotifyServerRpc(ulong clientId)
    {
        NetworkObject networkObject = Instantiate(handAimTargetPrefab).GetComponent<NetworkObject>();

        // 부모 설정: 서버에서 변경
        networkObject.SpawnWithOwnership(clientId);
        networkObject.transform.SetParent(this.transform);


        // 오브젝트를 관리 리스트에 추가
        handAimTarget = networkObject.transform;
        handAimTargetulong.Value = networkObject.NetworkObjectId;

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
        if (constraint == null)
        {
            Debug.LogError("Constraint is null!");
            return;
        }

        if (source == null)
        {
            Debug.LogError("Source Object is null!");
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