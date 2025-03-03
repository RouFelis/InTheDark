using UnityEngine;
using Unity.Netcode;

public class PickupItem : NetworkBehaviour , IPickupItem
{
    public InventoryItem inventoryItem;

   [SerializeField] private InventoryItem CloneItem;

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
            networkInventoryItemData.Value = cloneItem.ToData();
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

    void Update()
    {
		if (cloneItem.isPlaceable)
		{
            cloneItem.batteryLevel -= cloneItem.batteryEfficiency * Time.deltaTime;
        }       
    }

    public virtual void UseItem()
	{
        Debug.Log($"UseItem �׽�Ʈ");
	}
}
