using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace InTheDark.Prototypes
{
	[Serializable]
	public class ResurrectingPawn
    {
        public EnemyPrototypePawn Pawn;
        public float Time;
    }

	public class ResurrectOnDeathHandler : NetworkBehaviour
    {
        [SerializeField]
        private List<ResurrectingPawn> _nodes = new();

		private List<ResurrectingPawn> _cache = new();

		public override void OnNetworkSpawn()
		{
			UpdateManager.OnUpdate += OnUpdate;
		}

		public override void OnNetworkDespawn()
		{
			UpdateManager.OnUpdate -= OnUpdate;
		}

		private void OnUpdate()
		{
			var time = Time.deltaTime;

			foreach (var node in _nodes)
			{
				node.Time = Mathf.Max(node.Time - time, 0.0F);

				if (node.Time < 0.0F || Mathf.Approximately(node.Time, 0.0F))
				{
					_cache.Add(node);
				}
			}

			foreach (var node in _cache)
			{
				_nodes.Remove(node);
				EnemyResurrectCompleteRPC(node.Pawn);
			}

			_cache.Clear();
		}

		[Rpc(SendTo.Server)]
        public void StartEnemyResurrectRPC(NetworkBehaviourReference reference, float time)
		{
			var isBehaviourAttached = reference.TryGet(out EnemyPrototypePawn pawn);

			if (isBehaviourAttached)
			{
				var node = new ResurrectingPawn()
				{
					Pawn = pawn,
					Time = time
				};

				pawn.IsDead = true;

				_nodes.Add(node);
			}
		}

		[Rpc(SendTo.Server)]
		private void EnemyResurrectCompleteRPC(NetworkBehaviourReference reference)
		{
			var isBehaviourAttached = reference.TryGet(out EnemyPrototypePawn pawn);

			if (isBehaviourAttached)
			{
				pawn.Health = 100;
				pawn.Resistance = 30.0F;
				pawn.IsDead = false;	
			}
		}
	} 
}