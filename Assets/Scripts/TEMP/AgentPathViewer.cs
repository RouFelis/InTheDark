using UnityEngine;
using UnityEngine.AI;

public class AgentPathViewer : MonoBehaviour
{
	[SerializeField]
	private float _sensitivity = 100.0F;

	[SerializeField]
	private float _weight;

	[SerializeField]
	private Vector3 _destination;

	[SerializeField]
	private NavMeshAgent _agent;

	[SerializeField]
	private EnemyPrototypePawn _pawn;

	//[SerializeField]
	//private Vector3[] _corners;

	//[SerializeField]
	//private Vector3 _nextDestination;

	// Update is called once per frame
	private void Update()
	{
		if (_pawn.Target)
		{
			var distance = Vector3.Distance(_agent.destination, _pawn.Target.transform.position);

			_weight += Mathf.Max(distance, _sensitivity);

			if (_weight >= _sensitivity)
			{
				var isOnNavMesh = NavMesh.SamplePosition(_pawn.Target.transform.position, out var hit, 1.0F, NavMesh.AllAreas);

				_weight -= _sensitivity;

				_agent.SetDestination(isOnNavMesh ? _pawn.Target.transform.position : hit.position);
			}
		}
	}
}
