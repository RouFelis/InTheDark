using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlaceableItemManager : NetworkBehaviour
{
    // == 프리팹 및 생성된 오브젝트 ==
    public GameObject previewPrefab { get; set; } // 설치 미리보기 프리팹
    public GameObject objectPrefab { get; set; } // 설치할 오브젝트의 프리팹
    public GameObject spawnedItem; // 생성된 오브젝트
    public ulong spawnedItemID; // 생성된 오브젝트의 ID

    // == 설치 관련 변수 ==
    public float maxPlacementDistance = 10f; // 최대 설치 거리
    public bool canPlace; // 설치 가능 여부

    // == 머티리얼 ==
    public Material transparentMaterial; // 투명한 머티리얼
    public Material validPlacementMaterial; // 설치 가능한 위치 머티리얼
    public Material invalidPlacementMaterial; // 설치 불가능한 위치 머티리얼

    // == 회전 관련 ==
    public float rotationSpeed = 100f; // 회전 속도
    private float currentRotation = 0f; // 현재 회전 각도
    private bool isRotating = false; // 회전 중인지 여부

    // == 네트워크 관련 ==
    public bool clientRpcCompleted = false;
    public bool networkLoading = false;
    private NetworkObject spawnedObjectParent;
    private NetworkInventoryController netInvenController;

    // == 기타 ==
    public bool enableLogs = true; // 로그 활성화 체크박스
    [HideInInspector] public GameObject previewObject; // 설치 미리보기 오브젝트
	private playerMoveController playerController; // 플레이어 컨트롤러 참조


    void Start()
    {
        if (IsOwner) // 현재 클라이언트가 오브젝트의 소유자인지 확인
        {
            InitializePreviewObject(previewPrefab);
        }

        // 플레이어 컨트롤러 찾기 시작
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
                        Debug.Log("PlayerController를 찾았습니다.");
                    }
                    
                }
			}
			catch
			{
                if (enableLogs)
                {
                    Debug.Log("PlayerController를 찾는 중...");
                }
            }
            
            yield return new WaitForSeconds(1f); // 1초마다 반복
        }
        while (netInvenController == null)
        {
            try
            {
                var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject(); // 로컬 플레이어 오브젝트 찾기
                if (playerController != null)
                {
                    netInvenController = playerObject.GetComponent<NetworkInventoryController>();
                    if (enableLogs)
                        Debug.Log("netInvenController 찾았습니다.");
                }
            }
            catch
            {
                if (enableLogs)
                {
                    Debug.Log("netInvenController를 찾는 중...");
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
                    Debug.Log("spawnedObjectParent 를 찾았습니다.");
                }
            }
            catch
            {
                if (enableLogs)
                {
                    Debug.Log("spawnedObjectParent 를 찾는 중...");
                }
            }

            yield return new WaitForSeconds(1f); // 1초마다 반복
        }

      
    }
    public void InitializePreviewObject(GameObject previewPrefab)
    {
        Quaternion spawnRotation = Quaternion.Euler(Vector3.zero);
        // 프리팹을 복제하여 설치 미리보기 오브젝트 생성
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
        targetPosition.y = GetGroundHeight(targetPosition); // 바닥 높이를 찾음
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


    [ServerRpc(RequireOwnership = false)] // 소유권이 필요하지 않도록 설정
    void PlaceObjectServerRpc(Vector3 position, Quaternion rotation, NetworkString path ,ServerRpcParams rpcParams = default)
    {
        GameObject loadObject = Resources.Load<GameObject>(path);
        // 모든 클라이언트에서 오브젝트를 설치하는 ClientRpc 호출
        GameObject placedObject = Instantiate(loadObject, position, rotation);
        NetworkObject networkObject = placedObject.GetComponent<NetworkObject>();
        networkObject.Spawn();
        NetworkObject parentObject = NetworkManager.SpawnManager.SpawnedObjects[spawnedObjectParent.NetworkObjectId];
        networkObject.transform.SetParent(parentObject.transform, true);        
        PlaceObjectClientRpc(networkObject.NetworkObjectId, position, rotation , rpcParams.Receive.SenderClientId);
    }

    [ClientRpc] // 클라이언트에서 호출되는 RPC 메서드
    void PlaceObjectClientRpc(ulong objectId, Vector3 position, Quaternion rotation, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {;
            NetworkObject networkObject = NetworkManager.SpawnManager.SpawnedObjects[objectId];
            spawnedItem = networkObject.gameObject;
            spawnedItemID = objectId;
            netInvenController.IsPlacingItem = false;
            // ClientRpc 완료 표시
            clientRpcCompleted = true;
        }
    }

    public void PreviewDestroy()// 미리보기 오브젝트 삭제
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
            PreviewDestroy();//미리보기 삭제
            

            // 인벤토리에서 아이템 제거 요청
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
        if (Input.GetMouseButtonDown(0)) // 왼쪽 마우스 버튼을 누르면
        {
            isRotating = true;
            playerController.SetMouseControl(false); // 플레이어 회전 중지
        }
        if (Input.GetMouseButtonUp(0)) // 왼쪽 마우스 버튼을 떼면
        {
            networkLoading = true;            
            StartCoroutine(UseItem(itemPath)); 
            isRotating = false;
            isPlacingItem = false; // 배치 모드 비활성화
            playerController.SetMouseControl(true); // 플레이어 회전 재개            
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
