using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.UI;

public class Quest1 : QuestBase
{

	[Tooltip("Trigger ������ ������ �� ������ ������Ʈ �̸�")]
	public string targetObjectName = "TargetObject";  // ������ ������Ʈ�� �̸�
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
		if (isCompleted.Value)
			return;

		if (networkObjectsInZone == null || networkObjectsInZone.Count == 0)
			return;

		bool hasMatchingObject = false;

		foreach (var netObj in networkObjectsInZone)
		{
			var questItem = netObj?.GetComponent<QuestItem>();
			var player = netObj?.GetComponent<Player>();

			// �÷��̾�� ������ �ְ� ��� �˻�
			if (player != null)
			{
				player.damageHandler.RequestDamage(10000);
				continue;
			}

			// QuestItem�� ������ ���� ����ġ�� ����
			if (questItem == null)
			{
				Debug.Log($"[TriggerLogger] '{netObj.name}' �� QuestItem ������Ʈ�� ����.");
				continue;
			}

			// ��ǥ ������ ��Ͽ� ���ԵǸ� ���� ���� ����
			if (targetObjectName.Contains(questItem.itemId))
			{
				hasMatchingObject = true;
			}
		}

		// ���� ����
		if (hasMatchingObject)
		{
			Debug.Log("[TriggerLogger] ���ǿ� �´� ������Ʈ�� ���� �� ����Ʈ ����");
			CompleteBoolChangeServerRpc(true);
			ShowSuccessClientRpc(moveDuration);

			foreach (var netObj in networkObjectsInZone)
			{
				// Player�� ���� �� ��
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
			Debug.Log("[TriggerLogger] ���ǿ� �´� ������Ʈ ���� �� ����Ʈ ����");
			QuestFailedServerRpc();
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

	// Ŭ���̾�Ʈ �� UI �� ���� ó��
	private System.Collections.IEnumerator HandleQuestUISequence(bool isSuccess, float duration)
	{
		if (monitor != null && loadingImage != null)
			monitor.sprite = loadingImage;

		// ���� ���
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
