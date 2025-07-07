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
		public float Offset;

		private NetworkVariable<bool> _isActive = new();

		public PositionConstraint PositionConstraint;
		public RotationConstraint RotationConstraint;

		public NetworkTransform NetworkTransform;

		public Collider Collider;
		public NavMeshAgent NavMeshAgent;
		public BehaviorTree BehaviorTree;

		[SerializeField]
		private Transform _targetTransform;

		private EnemyPrototypePawn _pawn;
		private Player _target;

		public bool IsActive
		{
			get
			{
				return _isActive.Value;
			}
		}

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

			_targetTransform = GetAnchorTransform();
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
			try
			{
				if (_target.Equals(_pawn.Target) && _target.IsDead)
				{
					Debug.Log($"Ä¹Å¸¿ö ÁÖ±Ý");

					Detach(_target);
				}
			}
			catch
			{

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
		protected void AttachClientRPC(NetworkBehaviourReference reference, Vector3 position)
		{
			if (reference.TryGet(out Player player))
			{
				var playerCollider = player.GetComponent<Collider>();
				var targetTransform = GetAnchorTransform();
				var constraintSource = new ConstraintSource()
				{
					sourceTransform = targetTransform,
					weight = 1.0F
				};

				Physics.IgnoreCollision(Collider, playerCollider, true);

				Collider.enabled = false;
				NetworkTransform.enabled = false;

				NavMeshAgent.enabled = false;
				BehaviorTree.enabled = false;

				PositionConstraint.AddSource(constraintSource);
				RotationConstraint.AddSource(constraintSource);

				PositionConstraint.constraintActive = true;
				RotationConstraint.constraintActive = true;

				targetTransform.SetParent(player.transform);
				targetTransform.LookAt(player.transform);

				targetTransform.position = position;
			}
		}

		[Rpc(SendTo.Server)]
		protected void AttachServerRPC(NetworkBehaviourReference reference)
		{
			if (reference.TryGet(out Player player))
			{
				var statusEffect = player.GetComponent<StatusEffect>();
				var insideUnitCircle = Random.insideUnitCircle;
				var randomPosition = new Vector3(insideUnitCircle.x + player.transform.position.x, player.transform.position.y + Offset, insideUnitCircle.y + player.transform.position.z);

				_isActive.Value = true;

				_target = player;

				statusEffect.ApplySlowServerRpc(true, default);

				AttachClientRPC(reference, randomPosition);
			}
		}

		[Rpc(SendTo.Everyone)]
		protected void DetachClientRPC(NetworkBehaviourReference reference)
		{
			if (reference.TryGet(out Player player))
			{
				var playerCollider = player.GetComponent<Collider>();
				var targetTransform = GetAnchorTransform();

				Physics.IgnoreCollision(Collider, playerCollider, false);

				Collider.enabled = true;
				NetworkTransform.enabled = true;

				NavMeshAgent.enabled = true;
				BehaviorTree.enabled = true;

				PositionConstraint.RemoveSource(0);
				RotationConstraint.RemoveSource(0);

				PositionConstraint.constraintActive = false;
				RotationConstraint.constraintActive = false;

				targetTransform.SetParent(transform);

				transform.position = Vector3.zero;
			}
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

		private Transform GetAnchorTransform()
		{
			if (!_targetTransform)
			{
				var gameObject = new GameObject();

				gameObject.transform.SetParent(transform, false);

				_targetTransform = gameObject.transform;
			}

			return _targetTransform;
		}
	} 
}