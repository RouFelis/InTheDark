using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class SetDestinationToTarget : Action
{
	public SharedVector3 target;
	public SharedNetworkBehaviour pawn;

	public override TaskStatus OnUpdate()
	{
		var result = pawn.Value ? TaskStatus.Success : TaskStatus.Failure;

		if (pawn.Value)
		{
			target.Value = pawn.Value.transform.position;
		}

		return result;
	}
}