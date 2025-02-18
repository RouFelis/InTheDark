using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace InTheDark.Prototypes
{
	[RequireComponent(typeof(NavMeshAgent))]
	public class EnemyNavMeshAgent : NetworkBehaviour
	{
		[SerializeField]
		private NavMeshAgent _agent;
	} 
}