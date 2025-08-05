using UnityEngine;
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
        SetEveryPlayerPosServerRPC(uerID);
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
    void SetEveryPlayerPosServerRPC(ulong playerId)
    {
        Debug.Log(playerId);
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(playerId, out var client))
        {
            var playerObject = client.PlayerObject;

            if (spawnPoint == null)
            {
                spawnPoint = GameObject.Find("OutPoint").GetComponent<Transform>();
            }

            playerObject.gameObject.GetComponent<CharacterController>().enabled = false;

            //��ġ�̵�
            playerObject.transform.position = spawnPoint.position;
            playerObject.gameObject.GetComponent<CharacterController>().enabled = true;

            MovePlayerClientRpc(playerObject.NetworkObjectId, spawnPoint.position);

            Debug.Log("Set PlayerPosition at " + spawnPoint.position + " .....");
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
}
