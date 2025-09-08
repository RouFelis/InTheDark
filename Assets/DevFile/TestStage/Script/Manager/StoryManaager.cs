using UnityEngine;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine.UI;

public class StoryManaager : MonoBehaviour
{
    public static StoryManaager Inst {get; set;}

    [Header("���丮���� �߰�.")]
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

    [SerializeField] private SaveSystem saveSystem; // SaveSystem ����
    [SerializeField] private List<int> unlockedStoryIDs; // ȹ���� ���丮 ID ���� ����Ʈ

    //����� �ẽ.
    void Start()
    {
        Inst = this;
        jsonPath = Path.Combine(Application.streamingAssetsPath, "Story/StoryJson.json");

        saveSystem = FindAnyObjectByType<SaveSystem>();
        if (saveSystem == null)
        {
            Debug.LogError("���� SaveSystem�� �������� �ʽ��ϴ�!");
            return;
        }

        LoadStories(); // 1. ��� ���丮 ������ ���� �ҷ�����

        // 2. SaveSystem�� ���� ����� ���丮 ����� �ҷ��� UI�� �����մϴ�.
        unlockedStoryIDs = saveSystem.LoadStoryData();
        foreach (int id in unlockedStoryIDs)
        {
            // �ҷ��� �����ͷ� UI�� ������ ���� ���� ������ �ʿ䰡 �����Ƿ� false�� �����մϴ�.
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
            Debug.LogWarning("SpawnedPrefab �ȿ� TMP �Ǵ� StoryUIMarker�� ����!");
        }

        buttonRedDot.SetActive(false);
    }

    public void AddStroyUIPrefab(int num)
    {
        // �̹� �߰��� ���丮���� Ȯ��
        foreach (var unlockedID in unlockedStoryIDs)
        {
            if (unlockedID == num)
            {
                Debug.Log($"���丮 ID {num}�� �̹� �����մϴ�. �߰����� �ʽ��ϴ�.");
                return;
            }
        }

        unlockedStoryIDs.Add(num); // ���ο� ���丮 ID�� ����Ʈ�� �߰�
        saveSystem.SaveStoryData(unlockedStoryIDs); // SaveSystem�� ������ ��û

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
            Debug.LogWarning("SpawnedPrefab �ȿ� TMP �Ǵ� StoryUIMarker�� ����!");
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

            Debug.Log("���丮 �ε� �Ϸ�: " + storyCollection.stories.Count + "��");
        }
        else
        {
            Debug.LogError("���丮 JSON�� ã�� �� ����: " + jsonPath);
        }
    }

 

    public string GetStoryText(int id)
    {
        if (storyCollection == null) return "���丮�� ����";

        foreach (var story in storyCollection.stories)
        {
            if (story.id == id) return story.text;
        }
        return "���丮�� ����";
    }

    public string GetStoryName(int id)
    {
        if (storyCollection == null) return "���丮�� ����";

        foreach (var story in storyCollection.stories)
        {
            if (story.id == id) return story.storyName;
        }
        return "���丮�� ����";
    }


    /// <summary>
    /// ���丮 �߰�.
    /// </summary>
    /// <param name="num"></param>
  /*  public void AddStroyUIPrefab(int num)
	{
        foreach (Transform child in storyUIParnets)
        {
            var existingMarker = child.GetComponent<StoryUIMarker>();

            if (existingMarker != null && existingMarker.StoryID == num)
            {
                Debug.Log($"���丮 ID {num}�� �̹� �����մϴ�. �߰����� �ʽ��ϴ�.");
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
            Debug.LogWarning("SpawnedPrefab �ȿ� TMP�� ����!");
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
