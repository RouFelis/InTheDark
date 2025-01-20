using UnityEngine;
using UnityEngine.UIElements;

namespace InTheDark.Prototypes
{
	[CreateAssetMenu(fileName = "new immediate spawn", menuName = "trigger/spawn/immediate")]
	public class EnemySpawnImmediate : EnemySpawnTrigger
	{
		[SerializeField]
		private int _buildIndex;

		public override void OnUpdate()
		{
			MonsterSpawner.Instance.SpawnEnemyRPC(_buildIndex, MonsterSpawner.Instance.GetRandomPositionInNavMesh(), Quaternion.identity);
		}
	}
}