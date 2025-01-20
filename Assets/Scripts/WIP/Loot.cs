using UnityEngine;

namespace InTheDark.Prototypes
{
	public abstract class Loot : ScriptableObject
	{
		public abstract void Execute(EnemyPrototypePawn pawn);
	} 
}