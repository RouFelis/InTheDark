using BehaviorDesigner.Runtime.Tasks;
using InTheDark.Prototypes;
using System;
using UnityEngine;

public class CanActivateSkill : Conditional
{
	//private EnemyPrototypePawn _pawn;
	private ChargeSkillManager _skillHandler;

	public override void OnAwake()
	{
		//_pawn = GetComponent<EnemyPrototypePawn>();
		_skillHandler = GetComponent<ChargeSkillManager>();
	}

	public override TaskStatus OnUpdate()
	{
		var result = _skillHandler && _skillHandler.IsEnable ? TaskStatus.Success : TaskStatus.Failure;

		return result;
	}
}