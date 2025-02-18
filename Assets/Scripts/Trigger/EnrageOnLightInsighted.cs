using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using UnityEngine;
using UnityEngine.AI;

namespace InTheDark.Prototypes
{
	[CreateAssetMenu(fileName = "new enrage", menuName = "trigger/insighted/enrage")]
	public class EnrageOnLightInsighted : EnemyLightInsightedTrigger
	{
		public const string ENRAGE_STATE = "Enrage";

		[SerializeField]
		private float _limitation;

		public override void OnUpdate(EnemyPrototypePawn pawn, SpotLight light)
		{
			if (_limitation < light.Damage)
			{
				//var damage =/* Time.deltaTime * */light.Damage;
				//var resistance = Mathf.Max(pawn.Resistance - damage, 0.0F);
				var agent = pawn.GetComponent<NavMeshAgent>();
				var behavior = pawn.GetComponent<BehaviorTree>();
				var player = light.GetComponent<Player>();

				pawn.TakeDamage(light.Damage, pawn.hitSound);

				//pawn.Resistance = resistance;
				pawn.State = ENRAGE_STATE;

				if (behavior && player)
				{
					behavior.SetVariableValue("TargetPawn", player);

					//Debug.Log("³ª È­³µ¾û!");
				}

				agent.speed = 11.25F;
			}
		}
	}
}