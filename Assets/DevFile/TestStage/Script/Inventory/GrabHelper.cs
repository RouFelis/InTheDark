using UnityEngine;
using Unity.Netcode;
using UnityEngine.Animations;
using Unity.Netcode.Components;


public class GrabHelper : NetworkBehaviour
{
    public Transform handTransform; // 손의 위치를 나타내는 Transform
    public bool isPickedUp = false;

    [SerializeField] PositionConstraint positionConstraint;
    [SerializeField] RotationConstraint rotationConstraint;
    [SerializeField] NetworkTransform networkTransform;
    [SerializeField] NetworkRigidbody networkRigidbody;

    private void Start()
    {

    }

    /*private void Update()
    {
        // 아이템이 플레이어에게 부모로 지정되면 손 위치를 찾고, 아이템을 손 위치에 맞춤
        if (isPickedUp && handTransform != null)
        {
            // 아이템을 손의 위치와 회전에 맞춰 업데이트
            transform.position = handTransform.position;
            transform.rotation = handTransform.rotation;
        }
    }*/

    [ServerRpc(RequireOwnership = false)]
    public void AttachToPlayerServerRpc(ulong PlayerID)
    {
        // 손 위치를 Position Constraint의 Source로 추가합니다.
        NetworkObject playerRootNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[PlayerID];
//        ConstraintSource source = new ConstraintSource();

        NetworkInventoryController playerInven = playerRootNetworkObject.gameObject.GetComponent<NetworkInventoryController>();
        playerInven.GrabedObject = this.NetworkObject;

        this.gameObject.transform.position = new Vector3(0,0,0);
        this.gameObject.transform.rotation = Quaternion.identity;

        this.tag = "Untagged";
        isPickedUp = true;
        AttachToPlayerClientRpc(PlayerID);
    }

    [ClientRpc]
    private void AttachToPlayerClientRpc(ulong PlayerID)
    {
        // NetworkObjectId로 playerRoot Transform을 찾습니다.
        NetworkObject playerRootNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[PlayerID];
        Transform playerRootTransform = playerRootNetworkObject.gameObject.GetComponent<NetworkInventoryController>().grabHandTransform;

        // Position Constraint에 Source를 추가합니다.
        ConstraintSource source = new ConstraintSource
        {
            sourceTransform = playerRootTransform,
            weight = 1
        };

        //포지션 값.
        positionConstraint.AddSource(source);
        positionConstraint.constraintActive = true;

        //회전값
        rotationConstraint.AddSource(source);
        rotationConstraint.constraintActive = true;

        this.gameObject.transform.position = new Vector3(0, 0, 0);
        this.gameObject.transform.rotation = Quaternion.identity;

        networkTransform.enabled = false;
        networkRigidbody.enabled = false;

        this.tag = "Item";
        isPickedUp = true;
    }

    public void DetachFromPlayer()
    {
        // Position Constraint 비활성화
        positionConstraint.constraintActive = false;
        rotationConstraint.constraintActive = false;
        isPickedUp = false;

        this.tag = "Item";
        positionConstraint.RemoveSource(0);
        rotationConstraint.RemoveSource(0);
        networkTransform.enabled = true;
        networkRigidbody.enabled = true;
    }
}

