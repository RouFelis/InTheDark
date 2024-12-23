using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class GoRoom : InteractableObject
{
    GameObject spawnPoint;

    public override void Start()
    {
    }


    public override void Interact(ulong uerID, Transform interactingObjectTransform)
    {
        base.Interact(uerID, interactingObjectTransform);
        // 서버에서 씬 전환
        NetworkManager.Singleton.SceneManager.LoadScene("GameRoom", LoadSceneMode.Single);

		// 추가?
		using var command = new InTheDark.Prototypes.Exit()
		{
			BuildIndex = 0
		};

		command.Invoke();
	}

}
