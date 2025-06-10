using BehaviorDesigner.Runtime.Tasks;
using InTheDark.Prototypes;
using System;
using UnityEngine;

public class CanGrabPlayer : Conditional
{
	private MonsterGrab _skill;

	public override void OnAwake()
	{
		_skill = GetComponent<MonsterGrab>();
	}

	public override TaskStatus OnUpdate()
	{
		var result = _skill && _skill.IsEnable ? TaskStatus.Success : TaskStatus.Failure;

		return result;
	}
}
