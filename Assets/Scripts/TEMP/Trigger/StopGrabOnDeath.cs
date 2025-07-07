using UnityEngine;

namespace InTheDark.Prototypes
{
	[CreateAssetMenu]
	public class StopGrabOnDeath : EnemyDeathTrigger
	{
		public override void OnUpdate(EnemyPrototypePawn pawn)
		{
			var grabHandler = pawn.GetComponent<MonsterGrab>();

			if (grabHandler && grabHandler.IsActive)
			{
				Debug.Log($"{grabHandler} �� ã�Ұ� ���ư��°� �ð� ���� �������ϴ�");

				grabHandler.Detach(pawn.Target);
			}

			pawn.IsDead = true;
		}
	}
}