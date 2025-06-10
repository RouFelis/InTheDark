using BehaviorDesigner.Runtime.Tasks;
using InTheDark.Prototypes;
using System.Collections;

using UnityEngine;

public class CatchPlayer : Action
{
	private EnemyPrototypePawn _pawn;
	private MonsterGrab _skill;

	public override void OnAwake()
	{
		_pawn = GetComponent<EnemyPrototypePawn>();
		_skill = GetComponent<MonsterGrab>();
	}

	public override TaskStatus OnUpdate()
	{
		_skill?.Attach(_pawn.Target);

		return TaskStatus.Success;
	}
}
