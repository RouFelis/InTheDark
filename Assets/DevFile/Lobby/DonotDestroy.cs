using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SaintsField.Playa;

public class DonotDestroy : MonoBehaviour
{
    // ����� �� �̸����� enum���� ����
    public enum SceneToDestroyIn
    {
        MainScene,
        Gameplay,
        Settings,
        Credits
    }

    [Header("�� ������Ʈ�� �ı��� ���� �����ϼ���")]
    public SceneToDestroyIn[] destroyInScenes;

    private HashSet<string> destroySceneNames;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        // Destroy ��� �� �̸� ĳ��
        destroySceneNames = new HashSet<string>();

        foreach (var scene in destroyInScenes)
        {
            destroySceneNames.Add(scene.ToString());
        }

        // �� ���� �̺�Ʈ ���
        SceneManager.sceneLoaded += OnSceneLoaded;
        // ���� ���� üũ
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
