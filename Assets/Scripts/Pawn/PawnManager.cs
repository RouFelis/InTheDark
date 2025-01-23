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

			}
		}

		public override void OnNetworkDespawn()
		{
			if (NetworkManager.Singleton)
			{

			}
		}

		private void OnAwake()
		{
			_pawns = new NetworkList<EnemyPawnRef>();

			DontDestroyOnLoad(gameObject);
		}
	}
}