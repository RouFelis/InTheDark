using UnityEngine;

namespace InTheDark.Prototypes
{
	[CreateAssetMenu(fileName = "new take damage", menuName = "trigger/insighted/damage")]
	public class TakeDamageOnLightInsighted : EnemyLightInsightedTrigger
	{
		public override void OnUpdate(EnemyPrototypePawn pawn, LightSource light)
		{
			var damage = Time.deltaTime * light.DamagePercent;
			var resistance = Mathf.Max(pawn.Resistance - damage, 0.0F);

			pawn.Resistance = resistance;
		}
	}
}