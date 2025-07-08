using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SaintsField.Playa;

public class DonotDestroy : MonoBehaviour
{
    // 사용할 씬 이름들을 enum으로 정의
    public enum SceneToDestroyIn
    {
        MainScene,
        Gameplay,
        Settings,
        Credits
    }

    [Header("이 오브젝트가 파괴될 씬을 선택하세요")]
    public SceneToDestroyIn[] destroyInScenes;

    private HashSet<string> destroySceneNames;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        // Destroy 대상 씬 이름 캐싱
        destroySceneNames = new HashSet<string>();

        foreach (var scene in destroyInScenes)
        {
            destroySceneNames.Add(scene.ToString());
        }

        // 씬 변경 이벤트 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
        // 시작 씬도 체크
        CheckScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckScene(scene.name);
    }

    private void CheckScene(string sceneName)
    {
        if (destroySceneNames.Contains(sceneName))
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}
