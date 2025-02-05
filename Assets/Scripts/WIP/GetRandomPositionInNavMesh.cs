using UnityEngine;
using UnityEngine.AI;

namespace InTheDark.Prototypes
{
	[CreateAssetMenu(fileName = "GetRandomPositionInNavMesh", menuName = "Scriptable Objects/GetRandomPositionInNavMesh")]
	public class GetRandomPositionInNavMesh : PositionGenerator
	{
		[SerializeField]
		private PositionGenerator _target;

		[SerializeField]
		private float _radius;

		[SerializeField]
		private int _maxCount;

		public override Vector3 Generate()
		{
			var result = _target.Generate();
			var isOnNavMesh = false;

			for (var i = 0; i < _maxCount && !isOnNavMesh; i++)
			{
				var direction = Random.insideUnitSphere * _radius;

				isOnNavMesh = NavMesh.SamplePosition(direction, out var hit, _radius, NavMesh.AllAreas);

				if (isOnNavMesh)
				{
					result = hit.position;
				}
			}

			return result;
		}
	}
}