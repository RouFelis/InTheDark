using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.Netcode;

public class SaveSystem : MonoBehaviour
{
    [SerializeField] GameObject SaveObjects;

    public bool useEncryption = true;

    private readonly string encryptionCodeWord = "fortheworld";

    private void Start()
	{
        SaveObjects = GameObject.Find("SpawnedObjects");
        //SaveGame();
    }

	public void SaveGame()
    {
        Save();
   //     playerSaveSystem.SavePlayerData();
    }

    public void LoadGame()
    {
        LoadObjects();
     //   playerSaveSystem.LoadPlayerData();
    }

    private void Save()
    {
        PickupItem[] saveObjectList = SaveObjects.GetComponentsInChildren<PickupItem>();

        List<SaveData> saveDataList = new List<SaveData>();

        foreach (PickupItem obj in saveObjectList)
        {
            SaveData data = new SaveData
            {
                objectName = obj.inventoryItem.itemName,
                position = obj.transform.position,
                rotation = obj.transform.rotation,
                price = obj.inventoryItem.price
            };
            saveDataList.Add(data);
        }

        string json = JsonUtility.ToJson(new SaveDataListWrapper { saveDataList = saveDataList }, true);

		if (useEncryption)
		{
            json = EncryptDecrypt(json);
        }
 

        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
        Debug.Log(Application.persistentDataPath + "/savefile.json 에 저장" );
    }

    [ServerRpc] //불러오기
    private void LoadObjects()
    {
        string path = Application.persistentDataPath + "/savefile.json";
        Debug.Log(path);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);

            if (useEncryption)
            {
                json = EncryptDecrypt(json);
            }

            SaveDataListWrapper dataWrapper = JsonUtility.FromJson<SaveDataListWrapper>(json);
            List<SaveData> saveDataList = dataWrapper.saveDataList;

            foreach (SaveData data in saveDataList)
            {                
                GameObject prefab = Resources.Load<GameObject>("Prefab/" + data.objectName);
                GameObject obj = Instantiate(prefab, data.position, data.rotation, SaveObjects.transform);
                NetworkObject netobj = obj.GetComponent<NetworkObject>();
                netobj.Spawn();

                NetworkObject parentObject = SaveObjects.GetComponent<NetworkObject>();
                netobj.transform.SetParent(parentObject.transform, true);

                Debug.Log(Application.persistentDataPath + "/savefile.json 에 불러오기");
                //가격 설정. 다른값도 설정해줘야할지도... 아 복잡티비
                if (obj != null)
                {
                    PickupItem changedObj = obj.GetComponent<PickupItem>();
                    var updatedItemData = new InventoryItemData(
                     changedObj.cloneItem.itemName,
                     changedObj.cloneItem.itemSpritePath,
                     changedObj.cloneItem.previewPrefabPath,
                     changedObj.cloneItem.objectPrefabPath,
                     changedObj.cloneItem.dropPrefabPath,
                     changedObj.cloneItem.isPlaceable,
                     changedObj.cloneItem.isUsable,
                     data.price, // 여기서 가격만 변경
                     changedObj.cloneItem.maxPrice,
                     changedObj.cloneItem.minPrice,
                     changedObj.cloneItem.batteryLevel,
                     changedObj.cloneItem.batteryEfficiency
                    );

                    changedObj.networkInventoryItemData.Value = updatedItemData;

                    Debug.Log(Application.persistentDataPath + "/savefile.json 에 불러오기");
                }
                else
                {
                    Debug.LogWarning("Object not found: " + data.objectName);
                }
            }
        }
    }

    public void SavePlayerData(Player player)
    {
        Debug.Log(2);
        PlayerData playerData = new PlayerData
        {
            playerName = player.playerName.Value.ToString(),
            experience = player.experience.Value,
            level = player.level.Value
        };

        string json = JsonUtility.ToJson(playerData, true);

        if (useEncryption)
        {
            json = EncryptDecrypt(json);
        }

        File.WriteAllText(Application.persistentDataPath + "/playerdata.json", json);

        Debug.Log("저장 : " + Application.persistentDataPath + "/playerdata.json");
    }

    public string EncryptDecrypt(string data)
	{
        string modifiedData = "";
        for (int i =0; i < data.Length ; i++)
		{
            modifiedData += (char)(data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]);
        }
        return modifiedData;
    }


    [System.Serializable]
    private class SaveDataListWrapper
    {
        public List<SaveData> saveDataList;
    }
}
