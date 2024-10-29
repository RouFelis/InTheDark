using UnityEngine;
using Unity.Netcode;
public class SharedData : NetworkBehaviour
{
    public NetworkVariable<int> Money = new NetworkVariable<int>(0);
	public static SharedData Instance { get; private set; }

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
