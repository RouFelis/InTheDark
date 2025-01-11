using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.AI;

namespace InTheDark.Prototypes
{
	// 구현 예정이긴 함
	// 이름도 임시임
	[Serializable]
	public class AIGenerateData
	{
		public NetworkObject[] Prefabs;
	}

	// TODO: Enemy Manager 등으로 일부 기능 이동 및 분리, Spawner는 Factory의 역할
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

		private static MonsterSpawner _instance;

		// 몬스터 생성 최대 거리
		[SerializeField]
		private float _radius;

		[SerializeField]
		private int _count;

		[SerializeField]
		private AIGenerateData[] _stage;

		// 프리팹
		[SerializeField]
		private NetworkObject _enemyPrototypePrefab;

		private NetworkList<EnemyRef> _spawned;
		private NetworkList<int> _enemyCount;

		private NetworkVariable<bool> _isLocked = new NetworkVariable<bool>(false);

		public static MonsterSpawner Instance
		{
			get
			{
				return _instance;
			}

			private set
			{
				_instance = value;
			}
		}

		static MonsterSpawner()
		{
			_instance = default;
		}

		private void Awake()
		{
			_instance = this;
			_spawned = new NetworkList<EnemyRef>();
			_enemyCount = new NetworkList<int>();

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

				//UpdateManager.OnUpdate += OnUpdate;
			}
		}

		public override void OnNetworkDespawn()
		{
			Game.OnDungeonEnter -= OnDungeonEnter;
			Game.OnDungeonExit -= OnDungeonExit;

			//UpdateManager.OnUpdate -= OnUpdate;
		}

		//private void OnUpdate()
		//{
		//	if (Input.GetKeyDown(KeyCode.V))
		//	{
		//		if (IsHost)
		//		{
		//			OnHostStarted();
		//		}

		//		OnClientStarted();
		//	}
		//}

		// 정보 추가
		public void OnHostStarted()
		{
			for (var i = 0; i < _count; i++)
			{
				var enemyRef = SpawnRandomRef();

				_spawned.Add(enemyRef);
			}
		}

		// 실제 오브젝트 생성
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
			OnDungeonEnterRpc(received.BuildIndex);
		}

		[Rpc(SendTo.Server)]
		private void OnDungeonEnterRpc(int buildIndex)
		{
			if (!_isLocked.Value)
			{
				var data = _stage[buildIndex];

				_isLocked.Value = true;

				for (var i = 0; i < data.Prefabs.Length; i++)
				{
					var enemyRef = SpawnRandomRef();

					_spawned.Add(enemyRef);
				}

				SpawnEnemyInDungeonRpc();
			}
		}

		private void OnDungeonExit(DungeonExitEvent received)
		{
			if (IsHost)
			{
				_spawned.Clear();
			}
		}

		[Rpc(SendTo.Everyone)]
		private void SpawnEnemyInDungeonRpc()
		{
			foreach (var enemyRef in _spawned)
			{
				Spawn(enemyRef.Position);
			}
		}

		public void Spawn(Vector3 position)
		{
			var enemy = Instantiate(_enemyPrototypePrefab, position, Quaternion.identity);
			var pawn = enemy.GetComponent<EnemyPrototypePawn>();

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
			var isOnNavMesh = false;

			for (var i = 0; i < 30 && !isOnNavMesh; i++)
			{
				var direction = UnityEngine.Random.insideUnitSphere * _radius;

				isOnNavMesh = NavMesh.SamplePosition(direction, out var hit, _radius, NavMesh.AllAreas);

				//Debug.Log(direction);
				//Debug.Log($"[{isOnNavMesh}] [{direction} == {hit.position}]");

				if (isOnNavMesh)
				{
					result = hit.position;
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