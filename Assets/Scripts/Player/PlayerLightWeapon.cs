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
		//	Debug.Log($"{_controller.weaponLight.spotAngle}: 앵글, {_controller.weaponLight.range}: 거리");
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
					//Debug.Log("할당 중인디, 쭈인 맞아여!");
					SpotlightControl.OnFlash += OnFlash;
				}
				else
				{
					//Debug.Log("할당 중인디, 쭈인 아니에여...");
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
					//Debug.Log("할당 해제중인디, 쭈인 맞아여!");
					SpotlightControl.OnFlash -= OnFlash;
				}
				else
				{
					//Debug.Log("할당 해제중인디, 쭈인 아니에여...");
				}
			}
		}

		private void OnFlash()
		{
			Tick();
		}

		//private void OnPlayerRightClickHeld(bool previousValue, bool newValue)
		//{
		//	if (previousValue != newValue)
		//	{
		//		SetWeaponData(newValue);
		//	}
		//}

		//private void OnPlayerRecovering(bool previousValue, bool newValue)
		//{
		//	if (previousValue != newValue)
		//	{
		//		SetWeaponActive(newValue);
		//	}
		//}

		//private void SetWeaponData(bool value)
		//{
		//	var intensity = value ? _weapon.zoomedIntensity : _weapon.defaultIntensity;

		//	//Debug.LogError($"OnrightClicked {value}");

		//	_source.Angle = _weapon.thirdPersonWeaponLight.spotAngle;
		//	_source.Distance = Mathf.Sqrt(intensity);
		//	_source.DamagePercent = value ? _weapon.zoomDamage.Value : _weapon.baseDamage.Value;
		//}

		//private void SetWeaponActive(bool value)
		//{
		//	//Debug.LogError($"OnRestore {value}");

		//	if (value)
		//	{
		//		LightManager.Instance.OnWorkLightSpanwed(_source);
		//	}
		//	else
		//	{
		//		LightManager.Instance.OnWorkLightDespawned(_source);
		//	}
		//}
	} 
}