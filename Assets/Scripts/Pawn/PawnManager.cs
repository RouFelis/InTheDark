using InTheDark;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using Unity.Netcode;

using UnityEngine;

namespace InTheDark.Prototypes
{
	// ���� ��ü �� ����ȭ ������ ������ ���̾��µ� �̹� ��� �־ �ʿ� ������
	// �ٵ� ����� ������ �ڵ� ��ĳ ¥�� �ϴ��� �������� ����;;; ����;;;
	// ������ �ٽ� ������
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
		[SerializeField]
		private MonsterSpawner _spawner;

		// ��Ƽ�÷��� ���� ������
		private NetworkList<EnemyPawnRef> _pawns;

		private void Awake()
		{
			OnAwake();
		}

		public override void OnNetworkSpawn()
		{
			if (NetworkManager.Singleton)
			{
				NetworkManager.Singleton.OnServerStarted += OnServerStarted;

				NetworkManager.Singleton.OnClientStarted += OnClientStarted;
			}
		}

		public override void OnNetworkDespawn()
		{
			if (NetworkManager.Singleton)
			{
				NetworkManager.Singleton.OnServerStarted -= OnServerStarted;

				NetworkManager.Singleton.OnClientStarted -= OnClientStarted;
			}
		}

		private void OnAwake()
		{
			_pawns = new NetworkList<EnemyPawnRef>();

			DontDestroyOnLoad(gameObject);
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

		//private void OnListChanged(NetworkListEvent<EnemyPawnRef> changeEvent)
		//{
		//	Debug.Log($"Event Type: {changeEvent.Type}, Value: {changeEvent.Value}, Index: {changeEvent.Index}");
		//}
	}
}