using UnityEngine;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine.UI;

public class StoryManaager : MonoBehaviour
{
    public static StoryManaager Inst {get; set;}

    [Header("스토리라인 추가.")]
    [SerializeField] private GameObject notification;
    [SerializeField] private GameObject storyUIPrefab;
    [SerializeField] private Transform storyUIParnets;

    [SerializeField] private StoryCollection storyCollection;
    [SerializeField] private string jsonPath;

    [Header("Story Ref")]
    [SerializeField] private Button storyUIButton;
    [SerializeField] private GameObject buttonRedDot;
    [SerializeField] public TMP_Text storyText;
    [SerializeField] public GameObject memoWindow;

    [SerializeField] private SaveSystem saveSystem; // SaveSystem 참조
    [SerializeField] private List<int> unlockedStoryIDs; // 획득한 스토리 ID 관리 리스트

    //잼민이 써봄.
    void Start()
    {
        Inst = this;
        jsonPath = Path.Combine(Application.streamingAssetsPath, "Story/StoryJson.json");

        saveSystem = FindAnyObjectByType<SaveSystem>();
        if (saveSystem == null)
        {
            Debug.LogError("씬에 SaveSystem이 존재하지 않습니다!");
            return;
        }

        LoadStories(); // 1. 모든 스토리 정보를 먼저 불러오고

        // 2. SaveSystem을 통해 저장된 스토리 목록을 불러와 UI를 복원합니다.
        unlockedStoryIDs = saveSystem.LoadStoryData();
        foreach (int id in unlockedStoryIDs)
        {
            // 불러온 데이터로 UI를 생성할 때는 새로 저장할 필요가 없으므로 false를 전달합니다.
            AddStroyUIPrefab_Load(id);
        }
    }
    public void AddStroyUIPrefab_Load(int num)
    {
        var spawnedPrefab = Instantiate(storyUIPrefab, storyUIParnets);
        var newMarker = spawnedPrefab.GetComponent<StoryUIMarker>();
        TMP_Text tmp = spawnedPrefab.GetComponentInChildren<TMP_Text>();

        if (tmp != null && newMarker != null)
        {
            tmp.text = GetStoryName(num);
            newMarker.StoryID = num;
            newMarker.StoryText = GetStoryText(num);
            newMarker.IsRead = false;
        }
        else
        {
            Debug.LogWarning("SpawnedPrefab 안에 TMP 또는 StoryUIMarker가 없음!");
        }

        buttonRedDot.SetActive(false);
    }

    public void AddStroyUIPrefab(int num)
    {
        // 이미 추가된 스토리인지 확인
        foreach (var unlockedID in unlockedStoryIDs)
        {
            if (unlockedID == num)
            {
                Debug.Log($"스토리 ID {num}는 이미 존재합니다. 추가하지 않습니다.");
                return;
            }
        }

        unlockedStoryIDs.Add(num); // 새로운 스토리 ID를 리스트에 추가
        saveSystem.SaveStoryData(unlockedStoryIDs); // SaveSystem에 저장을 요청

        var spawnedPrefab = Instantiate(storyUIPrefab, storyUIParnets);
        var newMarker = spawnedPrefab.GetComponent<StoryUIMarker>();
        TMP_Text tmp = spawnedPrefab.GetComponentInChildren<TMP_Text>();

        if (tmp != null && newMarker != null)
        {
            tmp.text = GetStoryName(num);
            newMarker.StoryID = num;
            newMarker.StoryText = GetStoryText(num);
            newMarker.IsRead = false;
        }
        else
        {
            Debug.LogWarning("SpawnedPrefab 안에 TMP 또는 StoryUIMarker가 없음!");
        }

        buttonRedDot.SetActive(true);
    }

    void LoadStories()
    {
        if (File.Exists(jsonPath))
        {
            string json = File.ReadAllText(jsonPath);
            StoryEntry[] storyArray = JsonHelper.FromJson<StoryEntry>(json);
            storyCollection = new StoryCollection { stories = new List<StoryEntry>(storyArray) };

            Debug.Log("스토리 로드 완료: " + storyCollection.stories.Count + "개");
        }
        else
        {
            Debug.LogError("스토리 JSON을 찾을 수 없음: " + jsonPath);
        }
    }

 

    public string GetStoryText(int id)
    {
        if (storyCollection == null) return "스토리가 없음";

        foreach (var story in storyCollection.stories)
        {
            if (story.id == id) return story.text;
        }
        return "스토리가 없음";
    }

    public string GetStoryName(int id)
    {
        if (storyCollection == null) return "스토리가 없음";

        foreach (var story in storyCollection.stories)
        {
            if (story.id == id) return story.storyName;
        }
        return "스토리가 없음";
    }


    /// <summary>
    /// 스토리 추가.
    /// </summary>
    /// <param name="num"></param>
  /*  public void AddStroyUIPrefab(int num)
	{
        foreach (Transform child in storyUIParnets)
        {
            var existingMarker = child.GetComponent<StoryUIMarker>();

            if (existingMarker != null && existingMarker.StoryID == num)
            {
                Debug.Log($"스토리 ID {num}는 이미 존재합니다. 추가하지 않습니다.");
                return;
            }
        }

        var spawnedPrefab = Instantiate(storyUIPrefab, storyUIParnets);
        TMP_Text tmp = spawnedPrefab.GetComponentInChildren<TMP_Text>();

        var TemptMarker = spawnedPrefab.GetComponent<StoryUIMarker>();

        if (tmp != null)
        {
            tmp.text = GetStoryName(num);
            TemptMarker.StoryID = num;
            TemptMarker.StoryText = GetStoryText(num);
            TemptMarker.IsRead = false;
        }
        else
        {
            Debug.LogWarning("SpawnedPrefab 안에 TMP가 없음!");
        }

        buttonRedDot.SetActive(true);
    }
*/
}

#region Addon

public class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}

[System.Serializable]
public class StoryEntry
{
    public int id;
    public string storyName;
    public string text;
}

[System.Serializable]
public class StoryCollection
{
    public List<StoryEntry> stories;
}

#endregion
