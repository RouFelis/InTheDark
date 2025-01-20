using UnityEngine;

namespace InTheDark.Prototypes
{
	[CreateAssetMenu(fileName = "new remover", menuName = "trigger/death/remove")]
	public class RemoveOnDeath : EnemyDeathTrigger
	{
		public override void OnUpdate(EnemyPrototypePawn pawn)
		{
			pawn.IsDead = true;
		}
	}
}