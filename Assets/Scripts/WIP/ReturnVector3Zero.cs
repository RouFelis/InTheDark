using UnityEngine;

namespace InTheDark.Prototypes
{
	[CreateAssetMenu(fileName = "ReturnVector3Zero", menuName = "Scriptable Objects/ReturnVector3Zero")]
	public class ReturnVector3Zero : PositionGenerator
	{
		public override Vector3 Generate()
		{
			return Vector3.zero;
		}
	}
}