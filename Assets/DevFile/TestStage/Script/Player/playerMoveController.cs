using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Animations.Rigging;

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
    [SerializeField] private Transform camTarget;
    [SerializeField] private float lookSpeed = 2.0f;
    [SerializeField] private float lookXLimit = 60.0f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

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

    [Header("Head Bobbing Settings")]
    public float bobbingSpeed = 14f; // �Ӹ� ��鸲 �ӵ�
    public float bobbingAmount = 0.05f; // �Ӹ� ��鸲 ����
    public float walkBobbingAmount = 0.05f; // �Ӹ� ��鸲 ����
    public float RunningBobbingAmount = 0.05f; // �Ӹ� ��鸲 ����
    public float midpoint = 0f; // �⺻ ī�޶� ���� (�÷��̾� �Ӹ� ��ġ)

    private float timer = 0f; // �ð� ���� ����


    //private LayerMask layerMask;
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0.0f;
    private RaycastHit hit;


    private bool mouseControl = true;
    private bool isJumping = false;
    private Quaternion savedHeadRotation;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        //layerMask = ~LayerMask.GetMask("CharacterFirstPerson", "CharacterThridPerson");
        /*        layerMask = ((1 << LayerMask.NameToLayer("CharacterFirstPerson")) | (1 << LayerMask.NameToLayer("CharacterThridPerson")));
                layerMask = ~layerMask;*/
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

    public virtual void Start()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (IsOwner)
        {
            SpawnAndNotifyServerRpc(OwnerClientId);
        }
    }

    private void LateUpdate()
    {
        if (IsOwner)
        {
            if (!isEventPlaying.Value)
            {
                HandleInput();
                // HandleInputServerRpc(InputMoveNormal() , InputMouseNormal());
            }
        }
		else
		{
            FindAimTargetObject();
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
        // �̵� �Է�
        float horizontal = Input.GetAxis("Horizontal");
        Debug.Log("horizontal : " + horizontal);
        float vertical = Input.GetAxis("Vertical");
        Debug.Log("vertical : " + vertical);
        return new Vector2(horizontal, vertical);
    }
    private Vector2 InputMouseNormal()
    {
        // �̵� �Է�
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

        // ���� ó��
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
                animator.speed = 1.0f; // ���� �߿��� �⺻ �ӵ�
            }
        }
        else
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        if (mouseControl)
        {
            // ���콺 ȸ�� �Է�
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;

            camTarget.transform.localRotation = Quaternion.Euler(new Vector3(rotationX, 0, 0));
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, 90f);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        HandleAim();
        HeadBobbing();
    }

    private void HeadBobbing()
	{
        if (characterController != null && characterController.isGrounded && characterController.velocity.magnitude > 0.1f)
        {
            // �ȴ� ���� ���� ��鸲
            timer += Time.deltaTime * bobbingSpeed;
            float bobbingOffset = Mathf.Sin(timer) * bobbingAmount; // �����ķ� ��鸲 ���
            Vector3 newPosition = new Vector3(playerCamera.transform.localPosition.x, midpoint + bobbingOffset, playerCamera.transform.localPosition.z);
            playerCamera.transform.localPosition = newPosition;
        }
        else
        {
            // ���� ���� Y�� ��ġ�� �ʱ�ȭ
            timer = 0f;
            Vector3 resetPosition = new Vector3(playerCamera.transform.localPosition.x, midpoint, playerCamera.transform.localPosition.z);
            playerCamera.transform.localPosition = resetPosition;
        }
    }

    private void HandleAim()
	{
        /*    //�㸮 ���� �����̶� ���� ������
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, aimMaxDistance , layerMask))
            {
                // �浹�� ��� hit.point�� ����
               // handAimTarget.transform.position = hit.point;
                // �浹���� ���� ���, maxDistance �������� ����
                handAimTarget.transform.position = playerCamera.transform.position + playerCamera.transform.forward * aimMaxDistance;
            }
            else
            {
                // �浹���� ���� ���, maxDistance �������� ����
                handAimTarget.transform.position = playerCamera.transform.position + playerCamera.transform.forward * aimMaxDistance;
            }*/

        handAimTarget.transform.position = playerCamera.transform.position + playerCamera.transform.forward * aimMaxDistance;

        Vector3 cameraForward = playerCamera.transform.forward.normalized;
        Vector3 cameraPosition = playerCamera.transform.position;
    }

    private void UpdateAnimator()
    {
        isWalking.Value = moveDirection.x != 0 || moveDirection.z != 0;
        isRunning.Value = isWalking.Value && Input.GetKey(KeyCode.LeftShift);

        animator.SetBool("IsWalking", isWalking.Value);
      //  animator.SetBool("IsRunning", isRunning);
      //  animator.SetBool("IsJumping", isJumping);

        // �ִϸ��̼� �ӵ� �� ���� ����ȭ
        if (isWalking.Value)
        {
			if (isRunning.Value)
			{
                animator.speed = 1.4f;
                bobbingSpeed = 18f; // �ȱ� �ӵ�
                bobbingAmount = RunningBobbingAmount; // �ȱ� ��鸲 ����
            }
			else
			{
                animator.speed = 1f;
                bobbingSpeed = 14f; // �޸��� �ӵ�
                bobbingAmount = walkBobbingAmount; // �޸��� ��鸲 ����
            }
        }
        else
        {
            animator.speed = 1.0f; // �⺻ �ӵ�
        }
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

    public void SetMouseControl(bool enable)
    {
        mouseControl = enable;
    }

    private bool IsGrounded()
    {
        return Physics.OverlapSphere(groundCheckPosition.position, groundCheckRadius, groundLayer).Length > 0;
    }



    #region SpawnTargetPointer

    public void FindAimTargetObject()
    {
        handAimTarget = this.transform.Find("HandTarget(Clone)");

        // ��� MultiAimConstraint�� ���� �ҽ� ������Ʈ �߰�
        foreach (var config in playerConstraints)
        {
            AddSourceObject(config.constraint, handAimTarget, config.weight);
        }

        //���� �ʱ�ȭ.
        if (rigBuilder_firstPerson != null)
        {
            rigBuilder_firstPerson.Build();
        }
        //���� �ʱ�ȭ.
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

        // �θ� ����: �������� ����
        spawnedObject.transform.SetParent(this.transform);

        networkObject.SpawnWithOwnership(clientId);

        // ������Ʈ�� ���� ����Ʈ�� �߰�
        handAimTarget = spawnedObject.transform;


        // Ŭ���̾�Ʈ�鿡�� ���� ���� ����
        NotifyClientsOfSpawnClientRpc(networkObject.NetworkObjectId, clientId);


        Debug.Log($"Spawned object for client {clientId}");
    }

    [ClientRpc]
    private void NotifyClientsOfSpawnClientRpc(ulong networkObjectId, ulong ownerId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var networkObject))
        {
            handAimTarget = networkObject.transform;
            // �θ� ����: �������� ����
            networkObject.transform.SetParent(this.transform);

            Debug.Log($"Client notified: Object {networkObject.gameObject.name} owned by {ownerId}");
        }

        // ��� MultiAimConstraint�� ���� �ҽ� ������Ʈ �߰�
        foreach (var config in playerConstraints)
        {
            AddSourceObject(config.constraint, handAimTarget, config.weight);
        }

        //���� �ʱ�ȭ.
        if (rigBuilder_firstPerson != null)
        {
            rigBuilder_firstPerson.Build();
        }
        //���� �ʱ�ȭ.
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

        // ���� ������ �ҽ� ������Ʈ ����Ʈ ��������
        var sourceObjects = constraint.data.sourceObjects;

        // �ҽ� ������Ʈ �߰�
        sourceObjects.Add(new WeightedTransform(source, weight));

        // ������Ʈ�� �ҽ� ������Ʈ ����Ʈ ����
        constraint.data.sourceObjects = sourceObjects;

        Debug.Log($"Added source object '{source.name}' with weight {weight} to constraint on '{constraint.name}'");
    }

    #endregion
}

[System.Serializable]
public class ConstraintConfig
{
    public MultiAimConstraint constraint; // MultiAimConstraint ������Ʈ
    public float weight;                 // ������ ����ġ
}