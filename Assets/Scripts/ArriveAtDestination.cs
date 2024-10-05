using BehaviorDesigner.Runtime.Tasks;

using UnityEngine.AI;

public class ArriveAtDestination : Conditional
{
	public float distance;

	private NavMeshAgent _agent;

	public override void OnAwake()
	{
		_agent = gameObject.GetComponent<NavMeshAgent>();
	}

	public override TaskStatus OnUpdate()
	{
		return _agent.remainingDistance < distance ? TaskStatus.Success : TaskStatus.Failure;
	}
}