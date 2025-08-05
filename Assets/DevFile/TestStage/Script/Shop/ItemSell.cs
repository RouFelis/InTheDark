using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class ItemSell : InteractableObject
{
    public NetworkList<InventoryItemData> insertItemList; // ��Ʈ��ũ ����ȭ �κ��丮 ������ ����Ʈ
    [SerializeField] private TMP_Text sellLogTMP;
    public int maxLines = 10;


    private void Awake()
    {
        insertItemList = new NetworkList<InventoryItemData>();
    }

    public override bool Interact(ulong uerID , Transform interactingObjectTransform)
    {
		if (!base.Interact(uerID, interactingObjectTransform))
			return false;

        NetworkInventoryController controller = interactingObjectTransform.GetComponent<NetworkInventoryController>();


        string sellItemName = controller.GetSelectedItemName();
        int sellPrice = controller.HandleSellItem();

        if (sellPrice < 0)
        {
            return false;
        }


        SetSellTMPServerRpc(sellPrice, sellItemName);
    
        SharedData.Instance.AddMoneyServerRpc(sellPrice);

        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetSellTMPServerRpc(int Value, string itemName)
	{
        SetSellTMPClientRpc(Value, itemName);
    }

    [ClientRpc]
    private void SetSellTMPClientRpc(int Value , string itemName)
    {
        // ���ο� �α� �ؽ�Ʈ ����
        localizedString.TableReference = "UITable";
        localizedString.TableEntryReference = "Item";
        string newLog = $"{localizedString.GetLocalizedString()} : {itemName} / ";

        localizedString.TableReference = "UITable";
        localizedString.TableEntryReference = "Price";
        newLog += $"{localizedString.GetLocalizedString()} : {Value}";

        // ���� �α� �ٵ� �и�
        string[] lines = sellLogTMP.text.Split('\n');
        List<string> lineList = new List<string>(lines);

        // �ִ� �� �� �ʰ� �� ���� ���� ����
        if (lineList.Count >= maxLines)
        {
            lineList.RemoveAt(0);  // �� ���� �� ����
        }

        // ���ο� �α� �߰�
        lineList.Add(newLog);

        // �ٽ� ��ü �α� ���ڿ� ����
        sellLogTMP.text = string.Join("\n", lineList);
    }



}
