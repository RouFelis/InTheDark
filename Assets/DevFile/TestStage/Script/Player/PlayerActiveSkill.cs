using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerActiveSkill : NetworkBehaviour
{
    Player player;

    [Header("Prefabs")]
    [SerializeField] private GameObject navigateObject;

    [Header("Skill Settings")]
    [SerializeField] private int maxSkillUses = 10;       // 최대 저장량
    [SerializeField] private float cooldownTime = 3f;     // 사용 쿨타임
    [SerializeField] private float rechargeInterval = 5f; // 1개씩 충전되는 시간

    private int remainingSkillUses;          // 현재 저장량
    private float lastUseTime = -Mathf.Infinity; // 마지막 사용 시간 기록
    private float lastRechargeTime = 0f;         // 마지막 충전 시간 기록

    private void Start()
    {
        player = GetComponent<Player>();
        remainingSkillUses = maxSkillUses; // 시작 시 풀로 채움
        lastRechargeTime = Time.time;
    }

    private void FixedUpdate()
    {
        RechargeHandle();
        NavigateHandle();
    }

    /// <summary>
    /// Q 입력으로 스킬 사용
    /// </summary>
    private void NavigateHandle()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // 저장량이 없거나 쿨타임 중이면 사용 불가
            if (remainingSkillUses <= 0)
            {
                Debug.Log("스킬 저장량이 부족합니다!");
                return;
            }
            if (Time.time - lastUseTime < cooldownTime)
            {
                Debug.Log("스킬 쿨타임입니다!");
                return;
            }

            // 카메라 기준 위치 계산
            Vector3 cameraPosition = Camera.main.transform.position;
            Vector3 cameraForward = Camera.main.transform.forward.normalized;

            float spawnDistance = 2f;
            Ray ray = new Ray(cameraPosition, cameraForward);
            RaycastHit hit;

            Vector3 dropPosition;
            Quaternion dropRotation = Quaternion.LookRotation(cameraForward);

            if (Physics.Raycast(ray, out hit, spawnDistance))
            {
                // 장애물까지 거리보다 가까우면 벽 앞에 살짝 띄워서 스폰
                dropPosition = hit.point - cameraForward * 0.3f;
            }
            else
            {
                // 장애물이 없으면 기본 거리 앞에 스폰
                dropPosition = transform.position + cameraForward * 1.5f;
            }

            // 서버에 스폰 요청
            navigateObjectSpawnServerRpc(dropPosition, dropRotation);

            // 스킬 사용 처리
            remainingSkillUses--;
            lastUseTime = Time.time;
            Debug.Log($"스킬 사용! 남은 개수: {remainingSkillUses}/{maxSkillUses}");
        }
    }

    /// <summary>
    /// 일정 시간마다 스킬 충전
    /// </summary>
    private void RechargeHandle()
    {
        if (remainingSkillUses < maxSkillUses && Time.time - lastRechargeTime >= rechargeInterval)
        {
            remainingSkillUses++;
            lastRechargeTime = Time.time;
            Debug.Log($"스킬 충전됨: {remainingSkillUses}/{maxSkillUses}");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void navigateObjectSpawnServerRpc(Vector3 position, Quaternion rotation)
    {
        NetworkObject networkObject = Instantiate(navigateObject, position + new Vector3(0, 2, 0), rotation).GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
            StartCoroutine(ApplyForceAfterSpawn(networkObject.gameObject.GetComponent<Rigidbody>()));
            Debug.Log("오브젝트 스폰 완료");
        }
    }

    private IEnumerator ApplyForceAfterSpawn(Rigidbody rb)
    {
        yield return null; // 다음 프레임까지 대기
        if (player.handAimTarget != null)
        {
            Vector3 direction = (player.handAimTarget.position - transform.position).normalized;
            rb.AddForce(direction * 10f, ForceMode.Impulse);
            Debug.Log("타겟 방향으로 발사");
        }
    }

}
