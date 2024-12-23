using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIBox : MonoBehaviour
{
    [SerializeField]
    private float _radius;

    [SerializeField]
    private NavMeshAgent _agent;

	private void Start()
    {
        if (!_agent)
        {
			_agent = GetComponent<NavMeshAgent>();
		}
	}

	private void Update()
	{
        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            SetDestination();
        }
    }

	private void SetDestination()
    {
        var isOnNavMesh = false;
        var count = 0;

        do
        {
			var random = Random.insideUnitSphere * _radius + transform.position;

			isOnNavMesh = NavMesh.SamplePosition(random, out var hit, _radius * _radius, NavMesh.AllAreas);

            if (isOnNavMesh)
            {
                _agent.SetDestination(hit.position);
            }
        }
        while (!isOnNavMesh && count < 100);

        if (count is 100)
        {
            Debug.LogError("카운트 100회 초과!!!!");
        }
	}
}
