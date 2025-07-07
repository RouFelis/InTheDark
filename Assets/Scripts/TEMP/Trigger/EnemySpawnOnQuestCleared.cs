using Unity.Netcode;
using UnityEngine;

namespace InTheDark.Prototypes
{
	[CreateAssetMenu(fileName = "new on quest cleared spawn", menuName = "trigger/spawn/OnQuestCleared")]
	public class EnemySpawnOnQuestCleared : EnemySpawnTrigger
	{
		[SerializeField]
		private int _buildIndex;

		[SerializeField]
		private EnemySpawnOnQuestClearHandler _handlerPrefab;

		private EnemySpawnOnQuestClearHandler _handler;

		public override void OnUpdate()
		{
			SendingSpawnEnemyRPC();
		}

		[Rpc(SendTo.Server)]
		private void SendingSpawnEnemyRPC()
		{
			//var position = MonsterSpawner.Instance.GetRandomPositionInNavMesh();
			var position = MonsterSpawner.Instance.GetRandomPositionInNavMesh();

			GetHandler().SubscribeOnQuestClearedEventRPC(_buildIndex, position, Quaternion.identity);
		}

		private EnemySpawnOnQuestClearHandler GetHandler()
		{
			if (!_handler)
			{
				_handler = FindAnyObjectByType<EnemySpawnOnQuestClearHandler>();
			}

			if (!_handler)
			{
				_handler = Instantiate(_handlerPrefab);

				_handler.NetworkObject.Spawn();

				DontDestroyOnLoad(_handler.gameObject);
			}

			return _handler;
		}
	}
}