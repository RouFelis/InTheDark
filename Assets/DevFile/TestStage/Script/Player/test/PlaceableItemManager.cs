using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlaceableItemManager : NetworkBehaviour
{
    public GameObject previewPrefab; // ��ġ �̸����� ������
    public GameObject objectPrefab; // ��ġ�� ������Ʈ�� ������
    public float maxPlacementDistance = 10f; // �ִ� ��ġ �Ÿ�
    public Material transparentMaterial; // ������ ��Ƽ����
    public Material validPlacementMaterial; // ��ġ ������ ��ġ ��Ƽ����
    public Material invalidPlacementMaterial; // ��ġ �Ұ����� ��ġ ��Ƽ����
    public float rotationSpeed = 100f; // ȸ�� �ӵ�
    public bool enableLogs = true; // �α� Ȱ��ȭ üũ�ڽ�

    private GameObject previewObject; // ��ġ �̸����� ������Ʈ
    public bool canPlace; // ��ġ ���� ����
    private float currentRotation = 0f; // ���� ȸ�� ����
    private bool isRotating = false; // ȸ�� ������ ����
    [SerializeField] private NetworkInventoryController netInvenController; 
    [SerializeField] private testMove playerController; // �÷��̾� ��Ʈ�ѷ� ����

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsClient && IsOwner)
        {
            StartCoroutine(InitializePlayerController());
        }
    }

    private IEnumerator InitializePlayerController()
    {
        while (playerController == null)
        {
            var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject(); // ���� �÷��̾� ������Ʈ ã��
            if (playerObject != null)
            {
                playerController = playerObject.GetComponent<testMove>(); // �÷��̾� ��Ʈ�ѷ� ã��
                netInvenController = playerObject.GetComponent<NetworkInventoryController>();
            }
            yield return null;
        }
    }

   
    void Start()
    {
        if (IsOwner) // ���� Ŭ���̾�Ʈ�� ������Ʈ�� ���������� Ȯ��
        {
            InitializePreviewObject(previewPrefab);
        }

        // �÷��̾� ��Ʈ�ѷ� ã�� ����
        StartCoroutine(FindPlayerController());
    }

    private IEnumerator FindPlayerController()
    {
        while (playerController == null)
        {
            
            try 
            {
                var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                if (playerObject != null)
                {
                    playerController = playerObject.GetComponent<testMove>();
                    if (playerController != null && enableLogs)
                    {
                        Debug.Log("PlayerController�� ã�ҽ��ϴ�.");
                    }
                }
			}
			catch
			{
                if (enableLogs)
                {
                    Debug.Log("PlayerController�� ã�� ��...");
                }
            }
            
            yield return new WaitForSeconds(1f); // 1�ʸ��� �ݺ�
        }
    }
    public void InitializePreviewObject(GameObject previewPrefab)
    {
        // �������� �����Ͽ� ��ġ �̸����� ������Ʈ ����
        previewObject = Instantiate(previewPrefab);
        SetObjectTransparent(previewObject);
    }

    public void UpdatePreviewObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            float distance = Vector3.Distance(Camera.main.transform.position, hit.point);
            if (distance <= maxPlacementDistance)
            {
                previewObject.transform.position = hit.point;
                canPlace = hit.collider.CompareTag("Ground");
                SetObjectMaterial(previewObject, canPlace ? validPlacementMaterial : invalidPlacementMaterial);
            }
            else
            {
                PlacePreviewBelow(ray);
            }
        }
        else
        {
            PlacePreviewBelow(ray);
        }

        if (isRotating)
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            currentRotation += mouseX;
            previewObject.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);
        }
    }

    private void PlacePreviewBelow(Ray ray)
    {
        Vector3 targetPosition = ray.origin + ray.direction * maxPlacementDistance;
        targetPosition.y = GetGroundHeight(targetPosition); // �ٴ� ���̸� ã��
        previewObject.transform.position = targetPosition;
        canPlace = Physics.Raycast(previewObject.transform.position, Vector3.down, out RaycastHit hit) && hit.collider.CompareTag("Ground");
        SetObjectMaterial(previewObject, canPlace ? validPlacementMaterial : invalidPlacementMaterial);
    }

    private float GetGroundHeight(Vector3 position)
    {
        if (Physics.Raycast(position, Vector3.down, out RaycastHit hit, Mathf.Infinity))
        {
            return hit.point.y;
        }
        return position.y;
    }

    [ServerRpc(RequireOwnership = false)] // �������� �ʿ����� �ʵ��� ����
    void PlaceObjectServerRpc(Vector3 position, Quaternion rotation, ServerRpcParams rpcParams = default)
    {
        // ��� Ŭ���̾�Ʈ���� ������Ʈ�� ��ġ�ϴ� ClientRpc ȣ��
        GameObject placedObject = Instantiate(objectPrefab, position, rotation);
        placedObject.GetComponent<NetworkObject>().Spawn();
        PlaceObjectClientRpc(position, rotation);
    }

    [ClientRpc] // Ŭ���̾�Ʈ���� ȣ��Ǵ� RPC �޼���
    void PlaceObjectClientRpc(Vector3 position, Quaternion rotation)
    {
        if (IsOwner)
        {
            PreviewDestroy();//�̸����� ����
            netInvenController.IsPlacingItem = false;
        }
    }

    public void PreviewDestroy()// �̸����� ������Ʈ ����
    {
        Destroy(previewObject); 
    }

    public void UseItem()
    {
        if (canPlace)
        {
            PlaceObjectServerRpc(previewObject.transform.position, previewObject.transform.rotation);          

            // �κ��丮���� ������ ���� ��û
            playerController.GetComponent<NetworkInventoryController>().UseCurrentSelectedItem();
        }
        Destroy(previewObject); // �̸����� ������Ʈ ����

        PreviewDestroy();//�̸����� ����
    }

    public void CancelPreview()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }
    }

    public void HandleRotation(ref bool isPlacingItem)
    {
        if (Input.GetMouseButtonDown(0)) // ���� ���콺 ��ư�� ������
        {
            isRotating = true;
            playerController.SetMouseControl(false); // �÷��̾� ȸ�� ����
        }
        if (Input.GetMouseButtonUp(0)) // ���� ���콺 ��ư�� ����
        {
            UseItem(); // ��ġ            
            isRotating = false;
            playerController.SetMouseControl(true); // �÷��̾� ȸ�� �簳
            isPlacingItem = false; // ��ġ ��� ��Ȱ��ȭ
        }

        if (isRotating)
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            currentRotation += mouseX;
            previewObject.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);
        }
    }

    private void SetObjectTransparent(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material = transparentMaterial;
        }
    }

    private void SetObjectMaterial(GameObject obj, Material material)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material = material;
        }
    }
}
