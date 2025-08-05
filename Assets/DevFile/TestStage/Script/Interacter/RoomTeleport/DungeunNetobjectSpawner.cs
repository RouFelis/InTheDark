using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class DungeunNetobjectSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> spawnObjects;
    [SerializeField] private List<Transform> spawnMarkers;  // ������ ������ Empty ������Ʈ��

    private List<Vector3> spawnPos = new List<Vector3>();
    private List<Quaternion> spawnRot = new List<Quaternion>();

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            ExtractSpawnDataFromMarkers();
            SpawnAllObjectsServerRpc();
        }
    }

    private void ExtractSpawnDataFromMarkers()
    {
        spawnPos.Clear();
        spawnRot.Clear();

        foreach (var marker in spawnMarkers)
        {
            spawnPos.Add(marker.position);      // ���� ����
            spawnRot.Add(marker.localRotation);      // ���� ȸ��
        }
    }


    [ServerRpc]
    private void SpawnAllObjectsServerRpc()
    {
        // spawnObjects, spawnPos, spawnRot�� ���� Ȯ��
        if (spawnObjects.Count > spawnPos.Count || spawnObjects.Count > spawnRot.Count)
        {
            Debug.LogError("Not enough spawn positions or rotations for all spawn objects.");
            return;
        }

        for (int i = 0; i < spawnObjects.Count; i++)
        {
            // ���� ��ġ�� ȸ���� ���� �������� ��ȯ
            Vector3 worldPosition = spawnPos[i];
            Quaternion worldRotation = transform.rotation * spawnRot[i];

            // ��ü ����
            GameObject obj = Instantiate(spawnObjects[i], worldPosition, worldRotation, transform);

            // NetworkObject ���� ó��
            NetworkObject networkObj = obj.GetComponent<NetworkObject>();
            if (networkObj != null && !networkObj.IsSpawned)
            {
                networkObj.Spawn();
            }

            // �ڽ� ��ü ���� ó��
            var childTransforms = obj.GetComponentsInChildren<Transform>();
            foreach (Transform child in childTransforms)
            {
                NetworkObject childNetworkObject = child.GetComponent<NetworkObject>();
                if (childNetworkObject != null && !childNetworkObject.IsSpawned)
                {
                    childNetworkObject.Spawn();
                }
            }
        }
    }

    private void DestroyAllObjects()
    {
        foreach (var obj in spawnObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
    }
}
