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
	} 
}