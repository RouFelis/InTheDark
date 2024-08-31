using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class testMove : NetworkBehaviour, ICharacter , IDamaged
{
    [SerializeField] private float walkSpeed = 5.0f; // �ȱ� �ӵ�
    [SerializeField] private float lookSpeed = 2.0f; // �þ� ȸ�� �ӵ�
    [SerializeField] private float lookXLimit = 90.0f; // �þ� ȸ�� ���� ����

    [SerializeField] private Camera playerCamera; // �÷��̾� ī�޶�
    [SerializeField] private Transform headTarget; // �Ӹ� Ÿ�� (�þ� ȸ����)
    [SerializeField] private CharacterController characterController; // ĳ���� ��Ʈ�ѷ�
    private Vector3 moveDirection = Vector3.zero; // �̵� ����
    private float rotationX = 0; // X�� ȸ�� ��

    [SerializeField] private float jumpForce = 8.0f; // ���� ��
    [SerializeField] private float gravity = 20.0f; // �߷� ���ӵ�
    [SerializeField] private bool mouseControl = true; // ���콺 ��Ʈ�� ����

    private bool isJumping = false; // ���� ������ ����

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
            playerCamera.gameObject.SetActive(true); // �������� ���� ī�޶� Ȱ��ȭ
            Cursor.lockState = CursorLockMode.Locked; // Ŀ���� �߾ӿ� ����
        }
        else
        {
            playerCamera.gameObject.SetActive(false); // �����ڰ� �ƴ� ���� ī�޶� ��Ȱ��ȭ
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
            HandleInput(); // �Է� ó��
            UpdateNetworkedTransform(); // ��Ʈ��ũ ��ȯ �� ������Ʈ
        }
        else
        {
            SyncTransform(); // ��ȯ �� ����ȭ
        }
    }

    private void HandleInput()
    {
        PlayerControlle(); // �÷��̾� ��Ʈ�� ó��

        // ��Ʈ��ũ ��ġ�� ȸ�� �� ������Ʈ
        networkedPosition.Value = transform.position;
        networkedRotation.Value = transform.rotation;
        networkedHeadRotation.Value = headTarget.localRotation;
    }

    private void UpdateNetworkedTransform()
    {
        if (IsOwner)
        {
            // ��Ʈ��ũ ���� �� ������Ʈ
            networkedPosition.Value = transform.position;
            networkedRotation.Value = transform.rotation;
            networkedHeadRotation.Value = headTarget.localRotation;
        }
    }

    private void SyncTransform()
    {
        // ��Ʈ��ũ ���� ���� ����Ͽ� ��ġ�� ȸ�� ����ȭ
        transform.position = networkedPosition.Value;
        transform.rotation = networkedRotation.Value;
        headTarget.localRotation = networkedHeadRotation.Value;
    }

    // ��Ʈ��ũ ��ġ �� ���� �̺�Ʈ �ڵ鷯
    private void OnNetworkPositionChanged(Vector3 oldValue, Vector3 newValue)
    {
        if (!IsOwner)
        {
            transform.position = newValue;
        }
    }

    // ��Ʈ��ũ ȸ�� �� ���� �̺�Ʈ �ڵ鷯
    private void OnNetworkRotationChanged(Quaternion oldValue, Quaternion newValue)
    {
        if (!IsOwner)
        {
            transform.rotation = newValue;
        }
    }

    // ��Ʈ��ũ �Ӹ� ȸ�� �� ���� �̺�Ʈ �ڵ鷯
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
                moveDirection.y = 0; // �ٴڿ� ������ Y �ӵ� �ʱ�ȭ
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
            moveDirection.y -= gravity * Time.deltaTime; // �߷� ����
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
