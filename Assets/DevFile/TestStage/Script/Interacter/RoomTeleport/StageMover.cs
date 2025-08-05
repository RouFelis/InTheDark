using UnityEngine;
using Unity.Netcode;

public class StageMover : InteractableObject
{
    public Transform spawnPoint;

	public override bool Interact(ulong uerID, Transform interactingObjectTransform)
	{
		if (!base.Interact(uerID, interactingObjectTransform))
			return false;

		if (spawnPoint == null)
		{
			spawnPoint = GameObject.Find("StageSpawn").GetComponent<Transform>();
		}
		SetEveryPlayerPosServerRPC(uerID);

        return true;
    }


    [ServerRpc(RequireOwnership = false)]
    public virtual void SetEveryPlayerPosServerRPC(ulong playerId)
    {
        Debug.Log(playerId);
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(playerId, out var client))
        {
            var playerObject = client.PlayerObject;

            if (spawnPoint == null)
            {
                spawnPoint = GameObject.Find("StageSpawn").GetComponent<Transform>();
            }

            playerObject.gameObject.GetComponent<CharacterController>().enabled = false;

            //위치이동
            playerObject.transform.position = spawnPoint.position;
            playerObject.gameObject.GetComponent<CharacterController>().enabled = true;

            MovePlayerClientRpc(playerObject.NetworkObjectId, spawnPoint.position);

            Debug.Log("Set PlayerPosition at " + spawnPoint.position + " .....");
        }
    }


    // 모든 클라이언트에게 이동을 반영하는 RPC
    [ClientRpc]
    public void MovePlayerClientRpc(ulong playerId, Vector3 targetPosition)
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
