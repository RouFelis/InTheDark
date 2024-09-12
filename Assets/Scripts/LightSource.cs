using InTheDark.Prototypes;

using System;

using Unity.Netcode;

using UnityEngine;

namespace InTheDark.Prototypes
{
	// TODO: 감지 기능 별도 스크립트 분리
	public class LightSource : NetworkBehaviour
	{
		[SerializeField]
		private float _angle;

		[SerializeField]
		private float _distance;

		[SerializeField]
		private float _damagePercent;

		public float Angle => _angle;

		public float Distance => _distance;

		public float DamagePercent => _damagePercent;

		public static LightSource operator >(LightSource a, LightSource b)
		{
			return a._damagePercent > b._damagePercent ? a : b;
		}

		public static LightSource operator <(LightSource a, LightSource b)
		{
			return a._damagePercent < b._damagePercent ? b : a;
		}

		public override void OnNetworkSpawn()
		{
			LightManager.Instance.OnWorkLightSpanwed(this);
		}

		public override void OnNetworkDespawn()
		{
			LightManager.Instance.OnWorkLightDespawned(this);
		}
	}
}