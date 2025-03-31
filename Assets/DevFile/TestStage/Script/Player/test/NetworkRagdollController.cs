using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class NetworkRagdollController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rootBone; // Hips 본 연결
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
        // 초기 랙돌 상태 비활성화
        ToggleRagdoll(false);
        //networkTransform.enabled = false;
    }

    // 랙돌 활성화/비활성화 메인 메서드
    private void ToggleRagdoll(bool activate)
    {
        // 콜라이더 및 물리 상태 변경
        foreach (var col in ragdollColliders)
            col.enabled = activate;

        foreach (var rb in ragdollRbs)
        {
            rb.isKinematic = !activate;
            rb.detectCollisions = activate;
        }

        // 애니메이션 및 컨트롤러 상태
        animator.enabled = !activate;
        controller.enabled = !activate;
    }

    [ServerRpc]
    public void DieServerRpc(Vector3 deathPosition)
    {
        isRagdoll.Value = true;
        networkTransform.enabled = true;

        // 클라이언트 동기화
        ActivateRagdollClientRpc(deathPosition);
        StartCoroutine(StopPhysicsCoroutine());
    }

    [ClientRpc]
    private void ActivateRagdollClientRpc(Vector3 deathPosition)
    {
        ToggleRagdoll(true);

        // 랙돌에 폭발력 적용 (모든 클라이언트에서 동일하게 계산)
        Rigidbody hipsRb = rootBone.GetComponent<Rigidbody>();
        hipsRb.AddExplosionForce(deathForce, deathPosition, 5f);
    }

    [ServerRpc]
    public void ReviveServerRpc()
    {
        isRagdoll.Value = false;
        networkTransform.enabled = false;

        // 위치 리셋
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

        // 물리 정지 후 위치 고정
        foreach (var rb in ragdollRbs)
            rb.isKinematic = true;
    }

    private Vector3 GetSpawnPosition()
    {
        // 리스폰 위치 로직 구현
        return Vector3.zero;
    }

    public override void OnNetworkDespawn()
    {
        // 오브젝트 제거 시 초기화
        ToggleRagdoll(false);
        networkTransform.enabled = false;
    }
}
