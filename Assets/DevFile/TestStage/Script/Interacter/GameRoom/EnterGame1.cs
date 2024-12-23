using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class EnterGame1 : InteractableObject
{
    GameObject spawnPoint;

    public override void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            spawnPoint = GameObject.Find("SpawnPoint");
            // ���������� ����
            if (NetworkManager.Singleton.IsServer)
            {
                foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    // Ŭ���̾�Ʈ�� �÷��̾� ��ü ����
                    var playerObject = client.PlayerObject;
                    client.PlayerObject.gameObject.GetComponent<CharacterController>().enabled = false;
                    client.PlayerObject.transform.position = Vector3.zero;
                    client.PlayerObject.gameObject.GetComponent<CharacterController>().enabled = true;
                    Debug.Log("test");
                }
            }
        }
    }


    public override void Interact(ulong uerID, Transform interactingObjectTransform)
    {

        base.Interact(uerID, interactingObjectTransform);
    }
}
