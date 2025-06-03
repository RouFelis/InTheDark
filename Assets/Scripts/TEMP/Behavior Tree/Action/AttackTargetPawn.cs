using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using InTheDark.Prototypes;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

public class AttackTargetPawn : Action
{
	//public SharedNetworkBehaviour pawn;

	private EnemyPrototypePawn _self;
	//private NavMeshAgent _agent;

	private EnemyWeapon _weapon;

	public override void OnAwake()
	{
		_self = gameObject.GetComponent<EnemyPrototypePawn>();
		_weapon = gameObject.GetComponent<EnemyWeapon>();
		//_agent = gameObject.GetComponent<NavMeshAgent>();
	}

	public override TaskStatus OnUpdate()
	{
		var target = _self.Target;

		//if (!_agent.isStopped)
		//{
		//	_agent?.ResetPath();
		//}

		// ㅁㄴㅇㄹ
		_self.StopMove();

		// 대충 공격하는 스크립트
		if (target != null)
		{
			//transform.LookAt(target.FirstPersonCamera.transform);

			//_self.AttackPrototype(target);
			_weapon.Attack().Forget();
		}

		return TaskStatus.Success;
	}
}
