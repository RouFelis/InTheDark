using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class IsTargetAlive : Conditional
{
	//public SharedNetworkBehaviour pawn;

	public override TaskStatus OnUpdate()
	{
		var self = GetComponent<EnemyPrototypePawn>();
		//var isTargetAlive = pawn.Value && pawn.Value.gameObject.activeSelf;
		var target = self.Target;
		var result = target ? TaskStatus.Success : TaskStatus.Failure;

		return result;
	}
}