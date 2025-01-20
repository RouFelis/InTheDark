using UnityEngine;

namespace InTheDark.Prototypes
{
	public abstract class EnemyDeathTrigger : ScriptableObject
	{
		public abstract void OnUpdate(EnemyPrototypePawn pawn);
	} 
}