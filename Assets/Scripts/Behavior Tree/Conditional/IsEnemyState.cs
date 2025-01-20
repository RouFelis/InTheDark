using InTheDark.Prototypes;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class IsEnemyState : Conditional
{
	public string state;

	public override TaskStatus OnUpdate()
	{
		var pawn = GetComponent<EnemyPrototypePawn>();
		var isEnemyInState = pawn.State.Equals(state);
		var result = isEnemyInState ? TaskStatus.Success : TaskStatus.Failure;

		return result;
	}
}