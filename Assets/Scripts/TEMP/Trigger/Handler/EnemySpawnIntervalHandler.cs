using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace InTheDark.Prototypes
{
	[Serializable]
	public class SpawningEnemyInterval : IDisposable
	{
		public int BuildIndex;
		public Vector3 Position;
		public Quaternion Rotation;

		public int Count;

		public float Time;

		public float InitialTime;

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
	}

	public class EnemySpawnIntervalHandler : NetworkBehaviour
    {
		[SerializeField]
		private List<SpawningEnemyInterval> _nodes = new();

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
					EnemySpawnRPC(node.BuildIndex, node.Position, node.Rotation);

					node.Count += 1;

					node.Time = node.InitialTime;
				}
			}
		}

		[Rpc(SendTo.Server)]
		public void StartEnemySpawnRPC(int buildIndex, Vector3 position, Quaternion rotation, float cooldown)
		{
			var node = new SpawningEnemyInterval()
			{
				BuildIndex = buildIndex,
				Position = position,
				Rotation = rotation,

				Time = cooldown,

				InitialTime = cooldown
			};

			_nodes.Add(node);
		}

		[Rpc(SendTo.Server)]
		private void EnemySpawnRPC(int buildIndex, Vector3 position, Quaternion rotation)
		{
			MonsterSpawner.Instance.SpawnEnemyRPC(buildIndex, position, rotation);
		}
	} 
}