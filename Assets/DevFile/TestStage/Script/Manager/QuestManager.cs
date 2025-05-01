using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using System;

public class QuestManager : NetworkBehaviour
{

	public static QuestManager inst { get; set; }
	public List<QuestBase> questList;
	public int mustClearQuestTotal = 0;
	public int nowClearedQuestTotal = 0;
	QuestBase selectedQuest;

	public Action QuestFailAction;


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
		nowClearedQuestTotal += 1;

		if (index != -1)
		{
			Debug.Log("Quest Complete");
		}
		else
		{
			Debug.Log("Quest not found in the list.");
		}
	}

	public void QuestReset()
	{
		nowClearedQuestTotal = 0;
		mustClearQuestTotal = 0;
		questList.Clear();
	}

	
}
