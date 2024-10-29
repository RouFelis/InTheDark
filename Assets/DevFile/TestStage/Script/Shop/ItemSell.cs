using UnityEngine;
using Unity.Netcode;

public class ItemSell : InteractableObject
{
    public NetworkList<InventoryItemData> insertItemList; // ��Ʈ��ũ ����ȭ �κ��丮 ������ ����Ʈ

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
