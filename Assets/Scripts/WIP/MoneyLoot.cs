using UnityEngine;

namespace InTheDark.Prototypes
{
	[CreateAssetMenu(fileName = "new money loot", menuName = "loots/money")]
	public class MoneyLoot : Loot
	{
		[SerializeField]
		private int _value;

		public override void Execute(EnemyPrototypePawn pawn)
		{
			//var instance = SharedData.Instance;

			//if (!instance)
			//{
			//	return;
			//}
			//else if (!instance.NetworkObject.IsSpawned)
			//{
			//	instance.NetworkObject.Spawn();
			//}

			//SharedData.Instance.Money.Value += _value;
			SharedData.Instance.AddMoneyServerRpc(_value);
		}
	}
}