using UnityEngine;

namespace InTheDark.Prototypes
{
	public abstract class EnemyLightInsightedTrigger : ScriptableObject
	{
		public abstract void OnUpdate(EnemyPrototypePawn pawn, SpotLight light);
	} 
}