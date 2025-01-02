using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class playerMoveController : NetworkBehaviour
{
    [SerializeField] private GameObject VirtualCam;
    [SerializeField] private float walkSpeed = 5.0f; // �ȱ� �ӵ�
    [SerializeField] private float lookSpeed = 2.0f; // �þ� ȸ�� �ӵ�
    [SerializeField] private float lookXLimit = 90.0f; // �þ� ȸ�� ���� ����

    [SerializeField] private Camera playerCamera; // �÷��̾� ī�޶�
    [SerializeField] private Transform headTarget; // �Ӹ� Ÿ�� (�þ� ȸ����)
    [SerializeField] private CharacterController characterController; // ĳ���� ��Ʈ�ѷ�
    [SerializeField] private Animator animator;
    private Vector3 moveDirection = Vector3.zero; // �̵� ����
    private float rotationX = 0; // X�� ȸ�� ��

    [SerializeField] private float jumpForce = 8.0f; // ���� ��
    [SerializeField] private float gravity = 20.0f; // �߷� ���ӵ�
    [SerializeField] private bool mouseControl = true; // ���콺 ��Ʈ�� ���� 


    private bool isJumping = false; // ���� ������ ����
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

    private float lastSyncTime = 0f; // ������ ����ȭ �ð�
    private const float syncInterval = 0.05f; // ����ȭ ���� (0.1��)

    public NetworkVariable<bool> isEventPlaying = new NetworkVariable<bool>(false, writePerm: NetworkVariableWritePermission.Owner);

    public virtual void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        characterController = GetComponent<CharacterController>();
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (IsOwner)
        {
            playerCamera.gameObject.SetActive(true); // �������� ���� ī�޶� Ȱ��ȭ
            VirtualCam.gameObject.SetActive(true); // �������� ���� ī�޶� Ȱ��ȭ
            FixedMouse(); // ���콺 ����
            MenuManager.Instance.OnPause += FreeMouse;
            MenuManager.Instance.OnResume += FixedMouse;
        }
        else
        {
            playerCamera.gameObject.SetActive(false); // �����ڰ� �ƴ� ���� ī�޶� ��Ȱ��ȭ
            VirtualCam.gameObject.SetActive(false); // �����ڰ� �ƴ� ���� ī�޶� ��Ȱ��ȭ
        }
    }
   
    void FixedMouse()
    {
        if (!isEventPlaying.Value)
        {
            Cursor.lockState = CursorLockMode.Locked; // Ŀ���� �߾ӿ� ����
        }
    }

    void FreeMouse()
    {
        Cursor.lockState = CursorLockMode.None; // Ŀ�� ���� ����
    }

    public void EventToggle(bool boolValue)
    {
        isEventPlaying.Value = boolValue;
        enabled = !boolValue;
        if (boolValue)
            Cursor.lockState = CursorLockMode.None; // ���콺 ����
        else
            Cursor.lockState = CursorLockMode.Locked; // ���콺 ���
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
            HandleInput(); // �Է� ó��
        }
        else
        {
            SyncTransform(); // ��ȯ �� ����ȭ
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Lobby")
        {
            Destroy(gameObject); // �κ� ������ �̵� �� ������Ʈ �ı�
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

        if (IsOwner && Time.time - lastSyncTime >= syncInterval) // ����ȭ ������ ������ ���� ����
        {
            lastSyncTime = Time.time; // ������ ����ȭ �ð� ����

            // ��Ʈ��ũ ���� �� ������Ʈ
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
