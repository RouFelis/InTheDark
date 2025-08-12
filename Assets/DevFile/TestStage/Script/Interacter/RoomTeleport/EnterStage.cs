using UnityEngine;
using Unity.Netcode;
using System;

public class EnterStage : InteractableObject
{   
    [SerializeField] Transform spawnPoint;
    [SerializeField] AudioSource otherSideDoorSource;

    public Action<bool> enterDoorAction;

    public override bool Interact(ulong uerID, Transform interactingObjectTransform)
    {
        if (!base.Interact(uerID, interactingObjectTransform))
            return false;

        if (spawnPoint == null)
		{
            spawnPoint = GameObject.Find("StageSpawn").GetComponent<Transform>();            
        }
        PlayDoorSound(); 
        SetPlayerPosServerRPC(uerID);
        enterDoorAction.Invoke(true);

        return true;
    }


	private void PlayDoorSound()
	{
        if (otherSideDoorSource == null)
        {
            otherSideDoorSource = GameObject.Find("OutDoor_1_SoundSource").GetComponent<AudioSource>();
        }
        otherSideDoorSource.Play();
    }


    [ServerRpc(RequireOwnership = false)]
    void SetPlayerPosServerRPC(ulong playerId)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(playerId, out var client))
            return;

        var playerObject = client.PlayerObject;
        if (spawnPoint == null)
            spawnPoint = GameObject.Find("StageSpawn").transform;

        var cc = playerObject.GetComponent<CharacterController>();
        var damageHandler = playerObject.GetComponent<PlayerDamageHandler>();

        // CC 끄고 이동
        cc.enabled = false;
        playerObject.transform.position = spawnPoint.position;

        // 물리/변환 동기화 후 CC 재활성
        Physics.SyncTransforms();
        cc.enabled = true;

        // CC 내부 velocity 초기화(중요)
        cc.Move(Vector3.zero);

        // 낙하 데미지 그레이스 시작(서버)
        if (damageHandler != null)
            damageHandler.BeginTeleportGrace(0.3f);

        // 클라 포지션 스냅 + 그레이스 시작(클라)
        MovePlayerClientRpc(playerObject.NetworkObjectId, spawnPoint.position);
        BeginTeleportGraceClientRpc(playerObject.NetworkObjectId, 0.3f);
    }

    [ClientRpc]
    void BeginTeleportGraceClientRpc(ulong netObjId, float seconds)
    {
        var obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[netObjId];
        var dh = obj.GetComponent<PlayerDamageHandler>();
        if (dh != null) dh.BeginTeleportGrace(seconds);
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
}
