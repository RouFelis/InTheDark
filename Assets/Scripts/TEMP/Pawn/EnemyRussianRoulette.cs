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

					// �� ĭ�� ����
					//player.Health = Mathf.Min(health + _healValue, maxHealth);

					Debug.Log($"ȸ��!!!!! {_count.Value}ź°");

					_count.Value++;

				}
				else
				{
					player.TakeDamage(_damageValue, default);

					Debug.Log($"����. {player}���� {_damageValue}�� ������.");

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