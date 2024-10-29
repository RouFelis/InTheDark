using BehaviorDesigner.Runtime.Tasks;
using InTheDark.Prototypes;
using UnityEngine;

public class AttackableNearby : Conditional
{
	public float distance;

	public SharedNetworkBehaviour pawn;

	// 어후 시야 감지 클래스 따로 만드는 게 낫겠네 차라리 ㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋ
	public override TaskStatus OnUpdate()
	{
		var networkPawn = pawn.Value;

		var isNearBy = networkPawn ? Vector3.Distance(networkPawn.transform.position, transform.position) <= distance : false;
		var isAttackable = isNearBy ? TaskStatus.Success : TaskStatus.Failure;

		return isAttackable;
	}
}