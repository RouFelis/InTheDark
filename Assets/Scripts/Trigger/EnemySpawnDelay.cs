using Unity.Netcode;
using UnityEngine;

namespace InTheDark.Prototypes
{
	[CreateAssetMenu(fileName = "new delay spawn", menuName = "trigger/spawn/delay")]
	public class EnemySpawnDelay : EnemySpawnTrigger
	{
		[SerializeField]
		private int _buildIndex;

		[SerializeField]
		private float _cooldown;

		[SerializeField]
		private PositionGenerator _generator;

		[SerializeField]
		private EnemySpawnDelayHandler _handlerPrefab;

		private EnemySpawnDelayHandler _handler;

		public override void OnUpdate()
		{
			SendingSpawnEnemyRPC();
		}

		[Rpc(SendTo.Server)]
		private void SendingSpawnEnemyRPC()
		{
			//var position = MonsterSpawner.Instance.GetRandomPositionInNavMesh();
			var position = _generator.Generate();

			GetHandler().StartEnemySpawnRPC(_buildIndex, position, Quaternion.identity, _cooldown);
		}

		private EnemySpawnDelayHandler GetHandler()
		{
			if (!_handler)
			{
				_handler = FindAnyObjectByType<EnemySpawnDelayHandler>();
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