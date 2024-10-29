using UnityEngine;
using Unity.Netcode;

public class ItemSell : InteractableObject
{
    public NetworkList<InventoryItemData> insertItemList; // 네트워크 동기화 인벤토리 아이템 리스트

    private void Awake()
    {
        insertItemList = new NetworkList<InventoryItemData>();
    }

    public override void Interact(Transform interactingObjectTransform)
    {
        NetworkInventoryController controller = interactingObjectTransform.GetComponent<NetworkInventoryController>();
        int sellPrice = controller.HandleSellItem();

        SharedData.Instance.AddMoneyServerRpc(sellPrice);

        base.Interact(interactingObjectTransform);
    }
}
