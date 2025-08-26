using UnityEngine;
using Unity.Netcode;
using UnityEngine.Animations;
using Unity.Netcode.Components;


public class GrabHelper : NetworkBehaviour
{
    [Header("Attachment Settings")]
    public Transform handTransform;
    public bool isPickedUp = false;

    [SerializeField] private PositionConstraint positionConstraint;
    [SerializeField] private RotationConstraint rotationConstraint;
    [SerializeField] private NetworkTransform networkTransform;
    [SerializeField] private NetworkRigidbody networkRigidbody;
    [SerializeField] private BoxCollider boxCollider;

    [SerializeField] private Player player;

    private void Start()
    {
        player = GetComponent<Player>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void AttachToPlayerServerRpc(ulong playerId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerId, out NetworkObject playerObj)) return;

        var inventory = playerObj.GetComponent<NetworkInventoryController>();
        inventory.GrabedObject = this.NetworkObject;

        isPickedUp = true;
        this.gameObject.layer = 0;

        AttachToPlayerClientRpc(playerId);
    }

    [ClientRpc]
    private void AttachToPlayerClientRpc(ulong playerId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerId, out NetworkObject playerObj)) return;

        var handTransform = playerObj.GetComponent<NetworkInventoryController>().grabHandTransform;

        ApplyAttachment(handTransform);
    }

    public void DetachFromPlayer()
    {
        isPickedUp = false;
        RemoveAttachment();

        // 다시 물리 활성화
        SetItemPhysics(true);
        this.tag = "Item";
    }

    // =========================
    // 헬퍼 메서드들
    // =========================

    private void ApplyAttachment(Transform targetTransform)
    {
        AddConstraint(positionConstraint, targetTransform);
        AddConstraint(rotationConstraint, targetTransform);

        ResetTransform();
        SetItemPhysics(false);
        this.gameObject.layer = 0;
        isPickedUp = true;
    }

    private void RemoveAttachment()
    {
        positionConstraint.constraintActive = false;
        rotationConstraint.constraintActive = false;

        positionConstraint.RemoveSource(0);
        rotationConstraint.RemoveSource(0);
    }

    private void ResetTransform()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    private void SetItemPhysics(bool enabled)
    {
        if (networkTransform != null)
            networkTransform.enabled = enabled;
        if (networkRigidbody != null)
            networkRigidbody.enabled = enabled; 
        if (boxCollider != null)
            boxCollider.enabled = enabled;
    }

    private void AddConstraint(IConstraint constraint, Transform source)
    {
        var sourceData = new ConstraintSource
        {
            sourceTransform = source,
            weight = 1
        };

        if (constraint is PositionConstraint pos)
        {
            pos.AddSource(sourceData);
            pos.constraintActive = true;
        }
        else if (constraint is RotationConstraint rot)
        {
            rot.AddSource(sourceData);
            rot.constraintActive = true;
        }
    }
}

