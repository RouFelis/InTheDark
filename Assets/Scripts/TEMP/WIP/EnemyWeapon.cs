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
		protected float _damage;

		[SerializeField]
		protected float _range;

		[SerializeField]
		protected float _delay = 0.9F;

		[SerializeField]
		protected NetworkVariable<float> _cooldown = new NetworkVariable<float>();

		[SerializeField]
		protected EnemyPrototypePawn _pawn;

		[SerializeField]
		protected Animator _animator;

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

		public virtual async UniTaskVoid Attack()
		{
			if (_cooldown.Value < 0.0F || Mathf.Approximately(_cooldown.Value, 0.0F) && IsServer)
			{
				var target = _pawn.Target;

				_cooldown.Value = _pawn.InitializeCooldownValue;

				//value.TakeDamage(_damage.Value , attackSound);
				//_animator?.SetTrigger("OnAttack");

				await OnAttack(_pawn.Target);
			}
		}

		protected virtual async UniTask OnAttack(IHealth target)
		{
			using var source = new CancellationTokenSource();

			if (_animator)
			{
				_animator.SetTrigger(EnemyPrototypePawn.ATTACK_TRIGGER);
				//_onAttack = source;
			}

			//_animator?.SetTrigger(ATTACK_TRIGGER);

			await UniTask.Delay(TimeSpan.FromSeconds(_delay));

			if (IsTargetNearby)
			{
				target.TakeDamage(_damage, _pawn.attackSound);
			}

			//Debug.Log("HIT!!!");
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.position, transform.position + transform.forward * _range);
		}
	} 
}