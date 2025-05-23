using Unity.Netcode;
using UnityEngine;

namespace InTheDark.Prototypes
{
	public class PlayerLightWeapon : SpotLight
	{
		[SerializeField]
		private SpotlightControl _weapon;

		public override void OnNetworkSpawn()
		{
			if (!_weapon)
			{
				_weapon = GetComponent<SpotlightControl>();
			}

			if (_weapon)
			{
				var player = GetComponent<Player>();
				var range = Mathf.Sqrt(_weapon.defaultIntensity);

				SetCauser(player);
				SetAngle(_weapon.thirdPersonWeaponLight.spotAngle);
				SetRange(range);
				SetDamage(_weapon.baseDamage.Value);

				if (player.IsOwner)
				{
					SpotlightControl.OnFlash += OnFlash;
				}
			}
		}

		public override void OnNetworkDespawn()
		{
			if (NetworkManager.Singleton && _weapon)
			{
				var player = GetComponent<Player>();

				if (player.IsOwner)
				{
					SpotlightControl.OnFlash -= OnFlash;
				}
			}
		}

		private void OnFlash()
		{
			Tick();
		}
	} 
}