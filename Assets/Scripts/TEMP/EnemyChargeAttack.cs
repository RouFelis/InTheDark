using Cysharp.Threading.Tasks;
using InTheDark.Prototypes;
using UnityEngine;

public class EnemyChargeAttack : EnemyWeapon
{
	protected override async UniTask OnAttack(IHealth target)
	{
		transform.LookAt(_pawn.Target.transform);

		Debug.Log("123 456 78");

		await base.OnAttack(target);

		Debug.Log("12 34 56");
	}
}
