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

			// 네트워크 오브젝트 유지하려면 이거로 해야하나?
			//NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);

			await SceneManager.LoadSceneAsync("Lobby", LoadSceneMode.Single);
		}
	} 
}