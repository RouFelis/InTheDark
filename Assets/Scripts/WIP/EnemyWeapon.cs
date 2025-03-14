using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using Unity.Netcode;
using UnityEngine;

namespace InTheDark.Prototypes
{
	public class EnemyWeapon : NetworkBehaviour
	{
		[SerializeField]
		private float _damage;

		[SerializeField]
		private float _range;

		[SerializeField]
		private NetworkVariable<float> _cooldown = new NetworkVariable<float>();

		[SerializeField]
		private EnemyPrototypePawn _pawn;

		[SerializeField]
		private Animator _animator;

		public bool IsTargetNearby
		{
			get
			{
				var target = _pawn.Target;

				if (!target)
				{
					return false;
				}

				var direction = target.transform.position - transform.position;
				var isNearBy = target ? Vector3.Distance(target.transform.position, transform.position) <= _range : false;
				var isOccultation = Physics.Raycast(transform.position, direction, out var hit, _range);

				var isAttackable = isNearBy && isOccultation;

				return isAttackable;
			}
		}

		private void Update()
		{
			if (IsServer)
			{
				_cooldown.Value = Math.Max(_cooldown.Value - Time.deltaTime, 0.0F);
			}
		}

		public async UniTaskVoid Attack()
		{
			if (_cooldown.Value < 0.0F || Mathf.Approximately(_cooldown.Value, 0.0F))
			{
				var target = _pawn.Target;

				_cooldown.Value = _pawn.InitializeCooldownValue;

				//value.TakeDamage(_damage.Value , attackSound);
				//_animator?.SetTrigger("OnAttack");

				await OnAttackWithAnimaiton(_pawn.Target);
			}
		}

		private async UniTask OnAttackWithAnimaiton(IHealth target)
		{
			using var source = new CancellationTokenSource();

			if (_animator)
			{
				_animator.SetTrigger(EnemyPrototypePawn.ATTACK_TRIGGER);
				//_onAttack = source;
			}

			//_animator?.SetTrigger(ATTACK_TRIGGER);

			await UniTask.Delay(TimeSpan.FromSeconds(0.9F));

			if (IsTargetNearby)
			{
				target.TakeDamage(_damage, _pawn.attackSound);
			}

			//Debug.Log("HIT!!!");
		}
	} 
}