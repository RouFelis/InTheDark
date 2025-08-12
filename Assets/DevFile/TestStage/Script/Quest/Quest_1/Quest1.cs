using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.UI;

public class Quest1 : QuestBase
{

	[Tooltip("Trigger 안으로 들어왔을 때 감지할 오브젝트 이름")]
	public string targetObjectName = "TargetObject";  // 감지할 오브젝트의 이름
	[SerializeField] private GameObject collectObjectWall;
	[SerializeField] private NetworkObject insertObject;
	[SerializeField] private Vector3 originPos;
	[SerializeField] private Vector3 endPos;
	[SerializeField] private float moveDuration = 2f;

	[SerializeField] private Image monitor;
	[SerializeField] private Sprite loadingImage;
	[SerializeField] private Sprite completeImage;

	[SerializeField] private AudioClip successSound;
	[SerializeField] private AudioClip failureSound;
	[SerializeField] private AudioSource audioSource;



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
		if (isCompleted.Value)
			return;

		if (networkObjectsInZone == null || networkObjectsInZone.Count == 0)
			return;

		bool hasMatchingObject = false;

		foreach (var netObj in networkObjectsInZone)
		{
			var questItem = netObj?.GetComponent<QuestItem>();
			var player = netObj?.GetComponent<Player>();

			// 플레이어면 데미지 주고 계속 검사
			if (player != null)
			{
				player.damageHandler.RequestDamage(10000);
				continue;
			}

			// QuestItem이 없으면 조건 불일치로 간주
			if (questItem == null)
			{
				Debug.Log($"[TriggerLogger] '{netObj.name}' 에 QuestItem 컴포넌트가 없음.");
				continue;
			}

			// 목표 아이템 목록에 포함되면 성공 조건 만족
			if (targetObjectName.Contains(questItem.itemId))
			{
				hasMatchingObject = true;
			}
		}

		// 최종 판정
		if (hasMatchingObject)
		{
			Debug.Log("[TriggerLogger] 조건에 맞는 오브젝트가 있음 → 퀘스트 성공");
			CompleteBoolChangeServerRpc(true);
			ShowSuccessClientRpc(moveDuration);

			foreach (var netObj in networkObjectsInZone)
			{
				// Player는 디스폰 안 함
				if (netObj.GetComponent<Player>() != null)
					continue;

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
		else
		{
			Debug.Log("[TriggerLogger] 조건에 맞는 오브젝트 없음 → 퀘스트 실패");
			QuestFailedServerRpc();
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

	// 클라이언트 측 UI 및 사운드 처리
	private System.Collections.IEnumerator HandleQuestUISequence(bool isSuccess, float duration)
	{
		if (monitor != null && loadingImage != null)
			monitor.sprite = loadingImage;

		// 사운드 재생
		if (audioSource != null)
		{
			audioSource.clip = isSuccess ? successSound : failureSound;
			audioSource.Play();
		}

		yield return new WaitForSeconds(duration);

		if (monitor != null && isSuccess && completeImage != null)
			monitor.sprite = completeImage;
	}


	[ClientRpc]
	private void ShowSuccessClientRpc(float duration)
	{
		StartCoroutine(HandleQuestUISequence(true, duration));
	}

	[ServerRpc(RequireOwnership = false)]
	protected override void QuestFailedServerRpc()
	{
		base.QuestFailedServerRpc();
		ShowFailureClientRpc();
	}

	[ClientRpc]
	private void ShowFailureClientRpc()
	{
		StartCoroutine(HandleQuestUISequence(false, 2f));
	}

}
