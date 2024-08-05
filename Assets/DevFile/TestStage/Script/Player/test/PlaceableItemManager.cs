using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlaceableItemManager : NetworkBehaviour
{
    public GameObject previewPrefab; // 설치 미리보기 프리팹
    public GameObject objectPrefab; // 설치할 오브젝트의 프리팹
    public float maxPlacementDistance = 10f; // 최대 설치 거리
    public Material transparentMaterial; // 투명한 머티리얼
    public Material validPlacementMaterial; // 설치 가능한 위치 머티리얼
    public Material invalidPlacementMaterial; // 설치 불가능한 위치 머티리얼
    public float rotationSpeed = 100f; // 회전 속도
    public bool enableLogs = true; // 로그 활성화 체크박스

    private GameObject previewObject; // 설치 미리보기 오브젝트
    public bool canPlace; // 설치 가능 여부
    private float currentRotation = 0f; // 현재 회전 각도
    private bool isRotating = false; // 회전 중인지 여부
    [SerializeField] private NetworkInventoryController netInvenController; 
    [SerializeField] private testMove playerController; // 플레이어 컨트롤러 참조

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
            var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject(); // 로컬 플레이어 오브젝트 찾기
            if (playerObject != null)
            {
                playerController = playerObject.GetComponent<testMove>(); // 플레이어 컨트롤러 찾기
                netInvenController = playerObject.GetComponent<NetworkInventoryController>();
            }
            yield return null;
        }
    }

   
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
                    playerController = playerObject.GetComponent<testMove>();
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
    }
    public void InitializePreviewObject(GameObject previewPrefab)
    {
        // 프리팹을 복제하여 설치 미리보기 오브젝트 생성
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
        targetPosition.y = GetGroundHeight(targetPosition); // 바닥 높이를 찾음
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

    [ServerRpc(RequireOwnership = false)] // 소유권이 필요하지 않도록 설정
    void PlaceObjectServerRpc(Vector3 position, Quaternion rotation, ServerRpcParams rpcParams = default)
    {
        // 모든 클라이언트에서 오브젝트를 설치하는 ClientRpc 호출
        GameObject placedObject = Instantiate(objectPrefab, position, rotation);
        placedObject.GetComponent<NetworkObject>().Spawn();
        PlaceObjectClientRpc(position, rotation);
    }

    [ClientRpc] // 클라이언트에서 호출되는 RPC 메서드
    void PlaceObjectClientRpc(Vector3 position, Quaternion rotation)
    {
        if (IsOwner)
        {
            PreviewDestroy();//미리보기 삭제
            netInvenController.IsPlacingItem = false;
        }
    }

    public void PreviewDestroy()// 미리보기 오브젝트 삭제
    {
        Destroy(previewObject); 
    }

    public void UseItem()
    {
        if (canPlace)
        {
            PlaceObjectServerRpc(previewObject.transform.position, previewObject.transform.rotation);          

            // 인벤토리에서 아이템 제거 요청
            playerController.GetComponent<NetworkInventoryController>().UseCurrentSelectedItem();
        }
        Destroy(previewObject); // 미리보기 오브젝트 삭제

        PreviewDestroy();//미리보기 삭제
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
        if (Input.GetMouseButtonDown(0)) // 왼쪽 마우스 버튼을 누르면
        {
            isRotating = true;
            playerController.SetMouseControl(false); // 플레이어 회전 중지
        }
        if (Input.GetMouseButtonUp(0)) // 왼쪽 마우스 버튼을 떼면
        {
            UseItem(); // 설치            
            isRotating = false;
            playerController.SetMouseControl(true); // 플레이어 회전 재개
            isPlacingItem = false; // 배치 모드 비활성화
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
