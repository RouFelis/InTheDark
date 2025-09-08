using UnityEngine;
using Unity.Netcode;

public class LineDrawer : NetworkBehaviour
{    
    public float cableRadius = 0.1f; // 전선의 두께

    [SerializeField] private GameObject cableObject;
    [SerializeField] private NetworkObject cableNet;

    public Material mat;
    public GameObject cablePrefab;

    [HideInInspector] public MeshRenderer createdMesh;
   public Transform startPoint; // 전선 시작점    
    [HideInInspector] public Color matColor;
    [HideInInspector] public bool isDraw = false;

    public float maxDistance = 20f;
    public float camDistance = 1f;

    [ServerRpc(RequireOwnership = false)]
    public void SpawnCylinderServerRpc(ulong clientId, int matColor)
    {
        // 서버에서 오브젝트 생성
        var spawnedObject = Instantiate(cablePrefab);
        var networkObject = spawnedObject.GetComponent<NetworkObject>();
        networkObject.Spawn();
        cableNet = networkObject;
        networkObject.ChangeOwnership(clientId);

        // 생성된 오브젝트 정보를 해당 클라이언트에게 전달
        SendSpawnInfoToClientRpc(clientId, networkObject.NetworkObjectId, matColor);
        Debug.Log("클라" + clientId);
    }

    [ClientRpc]
    private void SendSpawnInfoToClientRpc(ulong targetClientId, ulong networkObjectId, int matColor, ClientRpcParams clientRpcParams = default)
    {
        var networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
        cableNet = networkObject;
        Debug.Log("타겟 " + networkObjectId);
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
        // 클라이언트가 해당 오브젝트를 초기화
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
            cableObject = marker.wireGameobject;
        }        
    }

    public void SetColor(int matColor)
    {
        // value를 0~15로 제한
        int value = Mathf.Clamp(matColor, 0, 15);

        // HSV 색상 계산 (Hue를 0~1 사이로 분배)
        float hue = value / 15f; // 0부터 1까지 나눔
        this.matColor = Color.HSVToRGB(hue, 1f, 1f); // 채도(S)와 명도(V)를 최대값으로 설정
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

        // 전선의 길이와 위치 업데이트
        Vector3 midPoint = (startPoint.position + _endPoint.position) / 2;
        cableObject.transform.position = midPoint;

        // 전선 방향 맞추기
        Vector3 direction = _endPoint.position - startPoint.position;
        cableObject.transform.up = direction.normalized;

        // 전선 길이와 두께 조절
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

            Debug.Log("테스틉");


            // 전선의 길이와 위치 업데이트
            Vector3 midPoint = (startPoint.position + endPointPosition) / 2;
            cableObject.transform.position = midPoint;

            // 전선 방향 맞추기
            Vector3 direction = endPointPosition - startPoint.position;
            cableObject.transform.up = direction.normalized;

            // 전선 길이와 두께 조절
            cableObject.transform.localScale = new Vector3(cableRadius, direction.magnitude / 2, cableRadius);
		}        
    }
}
