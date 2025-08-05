using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using Unity.Collections;

public class Computer_Quest5 : InteractableObject_NonNet
{
    [SerializeField] private Quest5 quest5;


    public override void Interact(ulong userId, Transform interactingObjectTransform)
    {
        Debug.Log($"테스트 : {userId}");

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(userId, out var networkClient))
        {
            NetworkObject playerObject = networkClient.PlayerObject;

            if (playerObject != null)
            {
                playerObject.GetComponent<NetworkInventoryController>().HandleEraseItem();

                quest5.StartQuest();
            }
            else
            {
                Debug.LogWarning("플레이어 오브젝트가 null입니다.");
            }
        }
        else
        {
            Debug.LogWarning($"해당 ClientId({userId})에 대한 ConnectedClient를 찾을 수 없습니다.");
        }

        base.Interact(userId, interactingObjectTransform);
    }


}
