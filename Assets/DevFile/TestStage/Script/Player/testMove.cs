using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class testMove : NetworkBehaviour, ICharacter , IDamaged
{
    [SerializeField] private float walkSpeed = 5.0f; // 걷기 속도
    [SerializeField] private float lookSpeed = 2.0f; // 시야 회전 속도
    [SerializeField] private float lookXLimit = 90.0f; // 시야 회전 제한 각도

    [SerializeField] private Camera playerCamera; // 플레이어 카메라
    [SerializeField] private Transform headTarget; // 머리 타겟 (시야 회전용)
    [SerializeField] private CharacterController characterController; // 캐릭터 컨트롤러
    private Vector3 moveDirection = Vector3.zero; // 이동 방향
    private float rotationX = 0; // X축 회전 값

    [SerializeField] private float jumpForce = 8.0f; // 점프 힘
    [SerializeField] private float gravity = 20.0f; // 중력 가속도
    [SerializeField] private bool mouseControl = true; // 마우스 컨트롤 여부

    private bool isJumping = false; // 점프 중인지 여부

    public string Name { get; set; }
    public int Health { get; set; }
    public int Damage { get; set; }


    private NetworkVariable<Vector3> networkedPosition = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<Quaternion> networkedRotation = new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<Quaternion> networkedHeadRotation = new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Owner);

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (IsOwner)
        {
            playerCamera.gameObject.SetActive(true); // 소유자일 때만 카메라 활성화
            Cursor.lockState = CursorLockMode.Locked; // 커서를 중앙에 고정
        }
        else
        {
            playerCamera.gameObject.SetActive(false); // 소유자가 아닐 때는 카메라 비활성화
        }
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
            UpdateNetworkedTransform(); // 네트워크 변환 값 업데이트
        }
        else
        {
            SyncTransform(); // 변환 값 동기화
        }
    }

    private void HandleInput()
    {
        PlayerControlle(); // 플레이어 컨트롤 처리

        // 네트워크 위치와 회전 값 업데이트
        networkedPosition.Value = transform.position;
        networkedRotation.Value = transform.rotation;
        networkedHeadRotation.Value = headTarget.localRotation;
    }

    private void UpdateNetworkedTransform()
    {
        if (IsOwner)
        {
            // 네트워크 변수 값 업데이트
            networkedPosition.Value = transform.position;
            networkedRotation.Value = transform.rotation;
            networkedHeadRotation.Value = headTarget.localRotation;
        }
    }

    private void SyncTransform()
    {
        // 네트워크 변수 값을 사용하여 위치와 회전 동기화
        transform.position = networkedPosition.Value;
        transform.rotation = networkedRotation.Value;
        headTarget.localRotation = networkedHeadRotation.Value;
    }

    // 네트워크 위치 값 변경 이벤트 핸들러
    private void OnNetworkPositionChanged(Vector3 oldValue, Vector3 newValue)
    {
        if (!IsOwner)
        {
            transform.position = newValue;
        }
    }

    // 네트워크 회전 값 변경 이벤트 핸들러
    private void OnNetworkRotationChanged(Quaternion oldValue, Quaternion newValue)
    {
        if (!IsOwner)
        {
            transform.rotation = newValue;
        }
    }

    // 네트워크 머리 회전 값 변경 이벤트 핸들러
    private void OnNetworkHeadRotationChanged(Quaternion oldValue, Quaternion newValue)
    {
        if (!IsOwner)
        {
            headTarget.localRotation = newValue;
        }
    }


    void PlayerControlle()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        float curSpeedX = Input.GetAxis("Vertical") * walkSpeed;
        float curSpeedY = Input.GetAxis("Horizontal") * walkSpeed;
        moveDirection.x = (forward * curSpeedX + right * curSpeedY).x;
        moveDirection.z = (forward * curSpeedX + right * curSpeedY).z;

        if (characterController.isGrounded)
        {
            if (isJumping)
            {
                moveDirection.y = 0; // 바닥에 닿으면 Y 속도 초기화
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
            moveDirection.y -= gravity * Time.deltaTime; // 중력 적용
        }

        characterController.Move(moveDirection * Time.deltaTime);

        if (mouseControl)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localEulerAngles = new Vector3(rotationX, 0, 0);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + Input.GetAxis("Mouse X") * lookSpeed, 0);

            headTarget.localEulerAngles = new Vector3(-rotationX, 0, 0);
        }
    }


    public void SetMouseControl(bool enable)
    {
        mouseControl = enable;
    }

    public void TakeDamage(int amount)
    {
        throw new System.NotImplementedException();
    }

    public void Attack(ICharacter target)
    {
        throw new System.NotImplementedException();
    }
}
