using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Netcode;

public class ItemSell : InteractableObject
{
    public NetworkList<InventoryItemData> insertItemList; // ��Ʈ��ũ ����ȭ �κ��丮 ������ ����Ʈ
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

        // ���� �� �� ���
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
