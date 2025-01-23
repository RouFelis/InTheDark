using System;
using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;

namespace InTheDark.Prototypes
{
    [Serializable]
	public struct NetworkEnemyReference
    {

	}

	[Serializable]
    public class Enemy
    {

	}

    public class EnemyManager : NetworkBehaviour
    {
        private static EnemyManager _instance;

        [SerializeField]
        private NetworkVariable<int> _currentStage = new(-1);

        [SerializeField]
        private List<Enemy> _enemies = new();

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
            if (NetworkManager.Singleton)
            {
                Game.OnDungeonEnter -= OnDungeonEnter;
                Game.OnDungeonExit -= OnDungeonExit; 
            }
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