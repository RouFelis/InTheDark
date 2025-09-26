using UnityEngine;
using Unity.Netcode;
public class SharedData : NetworkBehaviour
{   
	public static SharedData Instance { get; private set; }

	public NetworkVariable<int> Money = new NetworkVariable<int>(0);
	public NetworkVariable<int> roundMoney = new NetworkVariable<int>(0);
	public NetworkVariable<int> networkSeed = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	public NetworkVariable<int> area = new NetworkVariable<int>(1);
	public NetworkVariable<int> questQuota = new NetworkVariable<int>(1);
	public NetworkVariable<int> moneyQuota = new NetworkVariable<int>(1000);

	public NetworkVariable<int> killed = new NetworkVariable<int>(0);

	private int questQoutaDefult = 3;
	private int moneyQoutaDefult = 300;


	public override void OnNetworkSpawn()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			// 인스턴스가 이미 존재하면 중복된 객체를 삭제
			Destroy(gameObject);
		}
		questQuota.Value = questQoutaDefult;
		moneyQuota.Value = moneyQoutaDefult;
	}

	private void Start()
	{
		Money.OnValueChanged += ChangeValue;
	}

	[ServerRpc]
	public void SetNetSeedServerRpc()
	{
		// 시드를 현재 시간으로 설정하여 매번 다른 난수를 생성
		int seed = (int)System.DateTime.Now.Ticks;
		Random.InitState(seed);

		SharedData.Instance.networkSeed.Value = seed;
	}

	private void ChangeValue(int oldValue , int newValue)
	{
		//인터페이스 변경
	}

	[ServerRpc(RequireOwnership = false)]
	public void AddMoneyServerRpc(int value)
	{
		Money.Value += value;
		roundMoney.Value += value;
	}

	[ServerRpc(RequireOwnership = false)]
	public void subtractionMoneyServerRpc(int value)
	{
		if (Money.Value >= value)
		{
			Money.Value = Money.Value - value;
		}
		else
		{
			Debug.LogError("Can not minus");
		}
		
	}

	public void ClearButton()
	{
		AddMoneyServerRpc(moneyQuota.Value);
		QuestManager.inst.nowClearedQuestTotal.Value = questQuota.Value;

	}

	#region 맵 설정...

	public void SetRoundClearData()
	{
		int round = SharedData.Instance.area.Value;
		Debug.Log($"라운드 테스트 {round}");
		if (round != 0)
		{
			// 2라운드당 1 증가 → round / 2
			int questCount = (round / 2);
			// 최소 1개 이상, 최대 8개 제한
			questCount = Mathf.Clamp(questCount, 1, 8);
			questQuota.Value = questCount;
		}
		else
		{
			questQuota.Value = 1;
		}

		if (round != 0)
		{
			moneyQuota.Value = (int)(300 * Mathf.Pow(1.2f, round));
		}
		else
		{
			moneyQuota.Value = 300;
		}

		roundMoney.Value = 0;

		Debug.Log("쿼타 설정.");
	}


	#endregion


}
