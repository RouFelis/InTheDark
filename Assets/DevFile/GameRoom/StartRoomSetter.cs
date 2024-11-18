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
        // 스폰포인트 자식객체에 모든 스폰지점을 담아놓음
        var spawnPoint = GameObject.Find("SpawnPoint").GetComponentsInChildren<Transform>();
        // 서버에서만 실행
        if (NetworkManager.Singleton.IsServer)
        {
            int replaceNum = 0;
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                // 클라이언트의 플레이어 객체 접근
                var playerObject = client.PlayerObject;

                // 플레이어 순간이동시에 CharacterController는 꺼야하더라~
                playerObject.gameObject.GetComponent<CharacterController>().enabled = false;

                Vector3 teleportPos = spawnPoint[replaceNum].transform.position;
                replaceNum++;

                //위치이동
                playerObject.transform.position = teleportPos;
                playerObject.gameObject.GetComponent<CharacterController>().enabled = true;

                MovePlayerClientRpc(playerObject.NetworkObjectId, teleportPos);

                Debug.Log("Set PlayerPosition at " + teleportPos + " .....");
            }
        }
    }

    // 모든 클라이언트에게 이동을 반영하는 RPC
    [ClientRpc]
    private void MovePlayerClientRpc(ulong playerId, Vector3 targetPosition)
    {
        // 각 클라이언트가 자신의 위치를 업데이트
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
