using UnityEngine;
using Unity.Netcode;
using UnityEngine.Animations;
using Unity.Netcode.Components;


public class GrabHelper : NetworkBehaviour
{
    public Transform handTransform; // ���� ��ġ�� ��Ÿ���� Transform
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
        // �������� �÷��̾�� �θ�� �����Ǹ� �� ��ġ�� ã��, �������� �� ��ġ�� ����
        if (isPickedUp && handTransform != null)
        {
            // �������� ���� ��ġ�� ȸ���� ���� ������Ʈ
            transform.position = handTransform.position;
            transform.rotation = handTransform.rotation;
        }
    }*/

    [ServerRpc(RequireOwnership = false)]
    public void AttachToPlayerServerRpc(ulong PlayerID)
    {
        // �� ��ġ�� Position Constraint�� Source�� �߰��մϴ�.
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
        // NetworkObjectId�� playerRoot Transform�� ã���ϴ�.
        NetworkObject playerRootNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[PlayerID];
        Transform playerRootTransform = playerRootNetworkObject.gameObject.GetComponent<NetworkInventoryController>().grabHandTransform;

        // Position Constraint�� Source�� �߰��մϴ�.
        ConstraintSource source = new ConstraintSource
        {
            sourceTransform = playerRootTransform,
            weight = 1
        };

        //������ ��.
        positionConstraint.AddSource(source);
        positionConstraint.constraintActive = true;

        //ȸ����
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
        // Position Constraint ��Ȱ��ȭ
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

