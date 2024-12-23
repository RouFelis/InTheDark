using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class WaitRoomSetter : NetworkBehaviour
{
    [SerializeField] List<Transform> spawnPoint;
    private void Awake()
    {
        // Ŭ���̾�Ʈ�� ������ �� ����Ǵ� �ݹ� ���
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    public override void OnDestroy()
    {
        // �ݹ� ��� ����
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId) // ���������� ó��
        {
            Debug.Log($"Ŭ���̾�Ʈ {clientId} ����!");
            SetUserPosServerRPC(clientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SetUserPosServerRPC(ulong userID)
    { 
        // ������ Ŭ���̾�Ʈ�� �÷��̾� ������Ʈ ã��
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(userID, out var client))
        {
            var playerObject = client.PlayerObject;

            if (playerObject != null && playerObject.TryGetComponent(out CharacterController characterController))
            {
                // ĳ���� ��Ʈ�ѷ� ��Ȱ��ȭ
                characterController.enabled = false;

                // ���� ��ġ ��� (�θ� ������Ʈ ����)
                int spawnIndex = (int)(userID % (ulong)(spawnPoint.Count - 1)) + 1;
                Vector3 spawnPosition = spawnPoint[spawnIndex].position;

                // �÷��̾� ��ġ �̵�
                playerObject.transform.position = spawnPosition;
                characterController.enabled = true;

                // Ŭ���̾�Ʈ�鿡�� ��ġ ����ȭ
                MovePlayerClientRpc(playerObject.NetworkObjectId, spawnPosition);

                Debug.Log($"�÷��̾� {userID} ��ġ�� {spawnPosition}�� �̵��߽��ϴ�.");
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

