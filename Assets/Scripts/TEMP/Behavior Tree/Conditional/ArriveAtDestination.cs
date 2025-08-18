using BehaviorDesigner.Runtime.Tasks;

using UnityEngine.AI;

public class ArriveAtDestination : Conditional
{
	public float distance;

	private NavMeshAgent _agent;
	private EnemyPrototypePawn pawn;

	public override void OnAwake()
	{
		_agent = GetComponent<NavMeshAgent>();
		pawn = GetComponent<EnemyPrototypePawn>();
	}

	public override TaskStatus OnUpdate()
	{
		//return _agent.remainingDistance < distance ? TaskStatus.Success : TaskStatus.Failure;
		return pawn.IsMoving ? TaskStatus.Failure : TaskStatus.Success;
	}
}