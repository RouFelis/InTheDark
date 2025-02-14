using BehaviorDesigner.Runtime;
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
			var behavior = pawn.GetComponent<BehaviorTree>();
			var player = light.GetComponent<Player>();

			pawn.Resistance = resistance;

			if (behavior && player)
			{
				behavior.SetVariableValue("TargetPawn", player);

				//Debug.Log("³ª È­³µ¾û!");
			}
		}
	}
}