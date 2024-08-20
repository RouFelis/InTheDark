using UnityEngine;

[CreateAssetMenu(fileName = "NewInventoryItem", menuName = "Inventory/ItemList")]
public class ItemDataList : ScriptableObject
{
	public InventoryItem []inventoryItemList;
}
