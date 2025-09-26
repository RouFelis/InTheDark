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
			// �ν��Ͻ��� �̹� �����ϸ� �ߺ��� ��ü�� ����
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
		// �õ带 ���� �ð����� �����Ͽ� �Ź� �ٸ� ������ ����
		int seed = (int)System.DateTime.Now.Ticks;
		Random.InitState(seed);

		SharedData.Instance.networkSeed.Value = seed;
	}

	private void ChangeValue(int oldValue , int newValue)
	{
		//�������̽� ����
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

	#region �� ����...

	public void SetRoundClearData()
	{
		int round = SharedData.Instance.area.Value;
		Debug.Log($"���� �׽�Ʈ {round}");
		if (round != 0)
		{
			// 2����� 1 ���� �� round / 2
			int questCount = (round / 2);
			// �ּ� 1�� �̻�, �ִ� 8�� ����
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

		Debug.Log("��Ÿ ����.");
	}


	#endregion


}
