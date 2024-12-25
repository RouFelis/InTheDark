using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.AI;

namespace InTheDark.Prototypes
{
	// ���� �����̱� ��
	// �̸��� �ӽ���
	[Serializable]
	public class AIGenerateData
	{
		public NetworkObject[] Prefabs;
	}

	// TODO: Enemy Manager ������ �Ϻ� ��� �̵� �� �и�, Spawner�� Factory�� ����
	public class MonsterSpawner : NetworkBehaviour
	{
		[Serializable]
		private struct EnemyRef : IEquatable<EnemyRef>, INetworkSerializable
		{
			public Vector3 Position;
			public Quaternion Quaternion;

			public static implicit operator EnemyRef(EnemyPrototypePawn enemy)
			{
				var enemyRef = new EnemyRef()
				{
					Position = enemy.transform.position,
					Quaternion = enemy.transform.rotation
				};

				return enemyRef;
			}

			public bool Equals(EnemyRef other)
			{
				return Position.Equals(other.Position) && Quaternion.Equals(other.Quaternion);
			}

			public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
			{
				serializer.SerializeValue(ref Position);
				serializer.SerializeValue(ref Quaternion);
			}
		}

		// ���� ���� �ִ� �Ÿ�
		[SerializeField]
		private float _radius;

		[SerializeField]
		private int _count;

		[SerializeField]
		private AIGenerateData[] _stage;

		// ������
		[SerializeField]
		private NetworkObject _enemyPrototypePrefab;

		private NetworkList<EnemyRef> _spawned;

		private void Awake()
		{
			_spawned = new NetworkList<EnemyRef>();

			DontDestroyOnLoad(gameObject);
		}

		public override void OnNetworkSpawn()
		{
			if (NetworkManager.Singleton)
			{
				//NetworkManager.Singleton.SceneManager.OnLoadComplete += (clientID, sceneName, loadSceneMode) =>
				//{
				//	if (sceneName is "GameRoom")
				//	{
				//		Despawn();
				//	}
				//};

				Game.OnDungeonEnter += OnDungeonEnter;
				Game.OnDungeonExit += OnDungeonExit;

				UpdateManager.OnUpdate += OnUpdate;
			}
		}

		public override void OnNetworkDespawn()
		{
			Game.OnDungeonEnter -= OnDungeonEnter;
			Game.OnDungeonExit -= OnDungeonExit;

			UpdateManager.OnUpdate -= OnUpdate;
		}

		private void OnUpdate()
		{
			if (Input.GetKeyDown(KeyCode.V))
			{
				if (IsHost)
				{
					OnHostStarted();
				}

				OnClientStarted();
			}
		}

		// ���� �߰�
		public void OnHostStarted()
		{
			for (var i = 0; i < _count; i++)
			{
				var enemyRef = SpawnRandomRef();

				_spawned.Add(enemyRef);
			}
		}

		// ���� ������Ʈ ����
		public void OnClientStarted()
		{
			foreach (var enemyRef in _spawned)
			{
				Spawn(enemyRef.Position);
			}
		}

		private EnemyRef SpawnRandomRef()
		{
			var enemyRef = new EnemyRef()
			{
				Position = GetRandomPositionInNavMesh(),
				Quaternion = Quaternion.identity
			};

			return enemyRef;
		}

		private void OnDungeonEnter(DungeonEnterEvent received)
		{
			if (IsHost)
			{
				var data = _stage[received.BuildIndex];

				for (var i = 0; i < data.Prefabs.Length; i++)
				{
					var enemyRef = SpawnRandomRef();

					_spawned.Add(enemyRef);
				}
			}

			foreach (var enemyRef in _spawned)
			{
				Spawn(enemyRef.Position);
			}
		}

		private void OnDungeonExit(DungeonExitEvent received)
		{
			if (IsHost)
			{
				_spawned.Clear();
			}
		}

		public void Spawn(Vector3 position)
		{
			var enemy = Instantiate(_enemyPrototypePrefab, position, Quaternion.identity);

			enemy.Spawn(true);

			Debug.Log(enemy.name);
		}

		public void Despawn()
		{
			_spawned.Clear();
		}

		private Vector3 GetRandomPositionInNavMesh()
		{
			var result = Vector3.zero;

			for (var i = 0; i < 30; i++)
			{
				var direction = UnityEngine.Random.insideUnitSphere * _radius;
				var isOnNavMesh = NavMesh.SamplePosition(direction, out var hit, _radius, NavMesh.AllAreas);

				//Debug.Log(direction);
				//Debug.Log($"[{isOnNavMesh}] [{direction} == {hit.position}]");

				if (isOnNavMesh)
				{
					result = hit.position;

					break;
				}
			}

			//while (true)
			//{
			//	var direction = UnityEngine.Random.insideUnitSphere * _radius;
			//	var isOnNavMesh = NavMesh.SamplePosition(direction, out var hit, _radius, NavMesh.AllAreas);

			//	if (isOnNavMesh)
			//	{
			//		result = hit.position;

			//		break;
			//	}
			//}

			return result;
		}
	}

}