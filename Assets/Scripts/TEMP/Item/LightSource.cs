using InTheDark.Prototypes;

using System;

using Unity.Netcode;

using UnityEngine;

namespace InTheDark.Prototypes
{
	// TODO: 감지 기능 별도 스크립트 분리
	// 사용 중지된 스크립트인데 일단 킵
	[Obsolete]
	public class LightSource : NetworkBehaviour
	{
		[SerializeField]
		private float _angle;

		[SerializeField]
		private float _distance;

		[SerializeField]
		private float _damagePercent;

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

		public float Distance
		{
			get
			{
				return _distance;
			}

			set
			{
				_distance = value;
			}
		}

		public float DamagePercent
		{
			get
			{
				return _damagePercent;
			}

			set
			{
				_damagePercent = value;
			}
		}

		//public static LightSource operator >(LightSource a, LightSource b)
		//{
		//	return a._damagePercent > b._damagePercent ? a : b;
		//}

		//public static LightSource operator <(LightSource a, LightSource b)
		//{
		//	return a._damagePercent < b._damagePercent ? b : a;
		//}

		//public override void OnNetworkSpawn()
		//{
		//	LightManager.Instance.OnWorkLightSpanwed(this);
		//}

		//public override void OnNetworkDespawn()
		//{
		//	LightManager.Instance.OnWorkLightDespawned(this);
		//}
	}
}