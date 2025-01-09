using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class DungeunNetobjectSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> spawnObjects;

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
            //DestroyAllObjects();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnAllObjectsServerRpc()
    {
        foreach (var obj in spawnObjects)
        {
            NetworkObject spawnedObj = obj.GetComponent<NetworkObject>();
            if (spawnedObj != null && !spawnedObj.IsSpawned)
            {
                spawnedObj.Spawn();
            }
            var test = spawnedObj.transform.GetComponentsInChildren<Transform>();

            foreach (Transform child in test)
            {
                NetworkObject childNetworkObject = child.GetComponent<NetworkObject>();
                if (childNetworkObject != null && !childNetworkObject.IsSpawned)
                {
                    childNetworkObject.Spawn();
                    child.SetParent(spawnedObj.transform);
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
                Destroy(obj.gameObject);
            }
        }
    }
}
