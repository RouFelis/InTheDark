using Cysharp.Threading.Tasks;

using System.Threading;
using System;

using UnityEngine;
using UnityEngine.AI;

using Unity.Netcode;

namespace InTheDark.Prototypes
{
    public class EnemyPoison : EnemyWeapon
    {
		private static Action<Player> OnPlayerPoisonHitten;
		private static Action OnMonsterMinionSpawned;

		[SerializeField]
		private NetworkVariable<bool> _isSpawned = new();

		[SerializeField]
		private float _radius;

		[SerializeField]
		private int[] _buildIndex;

		private void Awake()
		{
			OnPlayerPoisonHitten += OnPlayerPoisonHit;
			OnMonsterMinionSpawned += OnMonsterMinionSpawn;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();

			OnPlayerPoisonHitten -= OnPlayerPoisonHit;
			OnMonsterMinionSpawned -= OnMonsterMinionSpawn;
		}

		protected override async UniTask OnAttack(IHealth target)
		{
			//using var source = new CancellationTokenSource();

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

				OnPlayerPoisonHitten?.Invoke(_pawn.Target);

				if (IsServer && !_isSpawned.Value)
				{
					SpawnMonsterMinionRPC();
				}
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

		private void OnMonsterMinionSpawn()
		{
			//_isSpawned.Value = true;
		}

		[Rpc(SendTo.Server)]
		private void SpawnMonsterMinionRPC()
		{
			//OnMonsterMinionSpawned?.Invoke();

			_isSpawned.Value = true;

			//foreach (var trigger in _minions)
			//{
			//	trigger?.OnUpdate();
			//}

			foreach (var buildIndex in _buildIndex)
			{
				var position = transform.position;
				var isOnNavMesh = false;

				for (var i = 0; i < 30 && !isOnNavMesh; i++)
				{
					var direction = UnityEngine.Random.insideUnitSphere * _radius + position;

					isOnNavMesh = NavMesh.SamplePosition(direction, out var hit, _radius, NavMesh.AllAreas);

					if (isOnNavMesh)
					{
						position = hit.position;

						MonsterSpawner.Instance.SpawnEnemyRPC(buildIndex, position, Quaternion.identity);
					}
				}

				if (!isOnNavMesh)
				{
					Debug.LogError("아니 왜 생성 안됨?");
				}
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
				Debug.Log($"player: {target}, position: {target.transform.position} 플레이어야... {name}.{GetInstanceID()} 왔으면 인사좀 해라...");

				_pawn.Target = target;

				//_pawn?.StopMove();
				_pawn?.StartMove();

				Debug.Log($"{name}.{GetInstanceID()} destination: {agent.destination}");

				//agent?.SetDestination(target.transform.position);
			}
		}
	}
}