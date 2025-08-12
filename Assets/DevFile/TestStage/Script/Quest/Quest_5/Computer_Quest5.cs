using UnityEngine;
using Unity.Netcode;

public class Computer_Quest5 : InteractableObject_NonNet
{
    [SerializeField] private Quest5 quest5;
    [SerializeField] private BoxCollider boxCollider;

    public override void Interact(ulong userId, Transform interactingObjectTransform)
    {
        Debug.Log($"테스트 : {userId}");

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(userId, out var networkClient))
        {
            NetworkObject playerObject = networkClient.PlayerObject;

            if (playerObject != null)
            {
                var inventory =  playerObject.GetComponent<NetworkInventoryController>();
                if (inventory.GetSelectedItemName() == "USB")
				{
                    inventory.HandleEraseItem();
                    this.gameObject.layer = 0;
                    quest5.StartQuestClientRpc();
                }
            }
            else
            {
                Debug.LogWarning("플레이어 오브젝트가 null입니다.");
            }
        }
        else
        {
            Debug.LogWarning($"해당 ClientId({userId})에 대한 ConnectedClient를 찾을 수 없습니다.");
        }

        base.Interact(userId, interactingObjectTransform);
    }


}
