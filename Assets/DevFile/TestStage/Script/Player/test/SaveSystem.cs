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
        
        // 디버깅용: 시작 시 저장된 파일 삭제
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

    #region 저장 오브젝트

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
                //가격 설정. 다른값도 설정해줘야할지도...
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
                     changedObj.cloneItem.batteryEfficiency,
                     changedObj.cloneItem.isStoryItem,
                     changedObj.cloneItem.storyNumber
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

	#endregion


	#region 무기정보 저장
	public void SaveWeaponData(WeaponInstance weaponData, string playerName)
    {
        string json = JsonUtility.ToJson(weaponData, true); // JSON 문자열로 변환

        var filePath = Path.Combine(Application.persistentDataPath, playerName + ".json");

        File.WriteAllText(filePath, json); // JSON 파일 저장
        Debug.Log($"무기 데이터가 저장되었습니다: {filePath}");
    }

    public WeaponInstance LoadWeaponData(string playerName)
    {
        var filePath = Path.Combine(Application.persistentDataPath, playerName + ".json");
        if (!File.Exists(filePath))
        {
            Debug.LogWarning("무기 데이터 파일이 존재하지 않습니다. 기본 데이터를 생성합니다.");
            return new WeaponInstance(CreateDefaultWeaponData()) ; // 기본 데이터 반환
        }
        string json = File.ReadAllText(filePath); // JSON 파일 읽기
        WeaponInstance weaponData = JsonUtility.FromJson<WeaponInstance>(json); // JSON 문자열을 객체로 변환
        Debug.Log(playerName + "의 무기 데이터가 로드되었습니다... path : " + filePath);
        return weaponData;
    }
    #endregion

    #region 스토리 정보 저장

    /// <summary>
    /// 퀘스트(스토리) 진행 상황을 JSON 파일로 저장합니다.
    /// </summary>
    /// <param name="unlockedStoryIDs">저장할 스토리 ID 리스트</param>
    public void SaveStoryData(List<int> unlockedStoryIDs)
    {
        // 1. 데이터를 래퍼 클래스에 담습니다.
        QuestDataWrapper dataWrapper = new QuestDataWrapper { unlockedStoryIDs = unlockedStoryIDs };

        // 2. JSON 문자열로 변환합니다.
        string json = JsonUtility.ToJson(dataWrapper, true);

        // 3. 암호화를 사용한다면 암호화를 적용합니다.
        if (useEncryption)
        {
            json = EncryptDecrypt(json);
        }

        // 4. 파일에 저장합니다.
        string path = Path.Combine(Application.persistentDataPath, "storydata.json");
        File.WriteAllText(path, json);
        Debug.Log($"스토리 데이터가 저장되었습니다: {path}");
    }

    /// <summary>
    /// 파일에서 퀘스트(스토리) 진행 상황을 불러옵니다.
    /// </summary>
    /// <returns>불러온 스토리 ID 리스트</returns>
    public List<int> LoadStoryData()
    {
        string path = Path.Combine(Application.persistentDataPath, "storydata.json");

        // 저장 파일이 존재하면 데이터를 읽어옵니다.
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);

            // 암호화가 사용되었다면 복호화합니다.
            if (useEncryption)
            {
                json = EncryptDecrypt(json);
            }

            QuestDataWrapper dataWrapper = JsonUtility.FromJson<QuestDataWrapper>(json);
            Debug.Log($"스토리 데이터를 불러왔습니다: {path}");
            return dataWrapper.unlockedStoryIDs;
        }
        else
        {
            // 저장 파일이 없으면 빈 리스트를 반환합니다.
            Debug.LogWarning("스토리 데이터 파일이 존재하지 않습니다. 새로운 데이터를 생성합니다.");
            return new List<int>();
        }
    }

    // JSON 직렬화를 위한 퀘스트 데이터 래퍼 클래스
    [System.Serializable]
    private class QuestDataWrapper
    {
        public List<int> unlockedStoryIDs;
    }

    #endregion

    /// <summary>
    /// 디버깅용: 저장된 JSON 파일들 삭제
    /// </summary>
    private void DeleteSaveFiles()
    {
        string[] targetFiles =
        {
        Path.Combine(Application.persistentDataPath, "savefile.json"),
        Path.Combine(Application.persistentDataPath, "storydata.json"),
        Path.Combine(Application.persistentDataPath, "playerdata.json")
        // 무기 데이터는 플레이어 이름 기반이니 필요 시 직접 지정하거나 전체 폴더 스캔 가능
    };

        foreach (var file in targetFiles)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
                Debug.Log($"디버깅용 삭제 완료: {file}");
            }
        }

        // 만약 모든 JSON 파일을 싹 지우고 싶다면:
        /*
        string[] allFiles = Directory.GetFiles(Application.persistentDataPath, "*.json");
        foreach (var file in allFiles)
        {
            File.Delete(file);
            Debug.Log($"삭제됨: {file}");
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
