using Unity.Netcode;

using UnityEngine;
using UnityEngine.VFX;

namespace InTheDark.Prototypes
{
    public class EnemyRussianRoulette : InteractableObject
    {
		[SerializeField]
		private float _healValue;

		[SerializeField]
		private float _damageValue;

		[SerializeField]
		private float _radius;

		[SerializeField]
		private int _maxCount;

		[SerializeField]
		private LayerMask _targetLayer;

		[SerializeField]
		private NetworkVariable<int> _count = new();

		[SerializeField]
		private AudioClip _exploseAudioClip;

		[SerializeField]
		private AnimationCurve _knockbackCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

		[SerializeField]
		private AudioSource _audioSource;

		[SerializeField]
		private VisualEffect _exploseEffect;

		private int _size;

		private Collider[] _colliders = new Collider[16];

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
					//player.TakeDamage(_damageValue, default);

					//Debug.Log($"명중. {player}에게 {_damageValue}의 데미지.");

					ExplosionServerRPC();

					Debug.Log("터질게");

					_count.Value = 0;
				}

				//InternalOnInteractClientRPC(reference, isSucceed);
			}
		}

		[Rpc(SendTo.Server)]
		private void ExplosionServerRPC()
		{
			_size = Physics.OverlapSphereNonAlloc(transform.position, _radius, _colliders, _targetLayer);

			for (var i = 0; i < _size; i++)
			{
				var collider = _colliders[i];
				var player = collider?.GetComponent<Player>();

				if (player)
				{
					player.TakeDamage(_damageValue, null);

					if (!player.IsDead)
					{
						var direction = player.transform.position - transform.position;
						var pushDir = direction.normalized;

						var flightTime = 1.0F;
						var flightSpeed = 10.0F;

						var knockBackHeight = 10.0F;

						StartCoroutine(player.SetStun(flightTime, flightSpeed, _knockbackCurve, knockBackHeight, direction));
					}

					Debug.Log($"{player.name}({player.OwnerClientId})가 폭발에 휩쓸림.");
				}

				_colliders[i] = default;
			}

			ExplosionClientRPC();
		}

		[Rpc(SendTo.Everyone)]
		private void ExplosionClientRPC()
		{
			_audioSource.PlayOneShot(_exploseAudioClip);
			_exploseEffect.Play();

			Debug.Log("폭발 효과 재생");
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