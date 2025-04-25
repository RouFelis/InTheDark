using BehaviorDesigner.Runtime.Tasks;
using InTheDark.Prototypes;
using UnityEngine;

public class ClearPawnTarget : Action
{
	private EnemyPrototypePawn _pawn;
	
	public override void OnAwake()
	{
		_pawn = GetComponent<EnemyPrototypePawn>();
	}

	public override TaskStatus OnUpdate()
	{
		_pawn.Target = default;

		return TaskStatus.Success;
	}
}
