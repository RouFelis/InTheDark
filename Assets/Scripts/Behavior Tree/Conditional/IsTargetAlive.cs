using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class IsTargetAlive : Conditional
{
	public SharedNetworkBehaviour pawn;

	public override TaskStatus OnUpdate()
	{
		var isTargetAlive = pawn.Value && pawn.Value.gameObject.activeSelf;
		var result = isTargetAlive ? TaskStatus.Success : TaskStatus.Failure;

		return result;
	}
}