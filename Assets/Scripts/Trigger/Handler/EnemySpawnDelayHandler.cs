using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace InTheDark.Prototypes
{
	[Serializable]
	public class SpawningEnemy : IDisposable
	{
		public int BuildIndex;
		public Vector3 Position;
		public Quaternion Rotation;

		public float Time;

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
	}

	public class EnemySpawnDelayHandler : NetworkBehaviour
	{
		[SerializeField]
		private List<SpawningEnemy> _nodes = new();

		private List<SpawningEnemy> _cache = new();

		public override void OnNetworkSpawn()
		{
			UpdateManager.OnUpdate += OnUpdate;
		}

		public override void OnNetworkDespawn()
		{
			UpdateManager.OnUpdate -= OnUpdate;
		}

		private void OnUpdate()
		{
			var time = Time.deltaTime;

			foreach (var node in _nodes)
			{
				node.Time = Mathf.Max(node.Time - time, 0.0F);

				if (node.Time < 0.0F || Mathf.Approximately(node.Time, 0.0F))
				{
					_cache.Add(node);
				}
			}

			foreach (var node in _cache)
			{
				_nodes.Remove(node);
				EnemySpawnCompleteRPC(node.BuildIndex, node.Position, node.Rotation);

				node.Dispose();
			}

			_cache.Clear();
		}

		[Rpc(SendTo.Server)]
		public void StartEnemySpawnRPC(int buildIndex, Vector3 position, Quaternion rotation, float cooldown)
		{
			var node = new SpawningEnemy()
			{
				BuildIndex = buildIndex,
				Position = position,
				Rotation = rotation,

				Time = cooldown
			};

			_nodes.Add(node);
		}

		[Rpc(SendTo.Server)]
		private void EnemySpawnCompleteRPC(int buildIndex, Vector3 position, Quaternion rotation)
		{
			MonsterSpawner.Instance.SpawnEnemyRPC(buildIndex, position, rotation);
		}
	} 
}