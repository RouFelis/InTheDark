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
            // 서버인 경우 모든 객체를 스폰
            SpawnAllObjectsServerRpc();
        }
        else
        {
            // 클라이언트인 경우 모든 객체를 파괴
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
