using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class QuestManager : NetworkBehaviour
{
	public static QuestManager inst { get; set; }
	public List<QuestBase> questList;
	QuestBase selectedQuest;


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

		if (index != -1)
		{
			Debug.Log("Quest Complete");
		}
		else
		{
			Debug.Log("Quest not found in the list.");
		}
	}

}
