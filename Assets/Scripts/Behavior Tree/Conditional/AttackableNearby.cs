using BehaviorDesigner.Runtime.Tasks;
using InTheDark.Prototypes;
using UnityEngine;

public class AttackableNearby : Conditional
{
	public float distance;

	public SharedNetworkBehaviour pawn;

	// ���� �þ� ���� Ŭ���� ���� ����� �� ���ڳ� ���� ����������������������
	public override TaskStatus OnUpdate()
	{
		var networkPawn = pawn.Value;

		var isNearBy = networkPawn ? Vector3.Distance(networkPawn.transform.position, transform.position) <= distance : false;
		var isAttackable = isNearBy ? TaskStatus.Success : TaskStatus.Failure;

		return isAttackable;
	}
}