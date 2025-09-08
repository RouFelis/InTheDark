using Unity.Netcode;
using Unity.Collections;
using System;

[System.Serializable]
public struct InventoryItemData : INetworkSerializable, IEquatable<InventoryItemData>
{
    public FixedString128Bytes itemName;
    public FixedString128Bytes itemSpritePath;
    public FixedString128Bytes previewPrefabPath;
    public FixedString128Bytes objectPrefabPath;
    public FixedString128Bytes dropPrefabPath;
    public bool isPlaceable;
    public bool isUsable;
    public int price;
    public int minPrice;
    public int maxPrice;
    public float batteryLevel;
    public float batteryEfficiency;
    public bool isStoryItem;
    public int storyNumber;

    public InventoryItemData( FixedString128Bytes itemName, FixedString128Bytes itemSpritePath, FixedString128Bytes previewPrefabPath, FixedString128Bytes objectPrefabPath, FixedString128Bytes dropPrefabPath, bool isPlaceable , bool isUsable, int price , int maxprice, int minprice , float batterylevel , float batteryefficiency, bool isStoryItem, int storyNumber)
    {
        this.itemName = itemName;
        this.itemSpritePath = itemSpritePath;   
        this.previewPrefabPath = previewPrefabPath;
        this.objectPrefabPath = objectPrefabPath;
        this.dropPrefabPath = dropPrefabPath;
        this.isPlaceable = isPlaceable;
        this.isUsable = isUsable;
        this.price = price;
        this.maxPrice = maxprice;
        this.minPrice = minprice;
        this.batteryLevel = batterylevel;
        this.batteryEfficiency = batteryefficiency;
        this.isStoryItem = isStoryItem;
        this.storyNumber = storyNumber;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref itemName);
        serializer.SerializeValue(ref itemSpritePath);
        serializer.SerializeValue(ref previewPrefabPath);
        serializer.SerializeValue(ref objectPrefabPath);
        serializer.SerializeValue(ref dropPrefabPath);
        serializer.SerializeValue(ref isPlaceable);
        serializer.SerializeValue(ref isUsable);
        serializer.SerializeValue(ref price);
        serializer.SerializeValue(ref maxPrice);
        serializer.SerializeValue(ref minPrice);
        serializer.SerializeValue(ref batteryLevel);
        serializer.SerializeValue(ref batteryEfficiency);
        serializer.SerializeValue(ref isStoryItem);
        serializer.SerializeValue(ref storyNumber);
    }

    public bool Equals(InventoryItemData other)
    {
        return itemName.Equals(other.itemName) &&
               itemSpritePath.Equals(other.itemSpritePath) &&
               previewPrefabPath.Equals(other.previewPrefabPath) &&
               objectPrefabPath.Equals(other.objectPrefabPath) &&
               dropPrefabPath.Equals(other.dropPrefabPath) &&
               isPlaceable == other.isPlaceable &&
               isUsable == other.isUsable &&
               price == other.price &&
               maxPrice == other.maxPrice &&
               minPrice == other.minPrice &&
               batteryLevel == other.batteryLevel &&
               batteryEfficiency == other.batteryEfficiency &&
               isStoryItem == other.isStoryItem &&
               storyNumber == other.storyNumber;
    }

    public override bool Equals(object obj)
    {
        return obj is InventoryItemData other && Equals(other);
    }

    public override int GetHashCode()
    {
        int hash1 = HashCode.Combine(itemName, itemSpritePath, previewPrefabPath, objectPrefabPath, dropPrefabPath, isPlaceable, isUsable);
        int hash2 = HashCode.Combine(price, maxPrice, minPrice, batteryLevel, batteryEfficiency, isStoryItem, storyNumber);
        return HashCode.Combine(hash1, hash2);
    }
}
