using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using System;

public class QuestManager : NetworkBehaviour
{

	public static QuestManager inst { get; set; }
	public List<QuestBase> questList;
	public NetworkVariable<int> mustClearQuestTotal = new NetworkVariable<int>(0);
	public NetworkVariable<int> nowClearedQuestTotal = new NetworkVariable<int>(0);
	QuestBase selectedQuest;

	public Action QuestFailAction;

	// [[25.06.24]] 이벤트 임시 추가
	// QuestBase에 이벤트 연결하는게 제일 좋아 보이긴 한데...
	public delegate void QuestCompletedEventHandler(QuestBase quest, int requireQuestCount, int currentQuestCount);

	public static QuestCompletedEventHandler OnQuestComplete;


	private void Awake()
	{
		inst = this;
		questList = new List<QuestBase>();
	}

	public void QuestInsert(QuestBase quest)
	{
		questList.Add(quest);
	}

	public void QuestComplete(QuestBase quest)
	{
		QuestBase foundQuest = questList.Find(q => q == quest);

		if (foundQuest != null)
		{
			Debug.Log("Quest Complete");
		}
		else
		{
			Debug.LogError("Quest not found.");
		}

		int index = questList.IndexOf(quest);
		nowClearedQuestTotal.Value += 1;

		if (index != -1)
		{
			SharedData.Instance.questQuota.Value += 1;
			Debug.Log("Quest Complete");

			// [[25.06.24]] 이벤트 임시 추가
			OnQuestComplete?.Invoke(quest, mustClearQuestTotal.Value, nowClearedQuestTotal.Value);
		}
		else
		{
			Debug.Log("Quest not found in the list.");
		}
	}

	public void QuestReset()
	{
		nowClearedQuestTotal.Value = 0;
		mustClearQuestTotal.Value = 0;
		questList.Clear();
	}

	
}
