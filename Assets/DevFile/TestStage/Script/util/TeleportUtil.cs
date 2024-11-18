using UnityEngine;
using Unity.Netcode;

namespace NetUtil {

    public class TeleportUtil : MonoBehaviour
    {
        [ServerRpc(RequireOwnership = false)]
        public static void SetEveryPlayerPosServerRPC(ulong playerID, Vector3 pos , Vector3 rot)
        {           
            if (NetworkManager.Singleton.IsServer)
            {
                var changedObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerID];

                changedObject.gameObject.GetComponent<CharacterController>().enabled = false;

                Vector3 teleportPos = pos;

                changedObject.transform.position = teleportPos;
                changedObject.transform.rotation = Quaternion.Euler(rot);
                changedObject.gameObject.GetComponent<CharacterController>().enabled = true;

                MovePlayerClientRpc(changedObject.NetworkObjectId, teleportPos);

                Debug.Log("Set PlayerPosition at " + teleportPos + " .....");             
            }
        }

        // ��� Ŭ���̾�Ʈ���� �̵��� �ݿ��ϴ� RPC
        [ClientRpc]
        private static void MovePlayerClientRpc(ulong playerId, Vector3 targetPosition)
        {
            // �� Ŭ���̾�Ʈ�� �ڽ��� ��ġ�� ������Ʈ
            NetworkObject playerObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerId];
            if (playerObject != null && playerObject.TryGetComponent(out CharacterController characterController))
            {
                characterController.enabled = false;
                playerObject.transform.position = targetPosition;
                characterController.enabled = true;
            }
        }
    }
}

