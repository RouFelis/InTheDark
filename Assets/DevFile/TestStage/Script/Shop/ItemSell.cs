using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Netcode;

public class ItemSell : InteractableObject
{
    public NetworkList<InventoryItemData> insertItemList; // 네트워크 동기화 인벤토리 아이템 리스트
    [SerializeField] private TMP_Text sellLogTMP;


    private void Awake()
    {
        insertItemList = new NetworkList<InventoryItemData>();
    }

    public override void Interact(ulong uerID , Transform interactingObjectTransform)
    {
        NetworkInventoryController controller = interactingObjectTransform.GetComponent<NetworkInventoryController>();

        int sellPrice = controller.HandleSellItem();

        SharedData.Instance.AddMoneyServerRpc(sellPrice);
        SetSellTMPServerRpc(sellPrice);

        base.Interact(uerID ,interactingObjectTransform);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetSellTMPServerRpc(int Value)
	{
        SetSellTMPClientRpc(Value);
    }

    [ClientRpc]
    private void SetSellTMPClientRpc(int Value)
    {
        string newLog = "\nSold it for " + Value.ToString() + ".";

        // 현재 줄 수 계산
        string[] lines = sellLogTMP.text.Split('\n');

        if (lines.Length >= 5)
        {
            sellLogTMP.text = newLog;
        }
        else
        {
            sellLogTMP.text += $"\n{newLog}";
        }
    }



}
