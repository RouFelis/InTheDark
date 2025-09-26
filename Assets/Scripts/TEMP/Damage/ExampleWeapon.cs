using Unity.Netcode;

using UnityEngine;

namespace InTheDark.Prototypes
{
    public class ExampleWeapon : NetworkBehaviour
    {
		[SerializeField]
		private float _damage;

		[SerializeField]
		private float _range;

		[SerializeField]
		private LayerMask _target;

		[SerializeField]
        private Player _owner;

		[SerializeField]
		private AudioClip _hitSound;

		private Collider[] _targets = new Collider[256];

		public float Damage
		{
			get => _damage;
			set => _damage = value;
		}

		public Player Owner => _owner;

		public AudioClip HitSound => _hitSound;

		private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
				ExampleAllEnemyAttackServerRPC();
			}
        }

		[Rpc(SendTo.Server)]
		private void ExampleAllEnemyAttackServerRPC()
		{
			var count = Physics.OverlapSphereNonAlloc(transform.position, _range, _targets, _target);

			for (var i = 0; i < count; i++)
			{
				var target = _targets[i];
				var pawn = target?.GetComponent<EnemyPrototypePawn>();

				if (pawn && target is not CharacterController)
				{
					using var damage = new ExampleDamage(this, pawn);

					damage.Update();
				}

				_targets[i] = null;
			}
		}
    } 
}