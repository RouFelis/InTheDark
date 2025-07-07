using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace InTheDark.Prototypes
{
	[Serializable]
	public class SpawningEnemyOnQuestClear : IDisposable
	{
		public int BuildIndex;
		public Vector3 Position;
		public Quaternion Rotation;

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
	}

	public class EnemySpawnOnQuestClearHandler : NetworkBehaviour
    {
		[SerializeField]
		private List<SpawningEnemyOnQuestClear> _nodes = new();

		private void Awake()
		{
			QuestManager.OnQuestComplete -= OnQuestComplete;
			QuestManager.OnQuestComplete += OnQuestComplete;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();

			QuestManager.OnQuestComplete -= OnQuestComplete;
		}

		[Rpc(SendTo.Server)]
		public void SubscribeOnQuestClearedEventRPC(int buildIndex, Vector3 position, Quaternion rotation)
		{
			var node = new SpawningEnemyOnQuestClear()
			{
				BuildIndex = buildIndex,
				Position = position,
				Rotation = rotation
			};

			_nodes.Add(node);
		}

		[Rpc(SendTo.Server)]
		private void OnQuestCompleteRPC()
		{
			foreach (var node in _nodes)
			{
				var buildIndex = node.BuildIndex;
				var position = node.Position;
				var rotation = node.Rotation;

				EnemySpawnRPC(buildIndex, position, rotation);

				node.Dispose();
			}

			if (_nodes.Count > 0)
			{
				_nodes.Clear();
			}
		}

		private void OnQuestComplete(QuestBase quest, int requireQuestCount, int currentQuestCount)
		{
			OnQuestCompleteRPC();
		}

		[Rpc(SendTo.Server)]
		private void EnemySpawnRPC(int buildIndex, Vector3 position, Quaternion rotation)
		{
			MonsterSpawner.Instance.SpawnEnemyRPC(buildIndex, position, rotation);
		}
	} 
}