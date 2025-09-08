using UnityEngine;
using Unity.Netcode;

public class PickupItem : NetworkBehaviour , IPickupItem
{
    public InventoryItem inventoryItem;

	[SerializeField] private InventoryItem CloneItem;

    [Header("���丮 �ѹ� �ĺ�")]
    [SerializeField] private int[] storyNumbers;

    /*
        [Header("Debug Info (Runtime Only)")]
        [SerializeField] private string itemName;
        [SerializeField] private string itemSpritePath;
        [SerializeField] private string previewPrefabPath;
        [SerializeField] private string objectPrefabPath;
        [SerializeField] private string dropPrefabPath;

        [SerializeField] private bool isPlaceable;
        [SerializeField] private bool isUsable;

        [SerializeField] private int price;
        [SerializeField] private int minPrice;
        [SerializeField] private int maxPrice;

        [SerializeField] private float batteryLevel;
        [SerializeField] private float batteryEfficiency;


        private void Update()
        {
            // ���� ���� ���� ������ �ν����� ������Ʈ
            if (!IsOwner && !IsServer) return;

            var data = networkInventoryItemData.Value;

            itemName = data.itemName.ToString();
            itemSpritePath = data.itemSpritePath.ToString();
            previewPrefabPath = data.previewPrefabPath.ToString();
            objectPrefabPath = data.objectPrefabPath.ToString();
            dropPrefabPath = data.dropPrefabPath.ToString();

            isPlaceable = data.isPlaceable;
            isUsable = data.isUsable;

            price = data.price;
            minPrice = data.minPrice;
            maxPrice = data.maxPrice;

            batteryLevel = data.batteryLevel;
            batteryEfficiency = data.batteryEfficiency;
        }
    */

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

    public bool RequestBoolStoryItem()
    {
        return cloneItem.isStoryItem;
    }

    public int RequestStoryNum()
    {
        return cloneItem.storyNumber;
    }

    protected virtual void Start()
    {
        if (IsServer)
        {
            SetStroyNumber();

            // Ŭ���̾�Ʈ���� �����Ͱ� ����� �� ������ �ε�
            networkInventoryItemData.OnValueChanged += (oldValue, newValue) =>
            {
                LoadItemFromData(newValue);
            };
        }
        else
        {
            cloneItem.CopyDataFrom(networkInventoryItemData.Value);

            networkInventoryItemData.OnValueChanged += (oldValue, newValue) =>
            {
                LoadItemFromData(newValue);
            };
        }
    }

    private void SetStroyNumber()
    {
        if (!cloneItem.isStoryItem && cloneItem.storyNumber != -1) return;

        // ���� �� �������ٸ� �������� ����
        if (storyNumbers.Length > 0)
        {
            cloneItem.storyNumber = storyNumbers[Random.Range(0, storyNumbers.Length)]; 
            networkInventoryItemData.Value = cloneItem.ToData();
        }
    }

    private void LoadItemFromData(InventoryItemData data)
    {
        Debug.Log("�׽�Ʈ 33333333");
        cloneItem.CopyDataFrom(data);
        Debug.Log("DataLoad....");
    }

    public virtual void UseItem(NetworkInventoryController controller)
	{
        Debug.Log($"UseItem �׽�Ʈ");

        //������ �����ϱ�.
        controller.RequestRemoveItemFromInventoryServerRpc(controller.selectedSlot.Value);
    }
}
