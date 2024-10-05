using InTheDark;

using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;

namespace InTheDark.Prototypes
{
	[Serializable]
	public struct EnemyPawnRef : IEquatable<EnemyPawnRef>, INetworkSerializable
	{
		public bool Equals(EnemyPawnRef other)
		{
			return base.Equals(other);
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{

		}
	}

	public class PawnManager : NetworkBehaviour
	{
		private static PawnManager _instance;

		[SerializeField]
		private MonsterSpawner _spawner;

		// 멀티플레이 전용 데이터
		private NetworkList<EnemyPawnRef> _pawns = new NetworkList<EnemyPawnRef>();

		public static PawnManager Instance
		{
			get => _instance;
		}

		//private void OnUpdate()
		//{

		//}

		public override void OnNetworkSpawn()
		{
			_pawns.OnListChanged += OnListChanged;

			if (NetworkManager.Singleton)
			{
				NetworkManager.Singleton.OnServerStarted += OnServerStarted;

				NetworkManager.Singleton.OnClientStarted += OnClientStarted;
			}

			//UpdateManager.OnUpdate += OnUpdate;
		}

		public override void OnNetworkDespawn()
		{
			_pawns.OnListChanged -= OnListChanged;

			if (NetworkManager.Singleton)
			{
				NetworkManager.Singleton.OnServerStarted -= OnServerStarted;

				NetworkManager.Singleton.OnClientStarted -= OnClientStarted;
			}

			//UpdateManager.OnUpdate -= OnUpdate;
		}

		private void OnServerStarted()
		{
			for (var i = 0; i < 2; i++)
			{
				var pawn = new EnemyPawnRef();

				_pawns.Add(pawn);
			}
		}

		private void OnClientStarted()
		{

		}

		private void OnListChanged(NetworkListEvent<EnemyPawnRef> changeEvent)
		{
			Debug.Log($"Event Type: {changeEvent.Type}, Value: {changeEvent.Value}, Index: {changeEvent.Index}");
		}
	}
}