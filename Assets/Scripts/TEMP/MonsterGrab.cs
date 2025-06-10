using BehaviorDesigner.Runtime;
using Unity.Netcode;
using Unity.Netcode.Components;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

namespace InTheDark.Prototypes
{
	public class MonsterGrab : NetworkBehaviour
	{
		public float Range;

		private NetworkVariable<bool> _isActive = new();

		public PositionConstraint PositionConstraint;
		public RotationConstraint RotationConstraint;

		public NetworkTransform NetworkTransform;

		public Collider Collider;
		public NavMeshAgent NavMeshAgent;
		public BehaviorTree BehaviorTree;

		private EnemyPrototypePawn _pawn;
		private Player _target;

		public bool IsEnable
		{
			get
			{
				var value = enabled && _pawn.Target && Vector3.Distance(transform.position, _pawn.Target.transform.position) <= Range && !_isActive.Value;

				return value;
			}
		}

		private void Awake()
		{
			_pawn = GetComponent<EnemyPrototypePawn>();
		}

		public override void OnNetworkSpawn()
		{
			if (IsServer)
			{
				Player.OnDie += OnPlayerDie;
			}
		}

		public override void OnNetworkDespawn()
		{
			if (IsServer)
			{
				Player.OnDie -= OnPlayerDie;
			}
		}

		private void OnPlayerDie()
		{
			if (_target.Equals(_pawn.Target) && _target.IsDead)
			{
				Debug.Log($"Ä¹Å¸¿ö ÁÖ±Ý");

				Detach(_target);
			}
		}

		public void Attach(Player player)
		{
			AttachServerRPC(player);
		}

		public void Detach(Player player)
		{
			DetachServerRPC(player);
		}

		[Rpc(SendTo.Everyone)]
		protected void AttachClientRPC(NetworkBehaviourReference reference)
		{
			if (reference.TryGet(out Player player))
			{
				var constraintSource = new ConstraintSource()
				{
					sourceTransform = player.transform,
					weight = 1.0F
				};
				Collider.enabled = false;
				NetworkTransform.enabled = false;

				NavMeshAgent.enabled = false;
				BehaviorTree.enabled = false;

				PositionConstraint.AddSource(constraintSource);
				RotationConstraint.AddSource(constraintSource);

				PositionConstraint.constraintActive = true;
				RotationConstraint.constraintActive = true;
			}
		}

		[Rpc(SendTo.Server)]
		protected void AttachServerRPC(NetworkBehaviourReference reference)
		{
			if (reference.TryGet(out Player player))
			{
				var statusEffect = player.GetComponent<StatusEffect>();

				_isActive.Value = true;

				_target = player;

				statusEffect.ApplySlowServerRpc(true, 0.0F);

				AttachClientRPC(reference);
			}
		}

		[Rpc(SendTo.Everyone)]
		protected void DetachClientRPC(NetworkBehaviourReference reference)
		{
			Collider.enabled = true;
			NetworkTransform.enabled = true;

			NavMeshAgent.enabled = true;
			BehaviorTree.enabled = true;

			PositionConstraint.RemoveSource(0);
			RotationConstraint.RemoveSource(0);

			PositionConstraint.constraintActive = false;
			RotationConstraint.constraintActive = false;
		}

		[Rpc(SendTo.Server)]
		protected void DetachServerRPC(NetworkBehaviourReference reference)
		{
			if (reference.TryGet(out Player player))
			{
				var statusEffect = player.GetComponent<StatusEffect>();

				_isActive.Value = false;

				_target = default;

				statusEffect.RemoveSlowServerRpc();

				DetachClientRPC(reference);
			}
		}
	} 
}