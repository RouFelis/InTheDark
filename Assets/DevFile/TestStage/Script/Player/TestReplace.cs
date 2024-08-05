using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode; // Netcode 네임스페이스 추가

public class TestReplace : NetworkBehaviour // NetworkBehaviour 상속
{
    public GameObject previewPrefab; // 설치 미리보기 프리팹
    public GameObject objectPrefab; // 설치할 오브젝트의 프리팹
    public float maxPlacementDistance = 10f; // 최대 설치 거리
    public Material transparentMaterial; // 투명한 머티리얼
    public Material validPlacementMaterial; // 설치 가능한 위치 머티리얼
    public Material invalidPlacementMaterial; // 설치 불가능한 위치 머티리얼
    public float rotationSpeed = 10f; // 회전 속도

    private GameObject previewObject; // 설치 미리보기 오브젝트
    private bool canPlace; // 설치 가능 여부
    private float currentRotation = 0f; // 현재 회전 각도

    void Start()
    {
        if (IsOwner) // 현재 클라이언트가 오브젝트의 소유자인지 확인
        {
            // 프리팹을 복제하여 설치 미리보기 오브젝트 생성
            previewObject = Instantiate(previewPrefab);
            SetObjectTransparent(previewObject);
        }
    }

    void Update()
    {
        if (IsOwner) // 현재 클라이언트가 오브젝트의 소유자인지 확인
        {
            UpdatePreviewObject();
            HandleRotation();
            if (Input.GetMouseButtonDown(0) && canPlace)
            {
                // 서버에 설치 위치와 회전을 전달하는 ServerRpc 호출
                PlaceObjectServerRpc(previewObject.transform.position, previewObject.transform.rotation);
            }
        }
    }

    void UpdatePreviewObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * maxPlacementDistance, Color.green);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            float distance = Vector3.Distance(Camera.main.transform.position, hit.point);
            if (distance <= maxPlacementDistance)
            {
                previewObject.transform.position = hit.point;

                // 바닥 태그 확인
                if (hit.collider.CompareTag("Ground"))
                {
                    canPlace = true;
                    SetObjectMaterial(previewObject, validPlacementMaterial);
                }
                else
                {
                    canPlace = false;
                    SetObjectMaterial(previewObject, invalidPlacementMaterial);
                }
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

        // 현재 회전 각도를 미리보기 오브젝트에 적용
        previewObject.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);
    }

    void PlacePreviewBelow(Ray ray)
    {
        Vector3 targetPosition = ray.origin + ray.direction * maxPlacementDistance;
        targetPosition.y = GetGroundHeight(targetPosition); // 바닥 높이를 찾음
        previewObject.transform.position = targetPosition;

        // 바닥 태그 확인
        if (Physics.Raycast(previewObject.transform.position, Vector3.down, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Ground")) // "Ground" 태그 확인
            {
                canPlace = true;
                SetObjectMaterial(previewObject, validPlacementMaterial);
            }
            else
            {
                canPlace = false;
                SetObjectMaterial(previewObject, invalidPlacementMaterial);
            }
        }
        else
        {
            canPlace = false;
            SetObjectMaterial(previewObject, invalidPlacementMaterial);
        }
    }

    float GetGroundHeight(Vector3 position)
    {
        if (Physics.Raycast(position, Vector3.down, out RaycastHit hit, Mathf.Infinity))
        {
            return hit.point.y;
        }
        return position.y;
    }

    [ServerRpc] // 서버에서 호출되는 RPC 메서드
    void PlaceObjectServerRpc(Vector3 position, Quaternion rotation)
    {
        // 모든 클라이언트에서 오브젝트를 설치하는 ClientRpc 호출
        PlaceObjectClientRpc(position, rotation);
    }

    [ClientRpc] // 클라이언트에서 호출되는 RPC 메서드
    void PlaceObjectClientRpc(Vector3 position, Quaternion rotation)
    {
        // 오브젝트 생성 및 네트워크에 스폰
        Instantiate(objectPrefab, position, rotation).GetComponent<NetworkObject>().Spawn();
    }

    void HandleRotation()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            currentRotation -= rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.E))
        {
            currentRotation += rotationSpeed * Time.deltaTime;
        }
    }

    void SetObjectTransparent(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material = transparentMaterial;
        }
    }

    void SetObjectMaterial(GameObject obj, Material material)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material = material;
        }
    }
}
