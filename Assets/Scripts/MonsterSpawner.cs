using InTheDark;
using InTheDark.Prototypes;

using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.AI;

public class MonsterSpawner : NetworkBehaviour
{
	[Serializable]
	private struct EnemyRef : IEquatable<EnemyRef>, INetworkSerializable
	{
		public Vector3 Position;
		public Quaternion Quaternion;

		public static implicit operator EnemyRef(EnemyPrototype enemy)
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
	private bool _isActive;

	[SerializeField]
	private bool _isHost;

	private NetworkList<EnemyRef> _spawned = new NetworkList<EnemyRef>();

    // 프리팹
    [SerializeField]
    private NetworkObject _enemyPrototypePrefab;

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, _radius);
	}

	private void OnDisable()
	{
		if (_isActive)
		{
			OnMonsterSpawnerActive();
		}
	}

	public void OnHostStarted()
	{
		var enemyRef = SpawnRandomRef();

		_spawned.Add(enemyRef);

		Debug.Log(nameof(OnHostStarted));
	}

	public void OnClientStarted()
	{
		foreach (var enemyRef in _spawned)
		{
			Spawn(enemyRef.Position);
		}

		Debug.Log(nameof(OnClientStarted));
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

	public void OnMonsterSpawnerActive()
	{
		_isActive = !_isActive;

		if (_isActive && NetworkManager.Singleton)
		{
			NetworkManager.Singleton.OnServerStarted += OnHostStarted;

			NetworkManager.Singleton.OnClientStarted += OnClientStarted;
			NetworkManager.Singleton.OnClientStopped += Despawn;
		}
		else
		{
			NetworkManager.Singleton.OnServerStarted -= OnHostStarted;

			NetworkManager.Singleton.OnClientStarted -= OnClientStarted;
			NetworkManager.Singleton.OnClientStopped -= Despawn;
		}
	}
}
