using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace InTheDark.Prototypes
{
	[RequireComponent(typeof(NavMeshAgent))]
	[RequireComponent(typeof(EnemyPrototypePawn))]
	public class EnemyNavMeshAgent : NetworkBehaviour
	{
		[SerializeField]
		private NetworkVariable<Vector3> _target = new();

		[SerializeField]
		private NavMeshAgent _agent;

		[SerializeField]
		private EnemyPrototypePawn _pawn;

		[SerializeField]
		private CancellationTokenSource _onMove;

		public bool SetDestination(Vector3 target)
		{
			var result = _agent.SetDestination(target);

			if (result)
			{
				_pawn.StartMove();

				CheckDestinationArrive().Forget();
			}

			return result;
		}

		private async UniTaskVoid CheckDestinationArrive()
		{
			using var source = new CancellationTokenSource();

			var token = source.Token;

			_onMove = source;

			await UniTask.WaitUntil(() => _agent.remainingDistance <= _agent.stoppingDistance, PlayerLoopTiming.Update, token, false);

			// pawn.StartTree;

			_onMove = default;
		}
	} 
}