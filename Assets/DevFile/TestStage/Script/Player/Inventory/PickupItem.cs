using UnityEngine;
using Unity.Netcode;

public class PickupItem : NetworkBehaviour
{
    public InventoryItem inventoryItem;

    public InventoryItem CloneItem;

    public InventoryItem cloneItem 
    {
        get
        {
            if (CloneItem == null)
            {
                CloneItem = Instantiate(inventoryItem);
            }
            return CloneItem;
        }
        set 
        {
            CloneItem = value;
        }
    }

    public NetworkVariable<InventoryItemData> networkInventoryItemData = new NetworkVariable<InventoryItemData>();

	private void Start()
    {
        if (IsServer)
        {          
            // 클라이언트에서 데이터가 변경될 때 아이템 로드
            networkInventoryItemData.OnValueChanged += (oldValue, newValue) =>
            {
                LoadItemFromData(newValue);              
            };
        }
        else
        {
            networkInventoryItemData.OnValueChanged += (oldValue, newValue) =>
            {
                LoadItemFromData(newValue);
            };
        }
    }

    private void LoadItemFromData(InventoryItemData data)
    {
        cloneItem.CopyDataFrom(data);
    }
}
