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
			// 인스턴스가 이미 존재하면 중복된 객체를 삭제
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		Money.OnValueChanged += ChangeValue;
	}

	public void SetNetSeed()
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
