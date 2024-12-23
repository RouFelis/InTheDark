using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class WaitRoomSetter : NetworkBehaviour
{
    [SerializeField] List<Transform> spawnPoint;
    private void Awake()
    {
        // 클라이언트가 접속할 때 실행되는 콜백 등록
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    public override void OnDestroy()
    {
        // 콜백 등록 해제
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId) // 서버에서만 처리
        {
            Debug.Log($"클라이언트 {clientId} 접속!");
            SetUserPosServerRPC(clientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SetUserPosServerRPC(ulong userID)
    { 
        // 접속한 클라이언트의 플레이어 오브젝트 찾기
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(userID, out var client))
        {
            var playerObject = client.PlayerObject;

            if (playerObject != null && playerObject.TryGetComponent(out CharacterController characterController))
            {
                // 캐릭터 컨트롤러 비활성화
                characterController.enabled = false;

                // 스폰 위치 계산 (부모 오브젝트 제외)
                int spawnIndex = (int)(userID % (ulong)(spawnPoint.Count - 1)) + 1;
                Vector3 spawnPosition = spawnPoint[spawnIndex].position;

                // 플레이어 위치 이동
                playerObject.transform.position = spawnPosition;
                characterController.enabled = true;

                // 클라이언트들에게 위치 동기화
                MovePlayerClientRpc(playerObject.NetworkObjectId, spawnPosition);

                Debug.Log($"플레이어 {userID} 위치를 {spawnPosition}로 이동했습니다.");
            }
        }
    }

    [ClientRpc]
    private void MovePlayerClientRpc(ulong playerId, Vector3 targetPosition)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerId, out var playerObject))
        {
            if (playerObject.TryGetComponent(out CharacterController characterController))
            {
                characterController.enabled = false;
                playerObject.transform.position = targetPosition;
                characterController.enabled = true;
            }
        }
    }
}

