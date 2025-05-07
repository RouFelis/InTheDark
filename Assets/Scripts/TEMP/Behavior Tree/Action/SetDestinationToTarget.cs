using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class SetDestinationToTarget : Action
{
	public SharedVector3 target;
	//public SharedNetworkBehaviour pawn;

	public override TaskStatus OnUpdate()
	{
		var self = gameObject.GetComponent<EnemyPrototypePawn>();
		var result = self.Target ? TaskStatus.Success : TaskStatus.Failure;

		if (self.Target)
		{
			target.Value = self.Target.transform.position;
		}

		return result;
	}
}