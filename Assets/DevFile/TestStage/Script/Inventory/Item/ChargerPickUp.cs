using UnityEngine;

public class ChargerPickUp : PickupItem
{
	public override void UseItem()
	{
		base.UseItem();
		Debug.Log("������ ��� ���� 1��");
	}
}
