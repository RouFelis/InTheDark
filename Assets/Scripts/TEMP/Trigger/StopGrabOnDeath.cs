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
				Debug.Log($"{grabHandler} ¾ê Ã£¾Ò°í µ¹¾Æ°¡´Â°Å ºÃ°í ÀÌÁ¦ ²ô°ÚÀ¾´Ï´ç");

				grabHandler.Detach(pawn.Target);
			}

			pawn.IsDead = true;
		}
	}
}