using BehaviorDesigner.Runtime;
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

		public override void OnUpdate(EnemyPrototypePawn pawn, LightSource light)
		{
			if (_limitation < light.DamagePercent)
			{
				var damage = Time.deltaTime * light.DamagePercent;
				var resistance = Mathf.Max(pawn.Resistance - damage, 0.0F);
				var agent = pawn.GetComponent<NavMeshAgent>();

				pawn.Resistance = resistance;
				pawn.State = ENRAGE_STATE;

				agent.speed = 11.25F;
			}
		}
	}
}