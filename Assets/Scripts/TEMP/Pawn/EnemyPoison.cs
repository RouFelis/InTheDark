using Cysharp.Threading.Tasks;

using System.Threading;
using System;

using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using Unity.Collections;

namespace InTheDark.Prototypes
{
    public class EnemyPoison : EnemyWeapon
    {
		private static Action<Player> OnPlayerPoisonHitten;

		private void Awake()
		{
			OnPlayerPoisonHitten += OnPlayerPoisonHit;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();

			OnPlayerPoisonHitten -= OnPlayerPoisonHit;
		}

		protected override async UniTask OnAttack(IHealth target)
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

				OnPlayerPoisonHitten?.Invoke(_pawn.Target);
			}

			//Debug.Log("HIT!!!");
		}

		private void OnPlayerPoisonHit(Player target)
		{
			if (!_pawn.Target)
			{
				OnPlayerTauntedRPC(target);
			}
		}

		[Rpc(SendTo.Server)]
		private void OnPlayerTauntedRPC(NetworkBehaviourReference reference)
		{
			var isBehaviourAttached = reference.TryGet(out Player target);

			var agent = GetComponent<NavMeshAgent>();
			var isEnable = agent.SetDestination(target.transform.position);

			if (isEnable)
			{
				_pawn.Target = target;

				_pawn?.StartMove();
				agent?.SetDestination(target.transform.position);
			}
		}
	}
}