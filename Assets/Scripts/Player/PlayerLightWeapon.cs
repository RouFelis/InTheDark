using Unity.Netcode;
using UnityEngine;

namespace InTheDark.Prototypes
{
	public class PlayerLightWeapon : NetworkBehaviour
	{
		[SerializeField]
		private SpotlightControl _weapon;

		[SerializeField]
		private LightSource _source;

		//private void Update()
		//{
		//	Debug.Log($"{_controller.weaponLight.spotAngle}: ¾Þ±Û, {_controller.weaponLight.range}: °Å¸®");
		//}

		public override void OnNetworkSpawn()
		{
			if (!_weapon)
			{
				_weapon = GetComponent<SpotlightControl>();
			}

			if (!_source)
			{
				_source = GetComponent<LightSource>();
			}

			if (_weapon && _source)
			{
				_weapon.isRightClickHeld.OnValueChanged += OnPlayerRightClickHeld;
				_weapon.isRecovering.OnValueChanged += OnPlayerRecovering;

				SetWeaponData(false);
				SetWeaponActive(false);
			}
		}

		public override void OnNetworkDespawn()
		{
			if (_weapon)
			{
				_weapon.isRightClickHeld.OnValueChanged -= OnPlayerRightClickHeld;
				_weapon.isRecovering.OnValueChanged -= OnPlayerRecovering;
			}
		}

		private void OnPlayerRightClickHeld(bool previousValue, bool newValue)
		{
			if (previousValue != newValue)
			{
				SetWeaponData(newValue);
			}
		}

		private void OnPlayerRecovering(bool previousValue, bool newValue)
		{
			if (previousValue != newValue)
			{
				SetWeaponActive(newValue);
			}
		}

		private void SetWeaponData(bool value)
		{
			var intensity = value ? _weapon.zoomedIntensity : _weapon.defaultIntensity;

			//Debug.LogError($"OnrightClicked {value}");

			_source.Angle = _weapon.weaponLight.spotAngle;
			_source.Distance = Mathf.Sqrt(intensity);
			_source.DamagePercent = value ? _weapon.zoomDamage.Value : _weapon.baseDamage.Value;
		}

		private void SetWeaponActive(bool value)
		{
			//Debug.LogError($"OnRestore {value}");

			if (value)
			{
				LightManager.Instance.OnWorkLightSpanwed(_source);
			}
			else
			{
				LightManager.Instance.OnWorkLightDespawned(_source);
			}
		}
	} 
}