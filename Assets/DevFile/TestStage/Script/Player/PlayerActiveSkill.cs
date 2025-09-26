using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerActiveSkill : NetworkBehaviour
{
    Player player;

    [Header("Prefabs")]
    [SerializeField] private GameObject navigateObject;

    [Header("Skill Settings")]
    [SerializeField] private int maxSkillUses = 10;       // �ִ� ���差
    [SerializeField] private float cooldownTime = 3f;     // ��� ��Ÿ��
    [SerializeField] private float rechargeInterval = 5f; // 1���� �����Ǵ� �ð�

    private int remainingSkillUses;          // ���� ���差
    private float lastUseTime = -Mathf.Infinity; // ������ ��� �ð� ���
    private float lastRechargeTime = 0f;         // ������ ���� �ð� ���

    private void Start()
    {
        player = GetComponent<Player>();
        remainingSkillUses = maxSkillUses; // ���� �� Ǯ�� ä��
        lastRechargeTime = Time.time;
    }

    private void FixedUpdate()
    {
        RechargeHandle();
        NavigateHandle();
    }

    /// <summary>
    /// Q �Է����� ��ų ���
    /// </summary>
    private void NavigateHandle()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // ���差�� ���ų� ��Ÿ�� ���̸� ��� �Ұ�
            if (remainingSkillUses <= 0)
            {
                Debug.Log("��ų ���差�� �����մϴ�!");
                return;
            }
            if (Time.time - lastUseTime < cooldownTime)
            {
                Debug.Log("��ų ��Ÿ���Դϴ�!");
                return;
            }

            // ī�޶� ���� ��ġ ���
            Vector3 cameraPosition = Camera.main.transform.position;
            Vector3 cameraForward = Camera.main.transform.forward.normalized;

            float spawnDistance = 2f;
            Ray ray = new Ray(cameraPosition, cameraForward);
            RaycastHit hit;

            Vector3 dropPosition;
            Quaternion dropRotation = Quaternion.LookRotation(cameraForward);

            if (Physics.Raycast(ray, out hit, spawnDistance))
            {
                // ��ֹ����� �Ÿ����� ������ �� �տ� ��¦ ����� ����
                dropPosition = hit.point - cameraForward * 0.3f;
            }
            else
            {
                // ��ֹ��� ������ �⺻ �Ÿ� �տ� ����
                dropPosition = transform.position + cameraForward * 1.5f;
            }

            // ������ ���� ��û
            navigateObjectSpawnServerRpc(dropPosition, dropRotation);

            // ��ų ��� ó��
            remainingSkillUses--;
            lastUseTime = Time.time;
            Debug.Log($"��ų ���! ���� ����: {remainingSkillUses}/{maxSkillUses}");
        }
    }

    /// <summary>
    /// ���� �ð����� ��ų ����
    /// </summary>
    private void RechargeHandle()
    {
        if (remainingSkillUses < maxSkillUses && Time.time - lastRechargeTime >= rechargeInterval)
        {
            remainingSkillUses++;
            lastRechargeTime = Time.time;
            Debug.Log($"��ų ������: {remainingSkillUses}/{maxSkillUses}");
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
            Debug.Log("������Ʈ ���� �Ϸ�");
        }
    }

    private IEnumerator ApplyForceAfterSpawn(Rigidbody rb)
    {
        yield return null; // ���� �����ӱ��� ���
        if (player.handAimTarget != null)
        {
            Vector3 direction = (player.handAimTarget.position - transform.position).normalized;
            rb.AddForce(direction * 10f, ForceMode.Impulse);
            Debug.Log("Ÿ�� �������� �߻�");
        }
    }

}
