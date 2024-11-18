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
			// �ν��Ͻ��� �̹� �����ϸ� �ߺ��� ��ü�� ����
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		Money.OnValueChanged += ChangeValue;
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
