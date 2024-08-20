using System.IO;
using UnityEngine;

public class PlayerSaveSystem : MonoBehaviour
{
    public string playerName;
    public int experience;
    public int level;

    public void SavePlayerData()
    {
        PlayerData playerData = new PlayerData
        {
            playerName = this.playerName,
            experience = this.experience,
            level = this.level
        };

        string json = JsonUtility.ToJson(playerData, true);
        File.WriteAllText(Application.persistentDataPath + "/playerdata_1.json", json);
    }

    public void LoadPlayerData()
    {
        string path = Application.persistentDataPath + "/playerdata_1.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PlayerData playerData = JsonUtility.FromJson<PlayerData>(json);

            this.playerName = playerData.playerName;
            this.experience = playerData.experience;
            this.level = playerData.level;
        }
    }
}
