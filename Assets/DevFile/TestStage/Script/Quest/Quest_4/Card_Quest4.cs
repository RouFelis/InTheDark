using UnityEngine;
using Unity.Netcode;

public class Card_Quest4 : InteractableObject_NonNet
{
    [SerializeField] private Quest4 quest4;

    public override void Interact(ulong userId, Transform interactingObjectTransform)
    {
        Debug.Log($"테스트 : {userId}");

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(userId, out var networkClient))
        {
            NetworkObject playerObject = networkClient.PlayerObject;

            if (playerObject != null)
            {
                var inventory = playerObject.GetComponent<NetworkInventoryController>();
                if (inventory.GetSelectedItemName() == "Level A Card")
                {
                    inventory.HandleEraseItem();
                    quest4.CardPassServerRpc();
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
