using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

[CreateAssetMenu(fileName = "NewInventoryItem", menuName = "Inventory/InventoryItem")]
public class InventoryItem : ScriptableObject, INetworkSerializable
{
    public string itemName;
    public string itemSpritePath;  // ��������Ʈ ��� �߰�
    public string previewPrefabPath; // ������ ��� �߰�
    public string objectPrefabPath; // ������ ��� �߰�
    public string dropPrefabPath; // ������ ��� �߰�
    public bool isPlaceable;
    public bool isUsable;
    public int price;
    public int minPrice;
    public int maxPrice;
    public float batteryLevel;
    public float batteryEfficiency;
    [Header("���丮 ������. ������ ��� -1 �� �Է��ϰ� �Ⱦ� �����ۿ��� ����")]
    public bool isStoryItem;
    public int storyNumber;

    public int weight;

    private Sprite itemSprite; // ĳ�̵� ��������Ʈ
    private GameObject previewPrefab; // ĳ�̵� ������
    private GameObject objectPrefab; // ĳ�̵� ������
    private GameObject dropPrefab; // ĳ�̵� ������


    public Sprite ItemSprite
    {
        get
        {
            if (itemSprite == null && !string.IsNullOrEmpty(itemSpritePath))
            {
                itemSprite = Resources.Load<Sprite>(itemSpritePath);
                if (itemSprite != null)
                {
                    Debug.Log($"Preview Prefab loaded successfully from {itemSpritePath} ,  Name: {itemSprite.name}");
                }
                else
                {
                    Debug.LogError($"Failed to load Preview Prefab from {itemSpritePath}");
                }
            }
            return itemSprite;
        }
    }

    public GameObject PreviewPrefab
    {
        get
        {
            if (previewPrefab == null && !string.IsNullOrEmpty(previewPrefabPath))
            {
                previewPrefab = Resources.Load<GameObject>(previewPrefabPath);
                if (previewPrefab != null)
                {
                    Debug.Log($"Preview Prefab loaded successfully from {previewPrefabPath}");
                }
                else
                {
                    Debug.LogError($"Failed to load Preview Prefab from {previewPrefabPath}");
                }
            }
            return previewPrefab;
        }
    }

    public GameObject ObjectPrefab
    {
        get
        {
            if (objectPrefab == null && !string.IsNullOrEmpty(objectPrefabPath))
            {
                objectPrefab = Resources.Load<GameObject>(objectPrefabPath);
                if (objectPrefab != null)
                {
                    Debug.Log($"Object Prefab loaded successfully from {objectPrefabPath}");
                }
                else
                {
                    Debug.LogError($"Failed to load Object Prefab from {objectPrefabPath}");
                }
            }
            return objectPrefab;
        }
    }

    public GameObject DropPrefab
    {
        get
        {
            if (dropPrefab == null && !string.IsNullOrEmpty(dropPrefabPath))
            {
                dropPrefab = Resources.Load<GameObject>(dropPrefabPath);
                if (dropPrefab != null)
                {
                    Debug.Log($"Drop Prefab loaded successfully from {dropPrefabPath}");
                }
                else
                {
                    Debug.LogError($"Failed to load Drop Prefab from {dropPrefabPath}");
                }
            }
            return dropPrefab;
        }
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref itemName);
        serializer.SerializeValue(ref isPlaceable);
        serializer.SerializeValue(ref isUsable);
        serializer.SerializeValue(ref price);
        serializer.SerializeValue(ref maxPrice);
        serializer.SerializeValue(ref minPrice);
        serializer.SerializeValue(ref previewPrefabPath);
        serializer.SerializeValue(ref objectPrefabPath); 
        serializer.SerializeValue(ref dropPrefabPath); 
        serializer.SerializeValue(ref batteryLevel); 
        serializer.SerializeValue(ref batteryEfficiency); 
        serializer.SerializeValue(ref isStoryItem); 
        serializer.SerializeValue(ref storyNumber); 
    }

    public void CopyDataFrom(InventoryItem sourceItem)
    {
        itemName = sourceItem.itemName;
        itemSprite = sourceItem.itemSprite;
        isPlaceable = sourceItem.isPlaceable;
        isUsable = sourceItem.isUsable;
        price = sourceItem.price;
        maxPrice = sourceItem.maxPrice;
        minPrice = sourceItem.minPrice;
        previewPrefabPath = sourceItem.previewPrefabPath; // ������ ���� �߰�
        objectPrefabPath = sourceItem.objectPrefabPath;
        dropPrefabPath = sourceItem.dropPrefabPath; 
        batteryLevel = sourceItem.batteryLevel;
        batteryEfficiency = sourceItem.batteryEfficiency;
        isStoryItem = sourceItem.isStoryItem;
        storyNumber = sourceItem.storyNumber;
    }

    public InventoryItemData ToData()
    {
        return new InventoryItemData(
            new FixedString128Bytes(itemName),
            new FixedString128Bytes(itemSpritePath),
            new FixedString128Bytes(previewPrefabPath),
            new FixedString128Bytes(objectPrefabPath),
            new FixedString128Bytes(dropPrefabPath),
            isPlaceable,
            isUsable,
            price,
            maxPrice,
            minPrice,
            batteryLevel,
            batteryEfficiency,
            isStoryItem,
            storyNumber
        );
    }

    public void CopyDataFrom(InventoryItemData data)
    {
        itemName = data.itemName.ToString();
        itemSpritePath = data.itemSpritePath.ToString();
        previewPrefabPath = data.previewPrefabPath.ToString();
        objectPrefabPath = data.objectPrefabPath.ToString();
        dropPrefabPath = data.dropPrefabPath.ToString();
        isPlaceable = data.isPlaceable;
        isUsable = data.isUsable;
        price = data.price;
        maxPrice = data.maxPrice;
        minPrice = data.minPrice;
        batteryLevel = data.batteryLevel;
        batteryEfficiency = data.batteryEfficiency;
        isStoryItem = data.isStoryItem;
        storyNumber = data.storyNumber;
    }

}
