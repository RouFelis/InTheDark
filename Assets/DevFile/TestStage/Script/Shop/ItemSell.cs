using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class ItemSell : InteractableObject
{
    public NetworkList<InventoryItemData> insertItemList; // 네트워크 동기화 인벤토리 아이템 리스트
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
        // 새로운 로그 텍스트 생성
        localizedString.TableReference = "UITable";
        localizedString.TableEntryReference = "Item";
        string newLog = $"{localizedString.GetLocalizedString()} : {itemName} / ";

        localizedString.TableReference = "UITable";
        localizedString.TableEntryReference = "Price";
        newLog += $"{localizedString.GetLocalizedString()} : {Value}";

        // 현재 로그 줄들 분리
        string[] lines = sellLogTMP.text.Split('\n');
        List<string> lineList = new List<string>(lines);

        // 최대 줄 수 초과 시 가장 윗줄 제거
        if (lineList.Count >= maxLines)
        {
            lineList.RemoveAt(0);  // 맨 위의 줄 삭제
        }

        // 새로운 로그 추가
        lineList.Add(newLog);

        // 다시 전체 로그 문자열 구성
        sellLogTMP.text = string.Join("\n", lineList);
    }



}
