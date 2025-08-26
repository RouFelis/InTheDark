using UnityEngine;
using Unity.Netcode;
public class SharedData : NetworkBehaviour
{   
	public static SharedData Instance { get; private set; }

	public NetworkVariable<int> Money = new NetworkVariable<int>(0);
	public NetworkVariable<int> networkSeed = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	public NetworkVariable<int> area = new NetworkVariable<int>(1);
	public NetworkVariable<int> questQuota = new NetworkVariable<int>(1);
	public NetworkVariable<int> moneyQuota = new NetworkVariable<int>(1000);

	private int questQoutaDefult = 3;
	private int moneyQoutaDefult = 3;


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

		if (IsServer)
			area.OnValueChanged += SetQuota;
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



	#region 맵 설정...


	public void SetQuota(int newValue, int oldValue)
	{
		RequestQuestQuota();
		RequestMoneyQuota();
	}


	private int RequestQuestQuota()
	{
		Debug.Log($"퀘스트 할당량 호출 : {(area.Value % 2)} ");
		return 3 + (area.Value % 2);
	}

	private int RequestMoneyQuota()
	{
		Debug.Log($"돈 할당량 호출 : {(area.Value % 2)} ");
		return 500 + 500 / (area.Value % 2);
	}



	#endregion


}
