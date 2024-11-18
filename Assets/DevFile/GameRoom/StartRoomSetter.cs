using UnityEngine;
using Unity.Netcode;


public class StartRoomSetter : NetworkBehaviour
{
    void Start()
    {
        SetEveryPlayerPos();
    }

    [ServerRpc]
    void SetEveryPlayerPosServerRPC()
    {
        // ��������Ʈ �ڽİ�ü�� ��� ���������� ��Ƴ���
        var spawnPoint = GameObject.Find("SpawnPoint").GetComponentsInChildren<Transform>();
        // ���������� ����
        if (NetworkManager.Singleton.IsServer)
        {
            int replaceNum = 0;
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                // Ŭ���̾�Ʈ�� �÷��̾� ��ü ����
                var playerObject = client.PlayerObject;

                // �÷��̾� �����̵��ÿ� CharacterController�� �����ϴ���~
                playerObject.gameObject.GetComponent<CharacterController>().enabled = false;

                Vector3 teleportPos = spawnPoint[replaceNum].transform.position;
                replaceNum++;

                //��ġ�̵�
                playerObject.transform.position = teleportPos;
                playerObject.gameObject.GetComponent<CharacterController>().enabled = true;

                MovePlayerClientRpc(playerObject.NetworkObjectId, teleportPos);

                Debug.Log("Set PlayerPosition at " + teleportPos + " .....");
            }
        }
    }

    // ��� Ŭ���̾�Ʈ���� �̵��� �ݿ��ϴ� RPC
    [ClientRpc]
    private void MovePlayerClientRpc(ulong playerId, Vector3 targetPosition)
    {
        // �� Ŭ���̾�Ʈ�� �ڽ��� ��ġ�� ������Ʈ
        NetworkObject playerObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerId];
        if (playerObject != null && playerObject.TryGetComponent(out CharacterController characterController))
        {
            characterController.enabled = false;
            playerObject.transform.position = targetPosition;
            characterController.enabled = true;
        }
    }

    protected void SetEveryPlayerPos()
    {
        if (NetworkManager.Singleton.IsServer)
            SetEveryPlayerPosServerRPC();
    }

}
