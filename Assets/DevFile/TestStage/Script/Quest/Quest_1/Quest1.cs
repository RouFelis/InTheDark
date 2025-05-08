using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
public class Quest1 : QuestBase
{

    [Tooltip("Trigger 안으로 들어왔을 때 감지할 오브젝트 이름")]
    public string targetObjectName = "TargetObject";  // 감지할 오브젝트의 이름
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

        // NetworkObject 컴포넌트 있는지 확인
        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && !networkObjectsInZone.Contains(netObj))
        {
            networkObjectsInZone.Add(netObj);
            Debug.Log($"[Enter] {netObj.name} 들어옴");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && networkObjectsInZone.Contains(netObj))
        {
            networkObjectsInZone.Remove(netObj);
            Debug.Log($"[Exit] {netObj.name} 나감");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CheckQuestServerRpc()
	{

        // targetObjectName에 포함되지 않은 이름이 하나라도 있는지 검사
        foreach (var netObj in networkObjectsInZone)
        {
            string cleanName = netObj.gameObject.name.Replace("(Clone)", "");
            if (!targetObjectName.Contains(cleanName))
            {
                Debug.Log($"[TriggerLogger] '{cleanName}' 은(는) 퀘스트 조건에 포함되지 않음. 퀘스트 실패 처리.");
                QuestFailedServerRpc();
                return; // 조건이 안 맞으면 즉시 종료
            }
        }

        // 여기까지 왔다는 건 모든 오브젝트 이름이 targetObjectName에 포함된 경우
        Debug.Log("[TriggerLogger] 모든 오브젝트가 퀘스트 조건을 만족함.");

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


    // 디스폰을 위한 NetworkObject 추가
    private System.Collections.IEnumerator MoveObjectOverTime(Transform objTransform, Vector3 startPos, Vector3 targetPos, float duration, NetworkObject netObjToDespawn)
    {
        float elapsed = 0f;

        // 계속 시간 경과에 따라 위치 보간
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            objTransform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            elapsed += Time.deltaTime;

            yield return null; // 다음 프레임까지 대기
        }

		// 마지막 위치 보정
		objTransform.localPosition = targetPos;

        // 코루틴 끝난 후 디스폰
        netObjToDespawn.Despawn(true);
    }

}
