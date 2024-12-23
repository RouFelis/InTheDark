using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class DungeunNetobjectSpawner : MonoBehaviour
{
    [SerializeField] private List<NetworkObject> spawnObjects;

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // ������ ��� ��� ��ü�� ����
            SpawnAllObjectsServerRpc();
        }
        else
        {
            // Ŭ���̾�Ʈ�� ��� ��� ��ü�� �ı�
            DestroyAllObjects();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnAllObjectsServerRpc()
    {
        foreach (var obj in spawnObjects)
        {
            if (obj != null && !obj.IsSpawned)
            {
                obj.Spawn();
            }
        }
    }

    private void DestroyAllObjects()
    {
        foreach (var obj in spawnObjects)
        {
            if (obj != null)
            {
                Destroy(obj.gameObject);
            }
        }
    }
}
