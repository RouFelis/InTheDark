using UnityEngine;

namespace InTheDark.Prototypes
{
	public abstract class PositionGenerator : ScriptableObject
	{
		public abstract Vector3 Generate();
	} 
}