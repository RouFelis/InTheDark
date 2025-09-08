using Unity.Netcode;
using UnityEngine;
using static UnityEditor.U2D.ScriptablePacker;

[CreateAssetMenu]
public class RewardItemTrigger : EnemyRandomBoxTrigger
{
	[SerializeField]
	private string _itemPath;

	[SerializeField]
	private Vector3 _offset;

	public override void OnUpdate(EnemyPrototypePawn pawn)
	{
		var audioSource = pawn.GetComponent<AudioSource>();
		var spawnedObjectParent = GameObject.Find("SpawnedObjects").GetComponent<NetworkObject>();

		var loadObject = Resources.Load<GameObject>(_itemPath);

		var spawnPosition = Random.insideUnitSphere + _offset + pawn.transform.position;

		//랜덤 회전값. (오브젝트 스폰시 다양하게 보이려고)
		var randomX = Random.value > 0.5f ? 90f : 0f;
		var randomZ = Random.value > 0.5f ? 90f : 0f;
		var randomY = Random.Range(0f, 360f);

		var spawnRotation = new Vector3(randomX, randomY, randomZ);

		// 모든 클라이언트에서 오브젝트를 설치하는 ClientRpc 호출
		var placedObject = Instantiate(loadObject, spawnPosition, Quaternion.Euler(spawnRotation));

		var networkObject = placedObject.GetComponent<NetworkObject>();
		var temptItem = placedObject.GetComponent<PickupItem>();

		var parentObject = pawn.NetworkManager.SpawnManager.SpawnedObjects[spawnedObjectParent.NetworkObjectId];

		var updatedItemData = new InventoryItemData(
			temptItem.inventoryItem.itemName,
			temptItem.inventoryItem.itemSpritePath,
			temptItem.inventoryItem.previewPrefabPath,
			temptItem.inventoryItem.objectPrefabPath,
			temptItem.inventoryItem.dropPrefabPath,
			temptItem.inventoryItem.isPlaceable,
			temptItem.inventoryItem.isUsable,
			0,
			temptItem.inventoryItem.maxPrice,
			temptItem.inventoryItem.minPrice,
			temptItem.inventoryItem.batteryLevel,
			temptItem.inventoryItem.batteryEfficiency,
			temptItem.inventoryItem.isStoryItem,
			temptItem.inventoryItem.storyNumber
		);

		networkObject.Spawn();

		temptItem.networkInventoryItemData.Value = updatedItemData;

		networkObject.transform.SetParent(parentObject.transform, true);
	}
}
