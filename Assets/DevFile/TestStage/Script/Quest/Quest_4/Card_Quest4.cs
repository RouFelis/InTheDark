using UnityEngine;
using Unity.Netcode;

public class Card_Quest4 : InteractableObject_NonNet
{
    [SerializeField] private Quest4 quest4;

    public override void Interact(ulong userId, Transform interactingObjectTransform)
    {
        Debug.Log($"�׽�Ʈ : {userId}");

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
                Debug.LogWarning("�÷��̾� ������Ʈ�� null�Դϴ�.");
            }
        }
        else
        {
            Debug.LogWarning($"�ش� ClientId({userId})�� ���� ConnectedClient�� ã�� �� �����ϴ�.");
        }

        base.Interact(userId, interactingObjectTransform);
    }

}
