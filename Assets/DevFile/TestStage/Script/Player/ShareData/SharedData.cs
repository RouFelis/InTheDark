using UnityEngine;
using Unity.Netcode;
public class SharedData : NetworkBehaviour
{   
	public static SharedData Instance { get; private set; }

	public NetworkVariable<int> Money = new NetworkVariable<int>(0);
	public NetworkVariable<int> networkSeed = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	public NetworkVariable<int> area = new NetworkVariable<int>(0);
	public NetworkVariable<int> questQuota = new NetworkVariable<int>(0);
	public NetworkVariable<int> moneyQuota = new NetworkVariable<int>(0);

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
	}

	private void Start()
	{
		Money.OnValueChanged += ChangeValue;
	}

	public void SetNetSeed()
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
	}

	public int subtractionMoney(int value)
	{
		if (Money.Value >= value)
		{
			Money.Value = Money.Value - value;
			return Money.Value;
		}
		else
		{
			Debug.LogError("Can not minus");
			return -1; 
		}
		
	}




}
