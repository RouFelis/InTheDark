using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class playerMoveController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float runSpeedMultiplier = 1.5f;
    [SerializeField] private float rotationSpeed = 2.0f;
    [SerializeField] private float gravity = 20.0f;
    [SerializeField] private float jumpForce = 8.0f;

    [Header("Camera & Head Rotation")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject virtualCamera;
    [SerializeField] private Transform headTarget;
    [SerializeField] private float lookSpeed = 2.0f;
    [SerializeField] private float lookXLimit = 90.0f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Networking")]
    [SerializeField] private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
    [SerializeField] private NetworkVariable<Vector3> networkRotation = new NetworkVariable<Vector3>();
    [SerializeField] private NetworkVariable<Quaternion> networkHeadRotation = new NetworkVariable<Quaternion>();
    [SerializeField] private NetworkVariable<bool> isEventPlaying = new NetworkVariable<bool>(false);

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0.0f;

    private bool mouseControl = true;
    private bool isJumping = false;
    private Quaternion savedHeadRotation;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerCamera.gameObject.SetActive(true);
            virtualCamera.SetActive(true);
            FixedMouse();
            MenuManager.Instance.OnPause += FreeMouse;
            MenuManager.Instance.OnResume += FixedMouse;
        }
        else
        {
            playerCamera.gameObject.SetActive(false);
            virtualCamera.SetActive(false);
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void LateUpdate()
    {
        if (IsOwner)
        {
            if (!isEventPlaying.Value)
            {
                HandleInput();
            }
        }
        else
        {
            SyncState();
        }

        UpdateMovement();
        UpdateAnimator();
        UpdateHeadRotation();
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
        // 이동 입력
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

        // 점프 처리
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
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + Input.GetAxis("Mouse X") * lookSpeed, 0);
            headTarget.localRotation = Quaternion.Euler(new Vector3(rotationX, 0, 0));
            savedHeadRotation = headTarget.localRotation;
        }


        if (IsOwner)
        {
            UpdateServerPositionRotationServerRpc(inputDirection * walkSpeed, Vector3.up * horizontal * rotationSpeed);
            UpdateHeadRotationServerRpc(headTarget.localRotation);
        }
    }

    private void UpdateMovement()
    {
        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void UpdateAnimator()
    {
        bool isWalking = moveDirection.x != 0 || moveDirection.z != 0;
        bool isRunning = isWalking && Input.GetKey(KeyCode.LeftShift);

        animator.SetBool("IsWalking", isWalking);
        animator.SetBool("IsRunning", isRunning);
        animator.SetBool("IsJumping", isJumping);

        // 애니메이션 속도 및 사운드 동기화
        if (isWalking)
        {
            animator.speed = isRunning ? 1.4f : 1.0f;
        }
        else
        {
            animator.speed = 1.0f; // 기본 속도
        }
    }

    private void UpdateHeadRotation()
    {
        if (!IsOwner)
        {
            headTarget.localRotation = networkHeadRotation.Value;
        }
    }

    private void SyncState()
    {
        if (!IsOwner)
        {
            // 네트워크에서 받은 방향 벡터로 이동
            Vector3 direction = networkPosition.Value - transform.position;

            // CharacterController로 부드럽게 이동
            characterController.SimpleMove(direction.normalized * walkSpeed);
        }
        transform.rotation = Quaternion.Euler(networkRotation.Value);
    }

    public void EventToggle(bool value)
    {
        isEventPlaying.Value = value;
        enabled = !value;

        if (value)
        {
            FreeMouse();
        }
        else
        {
            FixedMouse();
        }
    }

    private void FixedMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FreeMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateServerPositionRotationServerRpc(Vector3 newPosition, Vector3 newRotation)
    {
        networkPosition.Value = transform.position + newPosition * Time.deltaTime;
        networkRotation.Value = transform.eulerAngles + newRotation * Time.deltaTime;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateHeadRotationServerRpc(Quaternion newHeadRotation)
    {
        networkHeadRotation.Value = newHeadRotation;
    }

    public void SetMouseControl(bool enable)
    {
        mouseControl = enable;
    }
}