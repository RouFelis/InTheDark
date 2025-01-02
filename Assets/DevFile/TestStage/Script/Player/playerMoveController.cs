using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class playerMoveController : NetworkBehaviour
{
    [SerializeField] private GameObject VirtualCam;
    [SerializeField] private float walkSpeed = 5.0f; // 걷기 속도
    [SerializeField] private float lookSpeed = 2.0f; // 시야 회전 속도
    [SerializeField] private float lookXLimit = 90.0f; // 시야 회전 제한 각도

    [SerializeField] private Camera playerCamera; // 플레이어 카메라
    [SerializeField] private Transform headTarget; // 머리 타겟 (시야 회전용)
    [SerializeField] private CharacterController characterController; // 캐릭터 컨트롤러
    [SerializeField] private Animator animator;
    private Vector3 moveDirection = Vector3.zero; // 이동 방향
    private float rotationX = 0; // X축 회전 값

    [SerializeField] private float jumpForce = 8.0f; // 점프 힘
    [SerializeField] private float gravity = 20.0f; // 중력 가속도
    [SerializeField] private bool mouseControl = true; // 마우스 컨트롤 여부 


    private bool isJumping = false; // 점프 중인지 여부
    private Quaternion savedHeadRotation; // Save the head rotation during pause


    private NetworkVariable<Vector3> networkedPosition = new NetworkVariable<Vector3>(
     writePerm: NetworkVariableWritePermission.Owner
 );
    private NetworkVariable<Quaternion> networkedRotation = new NetworkVariable<Quaternion>(
        writePerm: NetworkVariableWritePermission.Owner
    );
    private NetworkVariable<Quaternion> networkedHeadRotation = new NetworkVariable<Quaternion>(
        writePerm: NetworkVariableWritePermission.Owner
    );

    private float lastSyncTime = 0f; // 마지막 동기화 시간
    private const float syncInterval = 0.05f; // 동기화 간격 (0.1초)

    public NetworkVariable<bool> isEventPlaying = new NetworkVariable<bool>(false, writePerm: NetworkVariableWritePermission.Owner);

    public virtual void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        characterController = GetComponent<CharacterController>();
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (IsOwner)
        {
            playerCamera.gameObject.SetActive(true); // 소유자일 때만 카메라 활성화
            VirtualCam.gameObject.SetActive(true); // 소유자일 때만 카메라 활성화
            FixedMouse(); // 마우스 고정
            MenuManager.Instance.OnPause += FreeMouse;
            MenuManager.Instance.OnResume += FixedMouse;
        }
        else
        {
            playerCamera.gameObject.SetActive(false); // 소유자가 아닐 때는 카메라 비활성화
            VirtualCam.gameObject.SetActive(false); // 소유자가 아닐 때는 카메라 비활성화
        }
    }
   
    void FixedMouse()
    {
        if (!isEventPlaying.Value)
        {
            Cursor.lockState = CursorLockMode.Locked; // 커서를 중앙에 고정
        }
    }

    void FreeMouse()
    {
        Cursor.lockState = CursorLockMode.None; // 커서 고정 해제
    }

    public void EventToggle(bool boolValue)
    {
        isEventPlaying.Value = boolValue;
        enabled = !boolValue;
        if (boolValue)
            Cursor.lockState = CursorLockMode.None; // 마우스 해제
        else
            Cursor.lockState = CursorLockMode.Locked; // 마우스 잠금
    }

    void OnEnable()
    {
        networkedPosition.OnValueChanged += OnNetworkPositionChanged;
        networkedRotation.OnValueChanged += OnNetworkRotationChanged;
        networkedHeadRotation.OnValueChanged += OnNetworkHeadRotationChanged;
    }

    void OnDisable()
    {
        networkedPosition.OnValueChanged -= OnNetworkPositionChanged;
        networkedRotation.OnValueChanged -= OnNetworkRotationChanged;
        networkedHeadRotation.OnValueChanged -= OnNetworkHeadRotationChanged;
    }

    void LateUpdate()
    {
        if (IsOwner)
        {
            HandleInput(); // 입력 처리
        }
        else
        {
            SyncTransform(); // 변환 값 동기화
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Lobby")
        {
            Destroy(gameObject); // 로비 씬으로 이동 시 오브젝트 파괴
        }
    }

    public override void OnDestroy()
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
        PlayerControlle();

        if (IsOwner && Time.time - lastSyncTime >= syncInterval) // 동기화 간격을 만족할 때만 실행
        {
            lastSyncTime = Time.time; // 마지막 동기화 시간 갱신

            // 네트워크 변수 값 업데이트
            if (networkedPosition.Value != transform.position)
                networkedPosition.Value = transform.position;

            if (networkedRotation.Value != transform.rotation)
                networkedRotation.Value = transform.rotation;

            if (networkedHeadRotation.Value != headTarget.localRotation)
                networkedHeadRotation.Value = headTarget.localRotation;
        }
    }

    private void SyncTransform()
    {
        transform.position = networkedPosition.Value;
        transform.rotation = networkedRotation.Value;
        headTarget.localRotation = networkedHeadRotation.Value;
    }

    private void OnNetworkPositionChanged(Vector3 oldValue, Vector3 newValue)
    {
        if (!IsOwner)
        {
            transform.position = newValue;
        }
    }

    private void OnNetworkRotationChanged(Quaternion oldValue, Quaternion newValue)
    {
        if (!IsOwner)
        {
            transform.rotation = newValue;
        }
    }

    private void OnNetworkHeadRotationChanged(Quaternion oldValue, Quaternion newValue)
    {
        if (!IsOwner)
        {
            headTarget.localRotation = newValue;
        }
    }

    void PlayerControlle()
    {
        if (MenuManager.Instance.IsPaused)
        {
            headTarget.localRotation = savedHeadRotation;
            animator.SetBool("IsWalking", false);

            if (!characterController.isGrounded)
            {
                moveDirection.y -= gravity * Time.deltaTime;
                characterController.Move(moveDirection * Time.deltaTime);
            }
            return;
        }

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        float curSpeedX = Input.GetAxis("Vertical") * walkSpeed;
        float curSpeedY = Input.GetAxis("Horizontal") * walkSpeed;

        if (Input.GetKey(KeySettingsManager.Instance.SprintKey))
        {
            curSpeedX *= 1.4f;
            curSpeedY *= 1.4f;
            animator.speed = 1.4f;
        }
        else
        {
            animator.speed = 1.0f;
        }

        moveDirection.x = (forward * curSpeedX + right * curSpeedY).x;
        moveDirection.z = (forward * curSpeedX + right * curSpeedY).z;

        bool isWalking = moveDirection.x != 0 || moveDirection.z != 0;
        animator.SetBool("IsWalking", isWalking);

        if (characterController.isGrounded)
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
            }
        }
        else
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        if (mouseControl)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + Input.GetAxis("Mouse X") * lookSpeed, 0);
            headTarget.localRotation = Quaternion.Euler(new Vector3(rotationX, 0, 0));
            savedHeadRotation = headTarget.localRotation;
        }
    }

    public void SetMouseControl(bool enable)
    {
        mouseControl = enable;
    }
}
