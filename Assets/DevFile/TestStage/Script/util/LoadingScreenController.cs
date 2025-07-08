using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class LoadingScreenController : MonoBehaviour
{
	public GameObject loadingUI;
	public TMP_Text loadingText; // ������� ǥ���� UI �ؽ�Ʈ
	private AsyncOperation currentOp;
	private bool isLoading = false;
	NetworkSceneManager networkSceneManager;

	void Start()
	{
		//StartCoroutine(test());
	}

	public IEnumerator test()
	{
		while (networkSceneManager == null)
		{
			try
			{				
				NetworkManager.Singleton.SceneManager.OnLoad += OnSceneLoadStarted;
				Debug.Log("�׽�Ʈ 2222222222222");
				NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadCompleted;
				Debug.Log("�׽�Ʈ 33333333333333333");

				DontDestroyOnLoad(this);
			}
			catch
			{
				Debug.Log("NetworkManager Serching...");
			}
			yield return new WaitForSeconds(0.1f);
		}
	}

	void OnDestroy()
	{
		var sceneManager = NetworkManager.Singleton.SceneManager;

		sceneManager.OnLoad -= OnSceneLoadStarted;
		sceneManager.OnLoadComplete -= OnSceneLoadCompleted;
	}

	// �� �ε� ����
	private void OnSceneLoadStarted(ulong clientId, string sceneName, LoadSceneMode mode, AsyncOperation op)
	{
		if (clientId == NetworkManager.Singleton.LocalClientId)
		{
			loadingUI?.SetActive(true);
			currentOp = op;
			isLoading = true;
			Debug.Log($"[�ε� ����] scene: {sceneName}");
		}
	}

	// �� �ε� �Ϸ�
	private void OnSceneLoadCompleted(ulong clientId, string sceneName, LoadSceneMode mode)
	{
		if (clientId == NetworkManager.Singleton.LocalClientId)
		{
			isLoading = false;
			loadingUI?.SetActive(false);
			currentOp = null;
			Debug.Log($"[�ε� �Ϸ�] scene: {sceneName}");
		}
	}

	void Update()
	{
		if (isLoading && currentOp != null)
		{
			float progress = Mathf.Clamp01(currentOp.progress / 0.9f); // 0~1 ���� ���߱�
			int percentage = Mathf.RoundToInt(progress * 100);
			loadingText.text = $"Loading... {percentage}%";
		}
	}

}
