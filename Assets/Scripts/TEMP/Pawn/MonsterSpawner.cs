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
		public EnemySpawnTrigger[] Triggers;
	}

	// �ӽ��� �ӽ�
	[Serializable]
	public class AIGenerateTable
	{
		public float Weight;
		public bool IsEnable;

		public int[] Index;
	}

	// TODO: Enemy Manager ������ �Ϻ� ��� �̵� �� �и�, Spawner�� Factory�� ����
	public class MonsterSpawner : NetworkBehaviour
	{
		// ���� �ʿ���� �����Ͱ� �ǹ��� ����
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

		// ���� ���� �ִ� �Ÿ�
		[SerializeField]
		private float _radius;

		// �� = ���� ���� �� ����
		[SerializeField]
		private float _logBase = Mathf.Sqrt(2.0F);

		[SerializeField]
		private float _agentHeight = 1.0F;

		[SerializeField]
		private List<EnemyPrototypePawn> _enemies = new();

		[SerializeField]
		private AIGenerateData[] _stage;

		[SerializeField]
		private AIGenerateTable[] _table;

		[SerializeField]
		private EnemyPrototypePawn[] _prefabs;

		private NetworkList<EnemyRef> _spawned;

		private NetworkVariable<bool> _isLocked = new NetworkVariable<bool>(false);

		private List<AIGenerateTable> _enabled = new();

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

		//	Debug.Log($"���� �������� �޾ƿ��� ��ǥ�� {target}��...");
		//}

		public override void OnNetworkSpawn()
		{
			if (NetworkManager.Singleton)
			{
				Game.OnDungeonEnter += OnDungeonEnter;
				Game.OnDungeonExit += OnDungeonExit;

				//if (_isLocked.Value)
				//{
				//	//Debug.Log("jdhfjdsfjdhfj");

				//	foreach (var enemyRef in _spawned)
				//	{
				//		SpawnInternal(enemyRef, GetRandomPositionInNavMesh(), Quaternion.identity);
				//	}
				//}



				//EnemyPrototypePawn.OnEnemyDieWithPlayer += skjdaksjd;
			}
		}

		public override void OnNetworkDespawn()
		{
			Game.OnDungeonEnter -= OnDungeonEnter;
			Game.OnDungeonExit -= OnDungeonExit;

			//EnemyPrototypePawn.OnEnemyDieWithPlayer -= skjdaksjd;
		}

		//private void skjdaksjd()
		//{
		//	Debug.Log("���� �ֱ�");
		//}

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
			var table = Select();
			var area = buildIndex + 1;

			if (!_isLocked.Value)
			{
				Debug.Log(area + "�� ����");

				var temp = 1 + Mathf.Log(area, _logBase);
				var max = Mathf.Floor(temp);

				for (var i = 0; i < max; i++)
				{
					//var index = UnityEngine.Random.Range(0, _stage.Length);
					//var data = _stage[index];

					var index = UnityEngine.Random.Range(0, table.Length);
					var data = table[index];

					Debug.Log(index + "�� ���� ����");

					foreach (var trigger in data.Triggers)
					{
						//SpawnEnemyRPC(index, GetRandomPositionInNavMesh(), Quaternion.identity);
						trigger.OnUpdate();
					}
				}

				//var data = _stage[buildIndex];

				//_isLocked.Value = true;

				//for (var i = 0; i < data.Triggers.Length; i++)
				//{
				//	var enemyRef = SpawnRandomRef();

				//	//enemyRef.DataIndex = buildIndex;
				//	//enemyRef.MonsterIndex = i;

				//	_spawned.Add(enemyRef);
				//}

				//foreach (var trigger in data.Triggers)
				//{
				//	//SpawnEnemyRPC(index, GetRandomPositionInNavMesh(), Quaternion.identity);
				//	trigger.OnUpdate();
				//}

				//SpawnEnemyInDungeonRpc();
			}
		}

		private AIGenerateData[] Select()
		{
			var max = 0.0F;
			var value = 0.0F;

			foreach (var element in _table)
			{
				if (element.IsEnable)
				{
					max += element.Weight;

					_enabled.Add(element);
				}
			}

			value = UnityEngine.Random.Range(0.0F, max);

			foreach (var element in _enabled)
			{
				if (value <= element.Weight)
				{
					//return element.Stages;

					var length = element.Index.Length;
					var result = new AIGenerateData[length];

					for (var i = 0; i < length; i++)
					{
						var index = element.Index[i];

						result[i] = _stage[index];
					}

					return result;
				}
				else
				{
					value -= element.Weight;
				}
			}

			Debug.LogError("������ �ȵǴµ���...");

			return new AIGenerateData[0];
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

			if (IsServer)
			{
				_enemies.Add(enemy);

				if (!enemy.IsSpawned)
				{
					enemy.NetworkObject.Spawn(true);
				}
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
			if (IsServer)
			{
				_spawned.Clear();
			}

			if (IsServer)
			{
				foreach (var enemy in _enemies)
				{
					if (enemy)
					{
						enemy.NetworkObject.Despawn(true);
					}
				}

				_enemies.Clear();
			}
		}

		public Vector3 GetRandomPositionInNavMesh()
		{
			var result = Vector3.zero;
			var isOnNavMesh = false;

			for (var i = 0; i < 300 && !isOnNavMesh; i++)
			{
				var direction = UnityEngine.Random.insideUnitSphere * _radius;

				isOnNavMesh = NavMesh.SamplePosition(direction, out var hit, _radius, NavMesh.AllAreas);

				//Debug.Log(direction);
				//Debug.Log($"[{isOnNavMesh}] [{direction} == {hit.position}]");

				if (isOnNavMesh)
				{
					result = hit.position;

					//Debug.Log($"{i}�� �õ� ���� {hit.position} ��ǥ ���� - {gameObject}");

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