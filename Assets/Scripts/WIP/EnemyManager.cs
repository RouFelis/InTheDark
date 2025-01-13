using System;
using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;

namespace InTheDark.Prototypes
{
    [Serializable]
	public struct NetworkSerializedEnemy : IEquatable<NetworkSerializedEnemy>
    {
        public int BuildID;
        public ulong InstanceID;

		public bool Equals(NetworkSerializedEnemy other)
		{
			return InstanceID.Equals(other.InstanceID);
		}
	}

	[Serializable]
    public class Enemy
    {
        [SerializeField]
        private int _buildID;

		[SerializeField]
        private ulong _instanceID;

		public int BuildID
		{
			get
			{
				return _buildID;
			}

			set
			{
				_buildID = value;
			}
		}

        public ulong InstanceID
        {
            get
            {
                return _instanceID;
            }

            set
            {
                _instanceID = value;
            }
        }
	}

    public class EnemyManager : NetworkBehaviour
    {
        private static EnemyManager _instance;

        [SerializeField]
        private NetworkVariable<bool> _isStagePlaying = new();

        [SerializeField]
        private List<Enemy> _enemies = new();

        [SerializeField]
        private AIGenerateData[] _generatingData;

        private NetworkList<NetworkSerializedEnemy> _networkEnemies;

        public static EnemyManager Instance
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

        static EnemyManager()
        {
            _instance = default;
        }

		private void Awake()
		{
            _instance = this;
            _networkEnemies = new NetworkList<NetworkSerializedEnemy>();

            DontDestroyOnLoad(gameObject);
		}

		public override void OnNetworkSpawn()
		{
			if (NetworkManager.Singleton)
			{
				Game.OnDungeonEnter += OnDungeonEnter;
				Game.OnDungeonExit += OnDungeonExit;
			}
		}

		public override void OnNetworkDespawn()
		{
			Game.OnDungeonEnter -= OnDungeonEnter;
			Game.OnDungeonExit -= OnDungeonExit;
		}

		private void OnDungeonEnter(DungeonEnterEvent received)
        {

        }

        private void OnDungeonExit(DungeonExitEvent received) 
        { 

        }

		[Rpc(SendTo.Server)]
        private void AddNetworkEnemyRPC(DungeonEnterEvent received)
        {
            
        }

        [Rpc(SendTo.Everyone)]
        private void InstantiateEnemyPawnRPC()
        {

        }
	} 
}