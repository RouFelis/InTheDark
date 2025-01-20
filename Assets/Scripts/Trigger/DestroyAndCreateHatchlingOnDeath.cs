using UnityEngine;
using UnityEngine.AI;

namespace InTheDark.Prototypes
{
	[CreateAssetMenu(fileName = "new hatchling", menuName = "trigger/death/hatchling")]
	public class DestroyAndCreateHatchlingOnDeath : EnemyDeathTrigger
	{
		[SerializeField]
		private float _radius;

		[SerializeField]
		private int[] _buildIndex;

		public override void OnUpdate(EnemyPrototypePawn pawn)
		{
			pawn.IsDead = true;

			foreach (var buildIndex in _buildIndex)
			{
				var position = pawn.transform.position;
				var isOnNavMesh = false;

				for (var i = 0; i < 30 && !isOnNavMesh; i++)
				{
					var direction = Random.insideUnitSphere * _radius;

					isOnNavMesh = NavMesh.SamplePosition(direction, out var hit, _radius, NavMesh.AllAreas);

					if (isOnNavMesh)
					{
						position = hit.position;

						MonsterSpawner.Instance.SpawnEnemyRPC(buildIndex, position, Quaternion.identity);
					}
				}

				if (!isOnNavMesh)
				{
					Debug.LogError("아니 왜 생성 안됨?");
				}

				//MonsterSpawner.Instance.SpawnEnemyRPC(buildIndex, position, Quaternion.identity);
			}
		}
	}
}