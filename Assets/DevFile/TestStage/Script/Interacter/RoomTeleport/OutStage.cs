using UnityEngine;
using System.Collections;
using Unity.Netcode;
using System;

public class OutStage : InteractableObject
{
    [SerializeField] Transform spawnPoint;
    [SerializeField] AudioSource otherSideDoorSource;

    public Action<bool> outDoorAction;

	private void OnEnable()
	{
        GameObject.Find("CompassManager").GetComponent<Compass>().outStage = this;
        outDoorAction = GameObject.Find("CompassManager").GetComponent<Compass>().setDungeunBool;
	}

	public override bool Interact(ulong uerID, Transform interactingObjectTransform)
    {
        if (!base.Interact(uerID, interactingObjectTransform))
            return false;

        if (spawnPoint == null)
        {
            spawnPoint = GameObject.Find("OutPoint").GetComponent<Transform>();
        }
        PlayDoorSound();
        SetPlayerPosServerRPC(uerID);
        outDoorAction.Invoke(false);

        // 2024.12.24 ���� ���� �̺�Ʈ ���ġ
        // 2024.12.26 ���� ���� �̺�Ʈ ���ġ
        //InTheDark.Prototypes.Game.OnDungeonExit.Invoke(new InTheDark.Prototypes.DungeonExitEvent()
        //{
        //	BuildIndex = 0
        //});

        return true;
    }

    private void PlayDoorSound()
    {
        if (otherSideDoorSource == null)
        {
            otherSideDoorSource = GameObject.Find("EnterDoor_1_SoundSource").GetComponent<AudioSource>();
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
            spawnPoint = GameObject.Find("OutPoint").transform;

        var cc = playerObject.GetComponent<CharacterController>();
        var damageHandler = playerObject.GetComponent<PlayerDamageHandler>();

        // CC ���� �̵�
        cc.enabled = false;
        playerObject.transform.position = spawnPoint.position;

        // ����/��ȯ ����ȭ �� CC ��Ȱ��
        Physics.SyncTransforms();
        cc.enabled = true;

        // CC ���� velocity �ʱ�ȭ(�߿�)
        cc.Move(Vector3.zero);

        // ���� ������ �׷��̽� ����(����)
        if (damageHandler != null)
            damageHandler.BeginTeleportGrace(0.3f);

        // Ŭ�� ������ ���� + �׷��̽� ����(Ŭ��)
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
}
