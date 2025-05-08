using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
public class Quest1 : QuestBase
{

    [Tooltip("Trigger ������ ������ �� ������ ������Ʈ �̸�")]
    public string targetObjectName = "TargetObject";  // ������ ������Ʈ�� �̸�
    [SerializeField] private GameObject collectObjectWall;
    [SerializeField] private NetworkObject insertObject;
    [SerializeField] private Vector3 originPos;
    [SerializeField] private Vector3 endPos;
    [SerializeField] private float moveDuration = 2f;
    
    
    [SerializeField] private List<NetworkObject> networkObjectsInZone = new List<NetworkObject>();   
    public IReadOnlyList<NetworkObject> ObjectsInZone => networkObjectsInZone;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        // NetworkObject ������Ʈ �ִ��� Ȯ��
        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && !networkObjectsInZone.Contains(netObj))
        {
            networkObjectsInZone.Add(netObj);
            Debug.Log($"[Enter] {netObj.name} ����");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && networkObjectsInZone.Contains(netObj))
        {
            networkObjectsInZone.Remove(netObj);
            Debug.Log($"[Exit] {netObj.name} ����");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CheckQuestServerRpc()
	{

        // targetObjectName�� ���Ե��� ���� �̸��� �ϳ��� �ִ��� �˻�
        foreach (var netObj in networkObjectsInZone)
        {
            string cleanName = netObj.gameObject.name.Replace("(Clone)", "");
            if (!targetObjectName.Contains(cleanName))
            {
                Debug.Log($"[TriggerLogger] '{cleanName}' ��(��) ����Ʈ ���ǿ� ���Ե��� ����. ����Ʈ ���� ó��.");
                QuestFailedServerRpc();
                return; // ������ �� ������ ��� ����
            }
        }

        // ������� �Դٴ� �� ��� ������Ʈ �̸��� targetObjectName�� ���Ե� ���
        Debug.Log("[TriggerLogger] ��� ������Ʈ�� ����Ʈ ������ ������.");

        CompleteBoolChangeServerRpc(true);

        foreach (var netObj in networkObjectsInZone)
        {
            if (collectObjectWall != null)
            {
                StartCoroutine(MoveObjectOverTime(
                    collectObjectWall.transform,
                    originPos,
                    endPos,
                    moveDuration,
                    netObj
                ));
            }
            else
            {
                netObj.Despawn(true);
            }
        }
    }


    // ������ ���� NetworkObject �߰�
    private System.Collections.IEnumerator MoveObjectOverTime(Transform objTransform, Vector3 startPos, Vector3 targetPos, float duration, NetworkObject netObjToDespawn)
    {
        float elapsed = 0f;

        // ��� �ð� ����� ���� ��ġ ����
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            objTransform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            elapsed += Time.deltaTime;

            yield return null; // ���� �����ӱ��� ���
        }

		// ������ ��ġ ����
		objTransform.localPosition = targetPos;

        // �ڷ�ƾ ���� �� ����
        netObjToDespawn.Despawn(true);
    }

}
