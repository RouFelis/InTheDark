using UnityEngine;
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
    public NetworkVariable<int> failTime = new NetworkVariable<int>(value: 0);
	[SerializeField] private int MaxFailTime = 3;

    [Header("기본 스토리 설정")]
    [SerializeField] private bool isStory = false;
    [SerializeField] private int[] storyNumbers;
    [SerializeField] private int storyNumber;

    protected virtual void Start()
	{
        isCompleted.OnValueChanged += QuestCompleteReward;
        SetStroyNumber();
        QuestManager.inst.QuestInsert(this);
    }

	public virtual void QuestCompleteReward(bool oldValue , bool newValue)
	{
		if (newValue)
        {
            if(rewardPrefab != null)
                SpawnClearRewardServerRpc();
            if(isStory)
              StoryManaager.Inst.AddStroyUIPrefab(storyNumber);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    protected virtual void QuestSucceedServerRpc()
    {
        failTime.Value += 1;

        if (MaxFailTime <= failTime.Value)
        {
            QuestManager.inst.QuestFailAction.Invoke();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void QuestFailedServerRpc()
	{
        failTime.Value += 1;

        if (MaxFailTime <= failTime.Value)
		{
           // QuestManager.inst.QuestFailAction.Invoke();
        }
	}

    [ServerRpc(RequireOwnership = false)]
    public void SpawnClearRewardServerRpc()
    {
		if (rewardPrefab != null)
        {
            GameObject spawned = Instantiate(rewardPrefab, spawnPoint.position, spawnPoint.rotation);
            spawned.GetComponent<NetworkObject>().Spawn();
		}
        ClearRewardClientRpc();
    }   
    
    [ClientRpc]
    public void ClearRewardClientRpc()
    {
        QuestManager.inst.QuestComplete(this);
    }



    /// <summary>
    /// 스토리 관련
    /// </summary>
    private void SetStroyNumber()
    {
        // 아직 안 정해졌다면 랜덤으로 선택
        if (storyNumbers.Length > 0)
        {
            storyNumber = storyNumbers[Random.Range(0, storyNumbers.Length)];
        }
    }

}