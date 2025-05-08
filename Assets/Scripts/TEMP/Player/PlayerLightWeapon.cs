using Unity.Netcode;
using UnityEngine;

namespace InTheDark.Prototypes
{
	public class PlayerLightWeapon : SpotLight
	{
		[SerializeField]
		private SpotlightControl _weapon;

		//[SerializeField]
		//private LightSource _source;

		//private void Update()
		//{
		//	Debug.Log($"{_controller.weaponLight.spotAngle}: �ޱ�, {_controller.weaponLight.range}: �Ÿ�");
		//}

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
					//Debug.Log("�Ҵ� ���ε�, ���� �¾ƿ�!");
					SpotlightControl.OnFlash += OnFlash;
				}
				else
				{
					//Debug.Log("�Ҵ� ���ε�, ���� �ƴϿ���...");
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
					//Debug.Log("�Ҵ� �������ε�, ���� �¾ƿ�!");
					SpotlightControl.OnFlash -= OnFlash;
				}
				else
				{
					//Debug.Log("�Ҵ� �������ε�, ���� �ƴϿ���...");
				}
			}
		}

		private void OnFlash()
		{
			Tick();
		}
	
	} 
}