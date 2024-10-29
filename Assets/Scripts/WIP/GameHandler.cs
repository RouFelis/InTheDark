using Cysharp.Threading.Tasks;

using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace InTheDark.Prototypes
{
    public sealed class GameHandler : MonoBehaviour
    {
		private async void Awake()
		{
			DontDestroyOnLoad(gameObject);

			// ��Ʈ��ũ ������Ʈ �����Ϸ��� �̰ŷ� �ؾ��ϳ�?
			//NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);

			await SceneManager.LoadSceneAsync("Lobby", LoadSceneMode.Single);
		}
	} 
}