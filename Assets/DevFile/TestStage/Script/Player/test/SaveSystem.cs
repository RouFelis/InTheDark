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
        
        // ������: ���� �� ����� ���� ����
        DeleteSaveFiles();
    }

	public void SaveGame()
    {
        SaveObject();
   //     playerSaveSystem.SavePlayerData();
    }

    public void LoadGame()
    {
        LoadObjects();
     //   playerSaveSystem.LoadPlayerData();
    }

    #region ���� ������Ʈ

    private void SaveObject()
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
        Debug.Log(Application.persistentDataPath + "/savefile.json �� ����" );
    }

	
	[ServerRpc] //�ҷ�����
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

                Debug.Log(Application.persistentDataPath + "/savefile.json �� �ҷ�����");
                //���� ����. �ٸ����� ���������������...
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
                     data.price, // ���⼭ ���ݸ� ����
                     changedObj.cloneItem.maxPrice,
                     changedObj.cloneItem.minPrice,
                     changedObj.cloneItem.batteryLevel,
                     changedObj.cloneItem.batteryEfficiency,
                     changedObj.cloneItem.isStoryItem,
                     changedObj.cloneItem.storyNumber
                    );

                    changedObj.networkInventoryItemData.Value = updatedItemData;

                    Debug.Log(Application.persistentDataPath + "/savefile.json �� �ҷ�����");
                }
                else
                {
                    Debug.LogWarning("Object not found: " + data.objectName);
                }
            }
        }
    }

	#endregion


	#region �������� ����
	public void SaveWeaponData(WeaponInstance weaponData, string playerName)
    {
        string json = JsonUtility.ToJson(weaponData, true); // JSON ���ڿ��� ��ȯ

        var filePath = Path.Combine(Application.persistentDataPath, playerName + ".json");

        File.WriteAllText(filePath, json); // JSON ���� ����
        Debug.Log($"���� �����Ͱ� ����Ǿ����ϴ�: {filePath}");
    }

    public WeaponInstance LoadWeaponData(string playerName)
    {
        var filePath = Path.Combine(Application.persistentDataPath, playerName + ".json");
        if (!File.Exists(filePath))
        {
            Debug.LogWarning("���� ������ ������ �������� �ʽ��ϴ�. �⺻ �����͸� �����մϴ�.");
            return new WeaponInstance(CreateDefaultWeaponData()) ; // �⺻ ������ ��ȯ
        }
        string json = File.ReadAllText(filePath); // JSON ���� �б�
        WeaponInstance weaponData = JsonUtility.FromJson<WeaponInstance>(json); // JSON ���ڿ��� ��ü�� ��ȯ
        Debug.Log(playerName + "�� ���� �����Ͱ� �ε�Ǿ����ϴ�... path : " + filePath);
        return weaponData;
    }
    #endregion

    #region ���丮 ���� ����

    /// <summary>
    /// ����Ʈ(���丮) ���� ��Ȳ�� JSON ���Ϸ� �����մϴ�.
    /// </summary>
    /// <param name="unlockedStoryIDs">������ ���丮 ID ����Ʈ</param>
    public void SaveStoryData(List<int> unlockedStoryIDs)
    {
        // 1. �����͸� ���� Ŭ������ ����ϴ�.
        QuestDataWrapper dataWrapper = new QuestDataWrapper { unlockedStoryIDs = unlockedStoryIDs };

        // 2. JSON ���ڿ��� ��ȯ�մϴ�.
        string json = JsonUtility.ToJson(dataWrapper, true);

        // 3. ��ȣȭ�� ����Ѵٸ� ��ȣȭ�� �����մϴ�.
        if (useEncryption)
        {
            json = EncryptDecrypt(json);
        }

        // 4. ���Ͽ� �����մϴ�.
        string path = Path.Combine(Application.persistentDataPath, "storydata.json");
        File.WriteAllText(path, json);
        Debug.Log($"���丮 �����Ͱ� ����Ǿ����ϴ�: {path}");
    }

    /// <summary>
    /// ���Ͽ��� ����Ʈ(���丮) ���� ��Ȳ�� �ҷ��ɴϴ�.
    /// </summary>
    /// <returns>�ҷ��� ���丮 ID ����Ʈ</returns>
    public List<int> LoadStoryData()
    {
        string path = Path.Combine(Application.persistentDataPath, "storydata.json");

        // ���� ������ �����ϸ� �����͸� �о�ɴϴ�.
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);

            // ��ȣȭ�� ���Ǿ��ٸ� ��ȣȭ�մϴ�.
            if (useEncryption)
            {
                json = EncryptDecrypt(json);
            }

            QuestDataWrapper dataWrapper = JsonUtility.FromJson<QuestDataWrapper>(json);
            Debug.Log($"���丮 �����͸� �ҷ��Խ��ϴ�: {path}");
            return dataWrapper.unlockedStoryIDs;
        }
        else
        {
            // ���� ������ ������ �� ����Ʈ�� ��ȯ�մϴ�.
            Debug.LogWarning("���丮 ������ ������ �������� �ʽ��ϴ�. ���ο� �����͸� �����մϴ�.");
            return new List<int>();
        }
    }

    // JSON ����ȭ�� ���� ����Ʈ ������ ���� Ŭ����
    [System.Serializable]
    private class QuestDataWrapper
    {
        public List<int> unlockedStoryIDs;
    }

    #endregion

    /// <summary>
    /// ������: ����� JSON ���ϵ� ����
    /// </summary>
    private void DeleteSaveFiles()
    {
        string[] targetFiles =
        {
        Path.Combine(Application.persistentDataPath, "savefile.json"),
        Path.Combine(Application.persistentDataPath, "storydata.json"),
        Path.Combine(Application.persistentDataPath, "playerdata.json")
        // ���� �����ʹ� �÷��̾� �̸� ����̴� �ʿ� �� ���� �����ϰų� ��ü ���� ��ĵ ����
    };

        foreach (var file in targetFiles)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
                Debug.Log($"������ ���� �Ϸ�: {file}");
            }
        }

        // ���� ��� JSON ������ �� ����� �ʹٸ�:
        /*
        string[] allFiles = Directory.GetFiles(Application.persistentDataPath, "*.json");
        foreach (var file in allFiles)
        {
            File.Delete(file);
            Debug.Log($"������: {file}");
        }
        */
    }

    private WeaponData CreateDefaultWeaponData()
    {
        return ScriptableObject.CreateInstance<WeaponData>();
    }

    public void SavePlayerData(Player player)
    {
        Debug.Log(2);
        PlayerData playerData = new PlayerData
        {
            playerName = player.Name,
            experience = player.Experience,
            level = player.Level
        };

        string json = JsonUtility.ToJson(playerData, true);

        if (useEncryption)
        {
            json = EncryptDecrypt(json);
        }

        File.WriteAllText(Application.persistentDataPath + "/playerdata.json", json);

        Debug.Log("���� : " + Application.persistentDataPath + "/playerdata.json");
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
