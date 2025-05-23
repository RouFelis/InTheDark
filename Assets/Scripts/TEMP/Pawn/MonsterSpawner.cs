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
		public EnemySpawnTrigger[] Triggers;
	}

	// TODO: Enemy Manager 등으로 일부 기능 이동 및 분리, Spawner는 Factory의 역할
	public class MonsterSpawner : NetworkBehaviour
	{
		// 뭔가 필요없는 데이터가 되버린 느낌
		[Serializable]
		private struct EnemyRef : IEquatable<EnemyRef>, INetworkSerializable
		{
			public int BuildIndex;

			public bool Equals(EnemyRef other)
			{
				return BuildIndex.Equals(other.BuildIndex);
			}

			public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
			{
				serializer.SerializeValue(ref BuildIndex);
			}
		}

		private static MonsterSpawner _instance;

		// 몬스터 생성 최대 거리
		[SerializeField]
		private float _radius;

		[SerializeField]
		private AIGenerateData[] _stage;

		[SerializeField]
		private EnemyPrototypePawn[] _prefabs;

		[SerializeField]
		private float _agentHeight = 1.0F;

		private NetworkList<EnemyRef> _spawned;

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

			DontDestroyOnLoad(gameObject);
		}

		//private void Update()
		//{
		//	var target = GetRandomPositionInNavMesh();

		//	Debug.Log($"현재 랜덤으로 받아오는 좌표는 {target}야...");
		//}

		public override void OnNetworkSpawn()
		{
			if (NetworkManager.Singleton)
			{
				Game.OnDungeonEnter += OnDungeonEnter;
				Game.OnDungeonExit += OnDungeonExit;

				if (_isLocked.Value)
				{
					//Debug.Log("jdhfjdsfjdhfj");

					foreach (var enemyRef in _spawned)
					{
						SpawnInternal(enemyRef, GetRandomPositionInNavMesh(), Quaternion.identity);
					}
				}
			}
		}

		public override void OnNetworkDespawn()
		{
			Game.OnDungeonEnter -= OnDungeonEnter;
			Game.OnDungeonExit -= OnDungeonExit;
		}

		[Rpc(SendTo.Server)]
		public void SpawnEnemyRPC(int buildIndex, Vector3 position, Quaternion rotation)
		{
			var enemyRef = new EnemyRef()
			{
				BuildIndex = buildIndex
			};

			//SpawnEnemyServerRPC(enemyRef);
			//SpawnEnemyClientRPC(enemyRef, position, rotation);

			SpawnEnemyServerRPC(enemyRef, position, rotation);
		}

		//[Rpc(SendTo.Server)]
		//private void SpawnEnemyServerRPC(EnemyRef enemyRef)
		//{
		//	_spawned.Add(enemyRef);
		//}

		[Rpc(SendTo.Server)]
		private void SpawnEnemyServerRPC(EnemyRef enemyRef, Vector3 position, Quaternion rotation)
		{
			_spawned.Add(enemyRef);
			SpawnInternal(enemyRef, position, rotation);
		}

		//[Rpc(SendTo.Everyone)]
		//private void SpawnEnemyClientRPC(EnemyRef enemyRef, Vector3 position, Quaternion rotation)
		//{
		//	//Debug.Log("ytyeuhvn"); ;

		//	SpawnInternal(enemyRef, position, rotation);
		//}

		//private EnemyRef SpawnRandomRef()
		//{
		//	var enemyRef = new EnemyRef()
		//	{
		//		Position = GetRandomPositionInNavMesh(),
		//		Quaternion = Quaternion.identity
		//	};

		//	return enemyRef;
		//}

		private void OnDungeonEnter(DungeonEnterEvent received)
		{
			OnDungeonEnterRPC(received.BuildIndex);
		}

		[Rpc(SendTo.Server)]
		private void OnDungeonEnterRPC(int buildIndex)
		{
			if (!_isLocked.Value)
			{
				var data = _stage[buildIndex];

				_isLocked.Value = true;

				//for (var i = 0; i < data.Triggers.Length; i++)
				//{
				//	var enemyRef = SpawnRandomRef();

				//	//enemyRef.DataIndex = buildIndex;
				//	//enemyRef.MonsterIndex = i;

				//	_spawned.Add(enemyRef);
				//}

				foreach (var trigger in data.Triggers)
				{
					//SpawnEnemyRPC(index, GetRandomPositionInNavMesh(), Quaternion.identity);
					trigger.OnUpdate();
				}

				//SpawnEnemyInDungeonRpc();
			}
		}

		private void SpawnInternal(EnemyRef enemyRef, Vector3 position, Quaternion rotation)
		{
			var prefab = _prefabs[enemyRef.BuildIndex];
			var enemy = Instantiate(prefab, position, rotation);
			var agent = enemy.GetComponent<NavMeshAgent>();
			var height = UnityEngine.Random.Range(0.1F, 1.0F);

			//Debug.LogError(_agentHeight + "dnfjdnfjdnjfndsjfndsjnk");

			agent.height = _agentHeight;

			_agentHeight += height;

			if (IsServer && !enemy.IsSpawned)
			{
				enemy.NetworkObject.Spawn(true);
			}

			//enemy.NetworkObject.Spawn(true);
			//OnEnemyPawnSpawnRPC(enemy);
		}

		//[Rpc(SendTo.Server)]
		//private void OnEnemyPawnSpawnRPC(NetworkBehaviourReference reference)
		//{
		//	Debug.Log($"{reference}...");

		//	if (reference.TryGet<EnemyPrototypePawn>(out var enemy) && !enemy.IsSpawned)
		//	{
		//		enemy.NetworkObject.Spawn(true);
		//	}
		//}

		private void OnDungeonExit(DungeonExitEvent received)
		{
			if (IsHost)
			{
				_spawned.Clear();
			}
		}

		public Vector3 GetRandomPositionInNavMesh()
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

					//Debug.Log($"{i}번 시도 차에 {hit.position} 목표 설정 - {gameObject}");
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