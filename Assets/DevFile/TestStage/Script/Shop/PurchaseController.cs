using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class PurchaseController : NetworkBehaviour 
{
    [System.Serializable]
    public struct PurchaseStruct 
    { 
        public Button purchaseButton;
        public TMP_Text name;
        public TMP_Text explain;
        public string objectPath;
        public int price;
        public bool upgrade;
    }

    [Header("���� UI ����")]
    public Transform[] SpawnPosition;
    public PurchaseStruct[] purchaseStruct;

    [Header("�÷��̾� ���� �ޱ����� Shopinterecter")]
    public ShopInteracter shopInteracter;
    [SerializeField] private TMP_Text logTMP;
    public int maxLines = 10;

    private WeaponSystem weaponSystem;
    private NetworkObject spawnedObjectParent;

    public LocalizedString localizedString;

    private void Start()
    {
        StartCoroutine(InitManager());
        initButton();
        shopInteracter.SelectPlayerCode.OnValueChanged += (olbvalue, newvalue) => PlayerChange();
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

            yield return new WaitForSeconds(1f); // 1�ʸ��� �ݺ�
        }
    }

    public void PlayerChange()
    {
        weaponSystem = NetworkManager.Singleton.SpawnManager.SpawnedObjects[shopInteracter.SelectPlayerCode.Value].GetComponent<WeaponSystem>();
        foreach (var purchase in purchaseStruct)
        {
			if (purchase.upgrade)
            {
                purchase.purchaseButton.onClick.AddListener(() => { weaponSystem.UpgradeWeapon(); });
                purchase.name.text = purchase.price.ToString() + "��";
            }
        }
    }

    private void initButton()
	{
        foreach (var purchase in purchaseStruct)
        {
			if (!purchase.upgrade)
            {
                purchase.name.text = purchase.price.ToString() + "��";
                purchase.purchaseButton.onClick.AddListener(() => {
                    PurchaseObjectServerRpc(PlayersManager.Instance.playersList[0].Name, purchase.name.text, purchase.objectPath, purchase.price);                     
                });
            }
        }
    }


	[ServerRpc(RequireOwnership = false)] // �������� �ʿ����� �ʵ��� ����
    void PurchaseObjectServerRpc(NetworkString playerName, NetworkString itemName, NetworkString path, int price ,ServerRpcParams rpcParams = default)
    {
		if (price > SharedData.Instance.Money.Value)
		{
            return;
		}

        SharedData.Instance.Money.Value = SharedData.Instance.Money.Value - price;

        GameObject loadObject = Resources.Load<GameObject>(path);

        //���� ��ǥ(������ ���� ���̿� �������� �����ǰ� ��. �ִ��� �پ��ϰ� ���̷���)
        float spawnX = UnityEngine.Random.Range(SpawnPosition[0].position.x , SpawnPosition[1].position.x);
        float spawnZ = UnityEngine.Random.Range(SpawnPosition[0].position.z , SpawnPosition[1].position.z);
        Vector3 spawnPosition = new Vector3(spawnX, SpawnPosition[0].position.y, spawnZ);       

        //���� ȸ����. (������Ʈ ������ �پ��ϰ� ���̷���)
        float randomX = UnityEngine.Random.value > 0.5f ? 90f : 0f; 
        float randomZ = UnityEngine.Random.value > 0.5f ? 90f : 0f; 
        float randomY = UnityEngine.Random.Range(0f, 360f);
        Vector3 spawnRotation = new Vector3(randomX, randomY, randomZ);

        // ��� Ŭ���̾�Ʈ���� ������Ʈ�� ��ġ�ϴ� ClientRpc ȣ��
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
            temptItem.inventoryItem.batteryEfficiency,
            temptItem.inventoryItem.isStoryItem,
            temptItem.inventoryItem.storyNumber
        );
        temptItem.networkInventoryItemData.Value = updatedItemData;

        networkObject.transform.SetParent(parentObject.transform, true);
        PerchaseClientRpc(playerName, itemName);
    }

    [ClientRpc]
    void PerchaseClientRpc(string playerName, string itemName)
	{
        // ���ο� �α� �ؽ�Ʈ ����
        localizedString.TableReference = "UITable";
        localizedString.TableEntryReference = "Buyer";
        string newLog = $"{localizedString.GetLocalizedString()} : {playerName} / ";

        localizedString.TableReference = "UITable";
        localizedString.TableEntryReference = "Item";
        newLog += $"{localizedString.GetLocalizedString()} : {itemName}";

        // ���� �α� �ٵ� �и�
        string[] lines = logTMP.text.Split('\n');
        List<string> lineList = new List<string>(lines);

        // �ִ� �� �� �ʰ� �� ���� ���� ����
        if (lineList.Count >= maxLines)
        {
            lineList.RemoveAt(0);  // �� ���� �� ����
        }

        // ���ο� �α� �߰�
        lineList.Add(newLog);

        // �ٽ� ��ü �α� ���ڿ� ����
        logTMP.text = string.Join("\n", lineList);
    }
}