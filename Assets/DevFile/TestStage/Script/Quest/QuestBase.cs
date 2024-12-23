using UnityEngine;
using System;
using Unity.Netcode;

public class QuestBase : NetworkBehaviour
{
    [Header("기본설정")]
    public string questName;
    public string description; 
    public int experienceReward;
    public int moneyReward;
    public GameObject rewardPrefab;
    public Transform spawnPoint;

    public NetworkVariable<bool> isCompleted = new NetworkVariable<bool>(false);


	protected virtual void Start()
	{
        isCompleted.OnValueChanged += QuestComplete;
        QuestManager.inst.QuestInsert(this);
    }

	public virtual void QuestComplete(bool oldValue , bool newValue)
	{
        SpawnClearRewardServerRpc();

    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnClearRewardServerRpc()
    {
        GameObject spawned = Instantiate(rewardPrefab, spawnPoint.position, Quaternion.identity);
        spawned.GetComponent<NetworkObject>().Spawn();
        ClearRewardClientRpc();
    }   
    
    [ClientRpc]
    public void ClearRewardClientRpc()
    {
        QuestManager.inst.QuestComplete(this);
    }

    [ServerRpc(RequireOwnership = false)]
    public void CompleteBoolChangeServerRpc(bool value)
    {
        isCompleted.Value = value;
    }
}