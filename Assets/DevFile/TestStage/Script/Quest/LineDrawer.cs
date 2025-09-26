using UnityEngine;
using Unity.Netcode;

public class LineDrawer : NetworkBehaviour
{    
    public float cableRadius = 0.1f; // ������ �β�

    [SerializeField] private GameObject cableObject;
    [SerializeField] private NetworkObject cableNet;

    public Material mat;
    public GameObject cablePrefab;

    [HideInInspector] public MeshRenderer createdMesh;
   public Transform startPoint; // ���� ������    
    [HideInInspector] public Color matColor;
    [HideInInspector] public bool isDraw = false;

    public float maxDistance = 20f;
    public float camDistance = 1f;

    [ServerRpc(RequireOwnership = false)]
    public void SpawnCylinderServerRpc(ulong clientId, int matColor)
    {
        // �������� ������Ʈ ����
        var spawnedObject = Instantiate(cablePrefab);
        var networkObject = spawnedObject.GetComponent<NetworkObject>();
        networkObject.Spawn();
        cableNet = networkObject;
        networkObject.ChangeOwnership(clientId);

        // ������ ������Ʈ ������ �ش� Ŭ���̾�Ʈ���� ����
        SendSpawnInfoToClientRpc(clientId, networkObject.NetworkObjectId, matColor);
        Debug.Log("Ŭ��" + clientId);
    }

    [ClientRpc]
    private void SendSpawnInfoToClientRpc(ulong targetClientId, ulong networkObjectId, int matColor, ClientRpcParams clientRpcParams = default)
    {
        var networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
        cableNet = networkObject;
        Debug.Log("Ÿ�� " + networkObjectId);
        Quest2_Marker marker = cableNet.GetComponent<Quest2_Marker>();

        if (networkObject != null)
        {
            SetColor(matColor);
            var meshRenderer = marker.letMesh;
            if (meshRenderer != null && meshRenderer.materials.Length > 0)
            {
                meshRenderer.materials[0].color = this.matColor;
            }
        }
        // Ŭ���̾�Ʈ�� �ش� ������Ʈ�� �ʱ�ȭ
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
            cableObject = marker.wireGameobject;
        }        
    }

    public void SetColor(int matColor)
    {
        // value�� 0~15�� ����
        int value = Mathf.Clamp(matColor, 0, 15);

        // HSV ���� ��� (Hue�� 0~1 ���̷� �й�)
        float hue = value / 15f; // 0���� 1���� ����
        this.matColor = Color.HSVToRGB(hue, 1f, 1f); // ä��(S)�� ��(V)�� �ִ밪���� ����
    }

    public void InitDrawer(Transform _startPoint , int matColor , ulong uerID)
	{
        this.startPoint = _startPoint;
        SpawnCylinderServerRpc(uerID, matColor);
        isDraw = false;
    }

	public void EndDraw(Transform _endPoint)
	{
        isDraw = true;

        // ������ ���̿� ��ġ ������Ʈ
        Vector3 midPoint = (startPoint.position + _endPoint.position) / 2;
        cableObject.transform.position = midPoint;

        // ���� ���� ���߱�
        Vector3 direction = _endPoint.position - startPoint.position;
        cableObject.transform.up = direction.normalized;

        // ���� ���̿� �β� ����
        cableObject.transform.localScale = new Vector3(cableRadius, direction.magnitude / 2, cableRadius);

        cableNet = null;
        cableObject = null;
    }

    public void MissDraw()
	{
        isDraw = true;
        DestroyCableServerRpc();
    }

    [ServerRpc(RequireOwnership =false)]
    private void DestroyCableServerRpc()
	{
        Destroy(cableNet);
        //cableNet.Despawn();
    }

    void Update()
    {
		if (startPoint == null)
		{
            return;
		}
		if (!isDraw)
		{
            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraPosition = Camera.main.transform.position;
            Vector3 endPointPosition = cameraPosition + cameraForward * camDistance;


            float distance = Vector3.Distance(startPoint.position, endPointPosition);

            if (distance > maxDistance)
            {
                Destroy(gameObject);
                return;
            }

            Debug.Log("�׽�Ƶ");


            // ������ ���̿� ��ġ ������Ʈ
            Vector3 midPoint = (startPoint.position + endPointPosition) / 2;
            cableObject.transform.position = midPoint;

            // ���� ���� ���߱�
            Vector3 direction = endPointPosition - startPoint.position;
            cableObject.transform.up = direction.normalized;

            // ���� ���̿� �β� ����
            cableObject.transform.localScale = new Vector3(cableRadius, direction.magnitude / 2, cableRadius);
		}        
    }
}
