using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PurchaseController : NetworkBehaviour 
{
    [System.Serializable]
    public struct PurchaseStruct 
    { 
        public Button purchaseButton;
        public TMP_Text tmp;
        public string objectPath;
        public int price;
    }

    public Transform[] SpawnPosition;
    public PurchaseStruct[] purchaseStruct;

    private NetworkObject spawnedObjectParent;

    private void Start()
	{
        StartCoroutine(InitManager());
        initButton();
    }
	private IEnumerator InitManager()
	{
        while (spawnedObjectParent == null)
        {
            try
            {
                spawnedObjectParent = GameObject.Find("SpawnedObjects").GetComponent<NetworkObject>();
            }
            catch
            {

            }

            yield return new WaitForSeconds(1f); // 1초마다 반복
        }
    }

    private void initButton()
	{
        foreach (var purchase in purchaseStruct)
        {
            purchase.tmp.text = purchase.price.ToString() + "TK";

            purchase.purchaseButton.onClick.AddListener(() => { PurchaseObjectServerRpc(purchase.objectPath, purchase.price); });
        }
    }


	[ServerRpc(RequireOwnership = false)] // 소유권이 필요하지 않도록 설정
    void PurchaseObjectServerRpc(NetworkString path, int price ,ServerRpcParams rpcParams = default)
    {
		if (price > SharedData.Instance.Money.Value)
		{
            return;
		}

        SharedData.Instance.Money.Value = SharedData.Instance.Money.Value - price;

        GameObject loadObject = Resources.Load<GameObject>(path);

        //랜덤 좌표(한점과 한점 사이에 랜덤으로 스폰되게 함. 최대한 다양하게 보이려고)
        float spawnX = Random.Range(SpawnPosition[0].position.x , SpawnPosition[1].position.x);
        float spawnZ = Random.Range(SpawnPosition[0].position.z , SpawnPosition[1].position.z);
        Vector3 spawnPosition = new Vector3(spawnX, SpawnPosition[0].position.y, spawnZ);       

        //랜덤 회전값. (오브젝트 스폰시 다양하게 보이려고)
        float randomX = Random.value > 0.5f ? 90f : 0f; 
        float randomZ = Random.value > 0.5f ? 90f : 0f; 
        float randomY = Random.Range(0f, 360f);
        Vector3 spawnRotation = new Vector3(randomX, randomY, randomZ);

        // 모든 클라이언트에서 오브젝트를 설치하는 ClientRpc 호출
        GameObject placedObject = Instantiate(loadObject, spawnPosition, Quaternion.Euler(spawnRotation));
        NetworkObject networkObject = placedObject.GetComponent<NetworkObject>();
        networkObject.Spawn();
        NetworkObject parentObject = NetworkManager.SpawnManager.SpawnedObjects[spawnedObjectParent.NetworkObjectId];


        PickupItem temptItem = placedObject.GetComponent<PickupItem>();

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
            temptItem.inventoryItem.batteryEfficiency
        );
        temptItem.networkInventoryItemData.Value = updatedItemData;

        networkObject.transform.SetParent(parentObject.transform, true);
    }

}
