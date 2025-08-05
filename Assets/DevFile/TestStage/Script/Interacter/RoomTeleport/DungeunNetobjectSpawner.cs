using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class DungeunNetobjectSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> spawnObjects;
    [SerializeField] private List<Transform> spawnMarkers;  // 프리팹 내부의 Empty 오브젝트들

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
            spawnPos.Add(marker.position);      // 로컬 기준
            spawnRot.Add(marker.localRotation);      // 로컬 회전
        }
    }


    [ServerRpc]
    private void SpawnAllObjectsServerRpc()
    {
        // spawnObjects, spawnPos, spawnRot의 갯수 확인
        if (spawnObjects.Count > spawnPos.Count || spawnObjects.Count > spawnRot.Count)
        {
            Debug.LogError("Not enough spawn positions or rotations for all spawn objects.");
            return;
        }

        for (int i = 0; i < spawnObjects.Count; i++)
        {
            // 로컬 위치와 회전을 월드 기준으로 변환
            Vector3 worldPosition = spawnPos[i];
            Quaternion worldRotation = transform.rotation * spawnRot[i];

            // 객체 생성
            GameObject obj = Instantiate(spawnObjects[i], worldPosition, worldRotation, transform);

            // NetworkObject 스폰 처리
            NetworkObject networkObj = obj.GetComponent<NetworkObject>();
            if (networkObj != null && !networkObj.IsSpawned)
            {
                networkObj.Spawn();
            }

            // 자식 객체 스폰 처리
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
