using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlaceableItemManager : NetworkBehaviour
{
    // == ������ �� ������ ������Ʈ ==
    public GameObject previewPrefab { get; set; } // ��ġ �̸����� ������
    public GameObject objectPrefab { get; set; } // ��ġ�� ������Ʈ�� ������
    public GameObject spawnedItem; // ������ ������Ʈ
    public ulong spawnedItemID; // ������ ������Ʈ�� ID

    // == ��ġ ���� ���� ==
    public float maxPlacementDistance = 10f; // �ִ� ��ġ �Ÿ�
    public bool canPlace; // ��ġ ���� ����

    // == ��Ƽ���� ==
    public Material transparentMaterial; // ������ ��Ƽ����
    public Material validPlacementMaterial; // ��ġ ������ ��ġ ��Ƽ����
    public Material invalidPlacementMaterial; // ��ġ �Ұ����� ��ġ ��Ƽ����

    // == ȸ�� ���� ==
    public float rotationSpeed = 100f; // ȸ�� �ӵ�
    private float currentRotation = 0f; // ���� ȸ�� ����
    private bool isRotating = false; // ȸ�� ������ ����

    // == ��Ʈ��ũ ���� ==
    public bool clientRpcCompleted = false;
    public bool networkLoading = false;
    private NetworkObject spawnedObjectParent;
    private NetworkInventoryController netInvenController;

    // == ��Ÿ ==
    public bool enableLogs = true; // �α� Ȱ��ȭ üũ�ڽ�
    [HideInInspector] public GameObject previewObject; // ��ġ �̸����� ������Ʈ
	private playerMoveController playerController; // �÷��̾� ��Ʈ�ѷ� ����


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
                    playerController = playerObject.GetComponent<playerMoveController>();
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
        while (netInvenController == null)
        {
            try
            {
                var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject(); // ���� �÷��̾� ������Ʈ ã��
                if (playerController != null)
                {
                    netInvenController = playerObject.GetComponent<NetworkInventoryController>();
                    if (enableLogs)
                        Debug.Log("netInvenController ã�ҽ��ϴ�.");
                }
            }
            catch
            {
                if (enableLogs)
                {
                    Debug.Log("netInvenController�� ã�� ��...");
                }
            }
            yield return new WaitForSeconds(1f);
        }

        while (spawnedObjectParent == null)
        {
            try
            {
                spawnedObjectParent = GameObject.Find("SpawnedObjects").GetComponent<NetworkObject>();
                if (playerController != null && enableLogs)
                {
                    Debug.Log("spawnedObjectParent �� ã�ҽ��ϴ�.");
                }
            }
            catch
            {
                if (enableLogs)
                {
                    Debug.Log("spawnedObjectParent �� ã�� ��...");
                }
            }

            yield return new WaitForSeconds(1f); // 1�ʸ��� �ݺ�
        }

      
    }
    public void InitializePreviewObject(GameObject previewPrefab)
    {
        Quaternion spawnRotation = Quaternion.Euler(Vector3.zero);
        // �������� �����Ͽ� ��ġ �̸����� ������Ʈ ����
        previewObject = Instantiate(previewPrefab,this.gameObject.transform.position ,spawnRotation);
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

    private float GetGroundHeight(Vector3 targetPosition)
    {
        if (Physics.Raycast(new Vector3(targetPosition.x, Camera.main.transform.position.y, targetPosition.z), Vector3.down, out RaycastHit hit))
        {
            return hit.point.y;
        }
        return targetPosition.y;
    }


    [ServerRpc(RequireOwnership = false)] // �������� �ʿ����� �ʵ��� ����
    void PlaceObjectServerRpc(Vector3 position, Quaternion rotation, NetworkString path ,ServerRpcParams rpcParams = default)
    {
        GameObject loadObject = Resources.Load<GameObject>(path);
        // ��� Ŭ���̾�Ʈ���� ������Ʈ�� ��ġ�ϴ� ClientRpc ȣ��
        GameObject placedObject = Instantiate(loadObject, position, rotation);
        NetworkObject networkObject = placedObject.GetComponent<NetworkObject>();
        networkObject.Spawn();
        NetworkObject parentObject = NetworkManager.SpawnManager.SpawnedObjects[spawnedObjectParent.NetworkObjectId];
        networkObject.transform.SetParent(parentObject.transform, true);        
        PlaceObjectClientRpc(networkObject.NetworkObjectId, position, rotation , rpcParams.Receive.SenderClientId);
    }

    [ClientRpc] // Ŭ���̾�Ʈ���� ȣ��Ǵ� RPC �޼���
    void PlaceObjectClientRpc(ulong objectId, Vector3 position, Quaternion rotation, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {;
            NetworkObject networkObject = NetworkManager.SpawnManager.SpawnedObjects[objectId];
            spawnedItem = networkObject.gameObject;
            spawnedItemID = objectId;
            netInvenController.IsPlacingItem = false;
            // ClientRpc �Ϸ� ǥ��
            clientRpcCompleted = true;
        }
    }

    public void PreviewDestroy()// �̸����� ������Ʈ ����
    {
		if (previewObject!=null)
        {
            Destroy(previewObject);
        }        
    }

    IEnumerator UseItem(string itemPath)
    {
        if (canPlace)
        {
            PlaceObjectServerRpc(previewObject.transform.position, previewObject.transform.rotation , itemPath);
            networkLoading = true;
            yield return new WaitUntil(() => clientRpcCompleted);
            clientRpcCompleted = false;
            networkLoading = false;
            PreviewDestroy();//�̸����� ����
            

            // �κ��丮���� ������ ���� ��û
            playerController.GetComponent<NetworkInventoryController>().UseCurrentSelectedItem(spawnedItemID);
		}
		else
		{
            /*yield return new WaitUntil(() => clientRpcCompleted);
            
            previewObject.SetActive(true);*/
        }

    }

	public void HandleRotation(ref bool isPlacingItem, string itemPath)
    {
        if (Input.GetMouseButtonDown(0)) // ���� ���콺 ��ư�� ������
        {
            isRotating = true;
            playerController.SetMouseControl(false); // �÷��̾� ȸ�� ����
        }
        if (Input.GetMouseButtonUp(0)) // ���� ���콺 ��ư�� ����
        {
            networkLoading = true;            
            StartCoroutine(UseItem(itemPath)); 
            isRotating = false;
            isPlacingItem = false; // ��ġ ��� ��Ȱ��ȭ
            playerController.SetMouseControl(true); // �÷��̾� ȸ�� �簳            
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
