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

    public InventoryItemData(FixedString128Bytes itemSpritePath, FixedString128Bytes itemName, FixedString128Bytes previewPrefabPath, FixedString128Bytes objectPrefabPath, FixedString128Bytes dropPrefabPath, bool isPlaceable)
    {
        this.itemName = itemName;
        this.itemSpritePath = itemSpritePath;
        this.previewPrefabPath = previewPrefabPath;
        this.objectPrefabPath = objectPrefabPath;
        this.dropPrefabPath = dropPrefabPath;
        this.isPlaceable = isPlaceable;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref itemName);
        serializer.SerializeValue(ref itemSpritePath);
        serializer.SerializeValue(ref previewPrefabPath);
        serializer.SerializeValue(ref objectPrefabPath);
        serializer.SerializeValue(ref dropPrefabPath);
        serializer.SerializeValue(ref isPlaceable);
    }

    public bool Equals(InventoryItemData other)
    {
        return itemName.Equals(other.itemName) &&
               itemSpritePath.Equals(other.itemSpritePath) &&
               previewPrefabPath.Equals(other.previewPrefabPath) &&
               objectPrefabPath.Equals(other.objectPrefabPath) &&
               dropPrefabPath.Equals(other.dropPrefabPath) &&
               isPlaceable == other.isPlaceable;
    }

    public override bool Equals(object obj)
    {
        return obj is InventoryItemData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(itemName, itemSpritePath, previewPrefabPath, objectPrefabPath, dropPrefabPath, isPlaceable);
    }
}
