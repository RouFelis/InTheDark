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
            // Ŭ���̾�Ʈ���� �����Ͱ� ����� �� ������ �ε�
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
