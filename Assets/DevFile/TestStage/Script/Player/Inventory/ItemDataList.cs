using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


[CreateAssetMenu(fileName = "NewInventoryItem", menuName = "Inventory/ItemList")]
public class ItemDataList : ScriptableObject
{
	public InventoryItem []inventoryItemList;
}
