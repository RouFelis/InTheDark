using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class EnterGame : InteractableObject
{
    GameObject spawnPoint;

    public override void Start()
    {
    }


    public override void Interact(Transform interactingObjectTransform)
    {
        base.Interact(interactingObjectTransform);
        // �������� �� ��ȯ
        NetworkManager.Singleton.SceneManager.LoadScene("TestScene", LoadSceneMode.Single);
    }
}
