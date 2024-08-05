using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode; // Netcode ���ӽ����̽� �߰�

public class TestReplace : NetworkBehaviour // NetworkBehaviour ���
{
    public GameObject previewPrefab; // ��ġ �̸����� ������
    public GameObject objectPrefab; // ��ġ�� ������Ʈ�� ������
    public float maxPlacementDistance = 10f; // �ִ� ��ġ �Ÿ�
    public Material transparentMaterial; // ������ ��Ƽ����
    public Material validPlacementMaterial; // ��ġ ������ ��ġ ��Ƽ����
    public Material invalidPlacementMaterial; // ��ġ �Ұ����� ��ġ ��Ƽ����
    public float rotationSpeed = 10f; // ȸ�� �ӵ�

    private GameObject previewObject; // ��ġ �̸����� ������Ʈ
    private bool canPlace; // ��ġ ���� ����
    private float currentRotation = 0f; // ���� ȸ�� ����

    void Start()
    {
        if (IsOwner) // ���� Ŭ���̾�Ʈ�� ������Ʈ�� ���������� Ȯ��
        {
            // �������� �����Ͽ� ��ġ �̸����� ������Ʈ ����
            previewObject = Instantiate(previewPrefab);
            SetObjectTransparent(previewObject);
        }
    }

    void Update()
    {
        if (IsOwner) // ���� Ŭ���̾�Ʈ�� ������Ʈ�� ���������� Ȯ��
        {
            UpdatePreviewObject();
            HandleRotation();
            if (Input.GetMouseButtonDown(0) && canPlace)
            {
                // ������ ��ġ ��ġ�� ȸ���� �����ϴ� ServerRpc ȣ��
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

                // �ٴ� �±� Ȯ��
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

        // ���� ȸ�� ������ �̸����� ������Ʈ�� ����
        previewObject.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);
    }

    void PlacePreviewBelow(Ray ray)
    {
        Vector3 targetPosition = ray.origin + ray.direction * maxPlacementDistance;
        targetPosition.y = GetGroundHeight(targetPosition); // �ٴ� ���̸� ã��
        previewObject.transform.position = targetPosition;

        // �ٴ� �±� Ȯ��
        if (Physics.Raycast(previewObject.transform.position, Vector3.down, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Ground")) // "Ground" �±� Ȯ��
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

    [ServerRpc] // �������� ȣ��Ǵ� RPC �޼���
    void PlaceObjectServerRpc(Vector3 position, Quaternion rotation)
    {
        // ��� Ŭ���̾�Ʈ���� ������Ʈ�� ��ġ�ϴ� ClientRpc ȣ��
        PlaceObjectClientRpc(position, rotation);
    }

    [ClientRpc] // Ŭ���̾�Ʈ���� ȣ��Ǵ� RPC �޼���
    void PlaceObjectClientRpc(Vector3 position, Quaternion rotation)
    {
        // ������Ʈ ���� �� ��Ʈ��ũ�� ����
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
