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
        // �������� �� ��ȯ
        NetworkManager.Singleton.SceneManager.LoadScene("GameRoom", LoadSceneMode.Single);

		// �߰�?
		using var command = new InTheDark.Prototypes.Exit()
		{
			BuildIndex = 0
		};

		command.Invoke();
	}

}
