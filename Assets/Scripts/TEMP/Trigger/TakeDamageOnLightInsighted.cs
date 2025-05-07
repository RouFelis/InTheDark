using BehaviorDesigner.Runtime;
using UnityEngine;

namespace InTheDark.Prototypes
{
	[CreateAssetMenu(fileName = "new take damage", menuName = "trigger/insighted/damage")]
	public class TakeDamageOnLightInsighted : EnemyLightInsightedTrigger
	{
		public const string TAUNTED_STATE = "Taunted";

		public override void OnUpdate(EnemyPrototypePawn pawn, SpotLight light)
		{
			//var damage = /*Time.deltaTime * */light.Damage;
			//var resistance = Mathf.Max(pawn.Resistance - damage, 0.0F);
			var behavior = pawn.GetComponent<BehaviorTree>();
			var player = light.GetComponent<Player>();

			pawn.TakeDamage(light.Damage, pawn.hitSound);

			//pawn.Resistance = resistance;

			if (behavior && player && pawn.State.Equals(EnemyPrototypePawn.DEFAULT_STATE))
			{
				pawn.State = TAUNTED_STATE;

				behavior.SetVariableValue("TargetPawn", player);

				//Debug.Log("³ª È­³µ¾û!");
			}
		}
	}
}