using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

public class AttackTargetPawn : Action
{
	public SharedNetworkBehaviour pawn;

	private EnemyPrototypePawn _self;
	private NavMeshAgent _agent;

	public override void OnAwake()
	{
		_self = gameObject.GetComponent<EnemyPrototypePawn>();
		_agent = gameObject.GetComponent<NavMeshAgent>();
	}

	public override TaskStatus OnUpdate()
	{
		var target = pawn.Value.GetComponent<IHealth>();

		//if (!_agent.isStopped)
		//{
		//	_agent?.ResetPath();
		//}

		_agent?.ResetPath();

		// 대충 공격하는 스크립트
		if (target != null)
		{
			_self.AttackPrototype(target);
		}

		return TaskStatus.Success;
	}
}
