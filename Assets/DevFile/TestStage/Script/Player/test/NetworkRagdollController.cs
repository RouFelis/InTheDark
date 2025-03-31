using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class NetworkRagdollController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rootBone; // Hips �� ����
    [SerializeField] private Rigidbody[] ragdollRbs;
    [SerializeField] private Collider[] ragdollColliders;

    [Header("Settings")]
    [SerializeField] private float deathForce = 35f;
    [SerializeField] private float physicsDuration = 4f;
    [SerializeField] private Animator animator;

    private NetworkTransform networkTransform;    
    private CharacterController controller;
    private NetworkVariable<bool> isRagdoll = new NetworkVariable<bool>();

    private void Awake()
    {
        networkTransform = rootBone.GetComponent<NetworkTransform>();
        controller = GetComponent<CharacterController>();

        //InitializeRagdoll();
    }

    private void InitializeRagdoll()
    {
        // �ʱ� ���� ���� ��Ȱ��ȭ
        ToggleRagdoll(false);
        //networkTransform.enabled = false;
    }

    // ���� Ȱ��ȭ/��Ȱ��ȭ ���� �޼���
    private void ToggleRagdoll(bool activate)
    {
        // �ݶ��̴� �� ���� ���� ����
        foreach (var col in ragdollColliders)
            col.enabled = activate;

        foreach (var rb in ragdollRbs)
        {
            rb.isKinematic = !activate;
            rb.detectCollisions = activate;
        }

        // �ִϸ��̼� �� ��Ʈ�ѷ� ����
        animator.enabled = !activate;
        controller.enabled = !activate;
    }

    [ServerRpc]
    public void DieServerRpc(Vector3 deathPosition)
    {
        isRagdoll.Value = true;
        networkTransform.enabled = true;

        // Ŭ���̾�Ʈ ����ȭ
        ActivateRagdollClientRpc(deathPosition);
        StartCoroutine(StopPhysicsCoroutine());
    }

    [ClientRpc]
    private void ActivateRagdollClientRpc(Vector3 deathPosition)
    {
        ToggleRagdoll(true);

        // ������ ���߷� ���� (��� Ŭ���̾�Ʈ���� �����ϰ� ���)
        Rigidbody hipsRb = rootBone.GetComponent<Rigidbody>();
        hipsRb.AddExplosionForce(deathForce, deathPosition, 5f);
    }

    [ServerRpc]
    public void ReviveServerRpc()
    {
        isRagdoll.Value = false;
        networkTransform.enabled = false;

        // ��ġ ����
        transform.position = GetSpawnPosition();
        ReviveClientRpc();
    }

    [ClientRpc]
    private void ReviveClientRpc()
    {
        ToggleRagdoll(false);
        controller.enabled = true;
        animator.enabled = true;
    }

    private System.Collections.IEnumerator StopPhysicsCoroutine()
    {
        yield return new WaitForSeconds(physicsDuration);

        // ���� ���� �� ��ġ ����
        foreach (var rb in ragdollRbs)
            rb.isKinematic = true;
    }

    private Vector3 GetSpawnPosition()
    {
        // ������ ��ġ ���� ����
        return Vector3.zero;
    }

    public override void OnNetworkDespawn()
    {
        // ������Ʈ ���� �� �ʱ�ȭ
        ToggleRagdoll(false);
        networkTransform.enabled = false;
    }
}
