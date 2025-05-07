using Unity.Netcode;
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
			SendingSpawnEnemyRPC();
		}

		[Rpc(SendTo.Server)]
		private void SendingSpawnEnemyRPC()
		{
			var position = MonsterSpawner.Instance.GetRandomPositionInNavMesh();

			MonsterSpawner.Instance.SpawnEnemyRPC(_buildIndex, position, Quaternion.identity);
		}
	}
}