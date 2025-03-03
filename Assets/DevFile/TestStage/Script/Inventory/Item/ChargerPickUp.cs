using UnityEngine;

public class ChargerPickUp : PickupItem
{
	public override void UseItem()
	{
		base.UseItem();
		Debug.Log("아이템 사용 실험 1번");
	}
}
