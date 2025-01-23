using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class SetEnemyState : Action
{
	public string state;

	public override TaskStatus OnUpdate()
	{
		var pawn = GetComponent<EnemyPrototypePawn>();
		var result = pawn ? TaskStatus.Success : TaskStatus.Failure;

		if (pawn)
		{
			pawn.State = state;
		}

		return result;
	}
}