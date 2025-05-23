using UnityEngine;
using Unity.Netcode;


public class ObjectDragTest : NetworkBehaviour
{
    public Camera playerCamera;
    public float grabDistance = 3f;
    public float holdDistance = 2f;
    public LayerMask grabbableLayer;
    public KeyCode grabKey = KeyCode.E;

    [SerializeField] private NetworkObject grabbedObject;

    void Update()
    {
        if (!IsOwner) return;

        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * grabDistance, Color.cyan);

        if (Input.GetKeyDown(grabKey))
        {
            if (grabbedObject == null)
                TryGrab();
            else
                Drop(); // 같은 키로 해제
        }
    }

    void TryGrab()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance, grabbableLayer))
        {
			if (!hit.collider.GetComponent<InteractableObject>().IsDragable)
			{
                return;
			}
            NetworkObject netObj = hit.collider.GetComponentInParent<NetworkObject>();
            if (netObj != null)
            {
                Debug.Log($"[CLIENT] Grabbing: {netObj.name}");
                Vector3 camForward = playerCamera.transform.forward;
                RequestGrabServerRpc(netObj.NetworkObjectId, camForward); // 방향 같이 전달
                grabbedObject = netObj;
            }
        }
    }

    void Drop()
    {
        if (grabbedObject == null) return;

        Debug.Log($"[CLIENT] Dropping: {grabbedObject.name}");
        RequestDropServerRpc(grabbedObject.NetworkObjectId);
        grabbedObject = null;
    }

    [ServerRpc]
    void RequestGrabServerRpc(ulong objectId, Vector3 camForward, ServerRpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject netObj))
        {
            netObj.TrySetParent((Transform)null, false);
            netObj.ChangeOwnership(OwnerClientId);

           /* var follower = netObj.GetComponent<NetworkObjectFollower>();
            if (follower != null)
            {
                follower.AlignRotationToPlayer(camForward); // 정방향 회전 맞추기
                follower.StartFollow(OwnerClientId);
            }*/
        }
    }

    [ServerRpc]
    void RequestDropServerRpc(ulong objectId, ServerRpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject netObj))
        {
    /*        var follower = netObj.GetComponent<NetworkObjectFollower>();
            if (follower != null)
                follower.StopFollow(); // 서버가 follow 멈추고 회전 해제*/
        }
    }

    public void SetGrabbed(NetworkObject obj)
    {
        grabbedObject = obj;
    }
}
