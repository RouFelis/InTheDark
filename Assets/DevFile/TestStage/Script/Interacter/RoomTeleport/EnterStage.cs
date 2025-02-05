using UnityEngine;
using Unity.Netcode;

public class EnterStage : InteractableObject
{   
    [SerializeField] Transform spawnPoint;
    [SerializeField] AudioSource otherSideDoorSource;

    public override void Interact(ulong uerID, Transform interactingObjectTransform)
    {
		if (spawnPoint == null)
		{
            spawnPoint = GameObject.Find("StageSpawn").GetComponent<Transform>();            
        }
        PlayDoorSound();
        SetEveryPlayerPosServerRPC(uerID);

		// 2024.12.24 ���� ���� �̺�Ʈ �߰�
		// 2024.12.26 ���� ���� �̺�Ʈ ���ġ
		//InTheDark.Prototypes.Game.OnDungeonEnter.Invoke(new InTheDark.Prototypes.DungeonEnterEvent()
		//{
		//    BuildIndex = 0
		//});
	}


	private void PlayDoorSound()
	{
        if (otherSideDoorSource == null)
        {
            otherSideDoorSource = GameObject.Find("OutDoor_1_SoundSource").GetComponent<AudioSource>();
            Debug.Log("�׽�Ʈ");
        }
        otherSideDoorSource.Play();
        Debug.Log("�׽�Ʈ2");
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
                spawnPoint = GameObject.Find("StageSpawn").GetComponent<Transform>();
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
