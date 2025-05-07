using System;
using Unity.Netcode;
using UnityEngine;

namespace InTheDark.Prototypes
{
	[Serializable]
	public struct OnAttackEvent
	{
		public NetworkBehaviourReference Target;
		public NetworkBehaviourReference Causer;
	}

	public class SpotLight : NetworkBehaviour
	{
		[SerializeField]
		private float _damage;

		[SerializeField]
		private float _angle;

		[SerializeField]
		private float _range;

		[SerializeField]
		private LayerMask _target;

		[SerializeField]
		private NetworkBehaviour _causer;

		public event Action<OnAttackEvent> OnAttack;

		public float Damage
		{
			get
			{
				return _damage;
			}

			set
			{
				_damage = value;
			}
		}

		public float Angle
		{
			get
			{
				return _angle;
			}

			set
			{
				_angle = value;
			}
		}

		public float Range
		{
			get
			{
				return _range;
			}

			set
			{
				_range = value;
			}
		}

		public LayerMask Target
		{
			get
			{
				return _target;
			}

			set
			{
				_target = value;
			}
		}

		public NetworkBehaviour Causer
		{
			get
			{
				return _causer;
			}

			set
			{
				_causer = value;
			}
		}

		public SpotLight SetDamage(float damage)
		{
			_damage = damage;

			return this;
		}

		public SpotLight SetAngle(float angle)
		{
			_angle = angle;

			return this;
		}

		public SpotLight SetRange(float range)
		{
			_range = range;

			return this;
		}

		public SpotLight SetTarget(LayerMask target)
		{
			_target = target;

			return this;
		}

		public SpotLight SetCauser(NetworkBehaviour causer)
		{
			_causer = causer;

			return this;
		}

		public void Tick()
		{
			var causer = !_causer ? this : _causer;
			var targets = Physics.OverlapSphere(causer.transform.position, _range, _target);

			foreach (var target in targets)
			{
				var direction = target.transform.position - causer.transform.position;
				var isCollision = Physics.Raycast(causer.transform.position, direction, out var hitInfo, _range);
				var isTargetInFOV = Vector3.Angle(direction, causer.transform.forward) < _angle;

				if (hitInfo.collider.Equals(target) && isCollision && isTargetInFOV)
				{
					var pawn = target.GetComponent<EnemyPrototypePawn>();
					var socket = new OnAttackEvent()
					{
						Causer = causer,
						Target = pawn
					};
					
					if (pawn)
					{
						pawn.OnLightInsighted(this);
					}

					OnAttack?.Invoke(socket);
				}
			}
		}
	} 
}