using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    public List<GameObject> objectsToSave;
    public PlayerSaveSystem playerSaveSystem;

    public void SaveGame()
    {
        SaveObjects();
        playerSaveSystem.SavePlayerData();
    }

    public void LoadGame()
    {
        LoadObjects();
        playerSaveSystem.LoadPlayerData();
    }

    private void SaveObjects()
    {
        List<SaveData> saveDataList = new List<SaveData>();

        foreach (GameObject obj in objectsToSave)
        {
            SaveData data = new SaveData
            {
                objectName = obj.name,
                position = obj.transform.position,
                rotation = obj.transform.rotation
            };
            saveDataList.Add(data);
        }

        string json = JsonUtility.ToJson(new SaveDataListWrapper { saveDataList = saveDataList }, true);
        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
    }

    private void LoadObjects()
    {
        string path = Application.persistentDataPath + "/savefile.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveDataListWrapper dataWrapper = JsonUtility.FromJson<SaveDataListWrapper>(json);
            List<SaveData> saveDataList = dataWrapper.saveDataList;

            foreach (SaveData data in saveDataList)
            {
                GameObject obj = GameObject.Find(data.objectName);
                if (obj != null)
                {
                    obj.transform.position = data.position;
                    obj.transform.rotation = data.rotation;
                }
                else
                {
                    Debug.LogWarning("Object not found: " + data.objectName);
                }
            }
        }
    }

    [System.Serializable]
    private class SaveDataListWrapper
    {
        public List<SaveData> saveDataList;
    }
}
