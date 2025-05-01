using UnityEngine;
using Unity.Netcode;

public class GrabbableObject : InteractableObject
{
    [Header("Grabbable Settings")]
    public float followDistance = 2f;
    public float baseFollowSpeed = 10f;
    public float maxDistanceBeforeDrop = 5f;

    [SerializeField] private bool isFollowing = false;
    [SerializeField] private ulong followingClientId;
    [SerializeField] private Rigidbody rb;

    public override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody>();
    }

    public override void Interact(ulong userId, Transform interactingObjectTransform)
    {
        if (isFollowing && userId == followingClientId)
        {
            Debug.Log("[SERVER] Already grabbed, dropping now.");
            StopGrab();
        }
        else
        {
            Debug.Log("[SERVER] Interacted by " + interactingObjectTransform.name + ", grabbing now.");
            StartGrab(userId);
        }
    }

    void StartGrab(ulong clientId)
    {
        followingClientId = clientId;
        isFollowing = true;

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.linearVelocity = Vector3.zero;
        }
    }

    public void StopGrab()
    {
        Debug.Log("[SERVER] Stopping Grab");

        isFollowing = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.None;
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner||!isFollowing || rb == null) return;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(followingClientId, out var client)) return;
        var playerObj = client.PlayerObject;
        if (playerObj == null) return;


        Transform playerTransform = playerObj.transform;

        // 플레이어 기준 앞 방향 계산
        Vector3 forward = playerTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 targetPos = playerTransform.position + forward * followDistance;
        targetPos.y = playerTransform.position.y;

        float distance = Vector3.Distance(transform.position, targetPos);
        if (distance > maxDistanceBeforeDrop)
        {
            Debug.LogWarning("[SERVER] Auto-drop: player too far.");
            StopGrab();
            return;
        }

        float massFactor = Mathf.Max(rb.mass, 0.1f);
        float adjustedSpeed = baseFollowSpeed / massFactor;
        Vector3 direction = targetPos - transform.position;
        rb.linearVelocity = direction * adjustedSpeed;

        // 정면 회전 유지
        if (forward.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(forward, Vector3.up);
            rb.MoveRotation(targetRotation);
        }

        Debug.DrawLine(transform.position, targetPos, Color.green);
    }
}

