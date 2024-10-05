using InTheDark;
using InTheDark.Prototypes;

using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.AI;

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

    // 몬스터 생성 최대 거리
    [SerializeField]
    private float _radius;

	[SerializeField]
	private int _count;

	private NetworkList<EnemyRef> _spawned = new NetworkList<EnemyRef>();

    // 프리팹
    [SerializeField]
    private NetworkObject _enemyPrototypePrefab;

	public override void OnNetworkSpawn()
	{
		if (NetworkManager.Singleton)
		{
			NetworkManager.Singleton.OnServerStarted += OnHostStarted;

			NetworkManager.Singleton.OnClientStarted += OnClientStarted;
			NetworkManager.Singleton.OnClientStopped += Despawn;
		}
	}

	public override void OnNetworkDespawn()
	{
		if (NetworkManager.Singleton)
		{
			NetworkManager.Singleton.OnServerStarted -= OnHostStarted;

			NetworkManager.Singleton.OnClientStarted -= OnClientStarted;
			NetworkManager.Singleton.OnClientStopped -= Despawn;
		}
	}

	public void OnHostStarted()
	{
		for (var i = 0; i < _count; i++)
		{
			var enemyRef = SpawnRandomRef();

			_spawned.Add(enemyRef);
		}
	}

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

	public void Spawn(Vector3 position)
    {
		var enemy = Instantiate(_enemyPrototypePrefab, position, Quaternion.identity);

		enemy.Spawn();

		Debug.Log(enemy.name);
	}

	public void Despawn(bool isHost)
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

			if (isOnNavMesh)
			{
				result = hit.position;

				break;
			}
		}

		return result;
	}
}
