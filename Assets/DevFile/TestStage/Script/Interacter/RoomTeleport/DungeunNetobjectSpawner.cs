using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class DungeunNetobjectSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> spawnObjects;
    [SerializeField] private List<Vector3> spawnPos; // �θ� ���� ���� ��ġ
    [SerializeField] private List<Quaternion> spawnRot; // �θ� ���� ���� ȸ��

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            SpawnAllObjectsServerRpc();
        }
        else
        {
          // DestroyAllObjects();
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
            Vector3 worldPosition = transform.TransformPoint(spawnPos[i]);
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
