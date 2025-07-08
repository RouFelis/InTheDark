using UnityEngine;
using DunGen;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;

public class ItemSpawnManager : MonoBehaviour , IDungeonCompleteReceiver
{
	public GameObject[] itemPrefabs;     // ������ ������ �����յ�
	public int itemCountToSpawn = 10;    // ������ �� ������ ��


    public List<Transform> spawnTransforms = new List<Transform>();

    public void OnDungeonComplete(Dungeon dungeon)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            StartCoroutine(SpawnItems());
        }
    }

    public IEnumerator SpawnItems()
    {
        yield return new WaitForEndOfFrame();
        SpanwItemServerRpc();
    }


    [ServerRpc]
    private void SpanwItemServerRpc()
	{
        spawnTransforms.Clear();
        // 1. "ItemSpawnPos" �̸��� ���� ��� ������Ʈ ã��
        ItemSpawnMarker[] spawnPoints = FindObjectsByType<ItemSpawnMarker>(FindObjectsSortMode.InstanceID);


       
        foreach (var point in spawnPoints)
        {
            spawnTransforms.Add(point.transform);
        }

        // 2. ���� ������ ��ġ���� ������ ������ ������ �ִ� ������ ��ŭ��
        int spawnCount = Mathf.Min(itemCountToSpawn, spawnTransforms.Count);

        // 3. ���� ��ġ�� �����ϰ� ����
        ShuffleList(spawnTransforms);

		for (int i = 0; i < spawnCount; i++)
		{
			Transform spawnPoint = spawnTransforms[i];
			GameObject randomPrefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
			GameObject spawnedObject = Instantiate(randomPrefab, spawnPoint.position, Quaternion.identity);

			var itemToDrop = spawnedObject.GetComponent<PickupItem>();


            int minPrice = itemToDrop.inventoryItem.minPrice;
            int maxPrice = itemToDrop.inventoryItem.maxPrice;

			int randomPrice = Random.Range(minPrice, maxPrice + 1);

            spawnedObject.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);

			var updatedItemData = new InventoryItemData(
				  itemToDrop.inventoryItem.itemName,
				  itemToDrop.inventoryItem.itemSpritePath,
				  itemToDrop.inventoryItem.previewPrefabPath,
				  itemToDrop.inventoryItem.objectPrefabPath,
				  itemToDrop.inventoryItem.dropPrefabPath,
				  itemToDrop.inventoryItem.isPlaceable,
				  itemToDrop.inventoryItem.isUsable,
                  randomPrice,
                  itemToDrop.inventoryItem.maxPrice,
				  itemToDrop.inventoryItem.minPrice,
				  itemToDrop.inventoryItem.batteryLevel,
				  itemToDrop.inventoryItem.batteryEfficiency
			  );



            StartCoroutine(SetDataNextFrame(itemToDrop, updatedItemData));
        }

        Debug.Log("ItemSpawn Complete...");
    }

    private IEnumerator SetDataNextFrame(PickupItem item, InventoryItemData data)
    {
        yield return null; // 1 frame delay
		item.networkInventoryItemData.Value = data;
    }

    // ����Ʈ ���� �Լ�
    public void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
