using Unity.Netcode;

using UnityEngine;

namespace InTheDark.Prototypes
{
    public class EnemyRussianRoulette : InteractableObject
    {
		[SerializeField]
		private float _healValue;

		[SerializeField]
		private float _damageValue;

		[SerializeField]
		private int _maxCount;

		[SerializeField]
		private NetworkVariable<int> _count = new();

		public override bool Interact(ulong userId, Transform interactingObjectTransform)
		{
			var player = interactingObjectTransform.GetComponent<Player>();
			var result = base.Interact(userId, interactingObjectTransform);

			InternalOnInteractServerRPC(player);

			return result;
		}

		[Rpc(SendTo.Server)]
		private void InternalOnInteractServerRPC(NetworkBehaviourReference reference)
		{
			var isEnable = reference.TryGet(out Player player);

			if (isEnable)
			{
				var randomValue = Random.Range(0, _maxCount - _count.Value);
				var isSucceed = randomValue > 0;

				if (isSucceed)
				{
					var health = player.Health;
					var maxHealth = player.stats.maxHealth;

					// 힐 칸이 없음
					//player.Health = Mathf.Min(health + _healValue, maxHealth);

					Debug.Log($"회복!!!!! {_count.Value}탄째");

					_count.Value++;

				}
				else
				{
					player.TakeDamage(_damageValue, default);

					Debug.Log($"명중. {player}에게 {_damageValue}의 데미지.");

					_count.Value = 0;
				}

				//InternalOnInteractClientRPC(reference, isSucceed);
			}
		}

		//[Rpc(SendTo.Everyone)]
		//private void InternalOnInteractClientRPC(NetworkBehaviourReference reference, bool isSucceed)
		//{
		//	var isEnable = reference.TryGet(out Player player);

		//	if (isEnable)
		//	{ 
		//		if (isSucceed)
		//		{

		//		}
		//		else
		//		{
		//			player.PlayerName
		//		}
		//	}
		//}
	}
}