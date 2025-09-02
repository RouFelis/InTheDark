using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.VFX;

namespace InTheDark.Prototypes
{
	public class EnemySelfDestruction : EnemyWeapon
	{
		[SerializeField]
		private float _minLightIntensity;

		[SerializeField]
		private float _maxLightIntensity;

		[SerializeField]
		private float _radius;

		[SerializeField]
		private float _explosingSpeedRatio;

		[SerializeField]
		private LayerMask _targetLayer;

		[SerializeField]
		private AudioClip _warningAudioClip;

		[SerializeField]
		private AudioClip _exploseAudioClip;

		[SerializeField]
		private AnimationCurve _knockbackCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

		[SerializeField]
		private Light _light;

		[SerializeField]
		private AudioSource _audioSource;

		[SerializeField]
		private VisualEffect _exploseEffect;

		[SerializeField]
		private NavMeshAgent _agent;

		private int _size;

		private Collider[] _colliders = new Collider[16];
		//서버 검증 X ->
		protected override async UniTask OnAttack(IHealth target)
		{
			OnExplosionServerRPC();

			// 그냥 알림뜨는거 보기 싫어서 넣음
			// UniTask 조만간 제거 예정
			await UniTask.NextFrame();

			//var speed = _agent.speed;

			//_agent.speed = speed * _explosingSpeedRatio;

			//await OnExploseHerald().ToUniTask();

			//OnExplosionClientRPC();

			//_size = Physics.OverlapSphereNonAlloc(transform.position, _radius, _colliders, _targetLayer);

			//for (var i = 0; i < _size; i++)
			//{
			//	var collider = _colliders[i];
			//	var player = collider?.GetComponent<Player>();

			//	if (player)
			//	{
			//		player.TakeDamage(_damage, null);

			//		if (!player.IsDead)
			//		{
			//			var direction = player.transform.position - transform.position;
			//			var pushDir = direction.normalized;

			//			var flightTime = 1.0F;
			//			var flightSpeed = 10.0F;

			//			var knockBackHeight = 10.0F;

			//			await player.SetStun(flightTime, flightSpeed, _knockbackCurve, knockBackHeight, direction).ToUniTask();
			//		}

			//		Debug.Log($"{player.name}({player.OwnerClientId})가 폭발에 휩쓸림.");
			//	}

			//	_colliders[i] = default;
			//}

			//Debug.Log("죽을게");

			//_pawn.Die();
		}

		[Rpc(SendTo.Server)]
		private void OnExplosionServerRPC()
		{
			var speed = _agent.speed;

			_agent.speed = speed * _explosingSpeedRatio;

			StartCoroutine(OnExploseHerald());

			_size = Physics.OverlapSphereNonAlloc(transform.position, _radius, _colliders, _targetLayer);

			for (var i = 0; i < _size; i++)
			{
				var collider = _colliders[i];
				var player = collider?.GetComponent<Player>();

				if (player)
				{
					player.TakeDamage(_damage, null);

					if (!player.IsDead)
					{
						var direction = player.transform.position - transform.position;
						var pushDir = direction.normalized;

						var flightTime = 1.0F;
						var flightSpeed = 10.0F;

						var knockBackHeight = 10.0F;

						StartCoroutine(player.SetStun(flightTime, flightSpeed, _knockbackCurve, knockBackHeight, direction));
					}

					Debug.Log($"{player.name}({player.OwnerClientId})가 폭발에 휩쓸림.");
				}

				_colliders[i] = default;
			}

			Debug.Log("죽을게");

			_pawn.Die();
		}

		[Rpc(SendTo.Everyone)]
		private void OnExplosionClientRPC()
		{
			_audioSource.PlayOneShot(_exploseAudioClip);
			_exploseEffect.Play();
		}

		private IEnumerator OnExploseHerald()
		{
			var time = 0.0F;

			var count = 0;

			while (time < _delay)
			{
				var deltaTime = Time.deltaTime;
				var normalized = Mathf.PingPong(time, 0.25F) / 0.25F;

				if (time > count * 0.5F)
				{
					_audioSource.PlayOneShot(_warningAudioClip);
					count++;
				}

				_light.intensity = Mathf.Lerp(_minLightIntensity, _maxLightIntensity, normalized);

				time += deltaTime;

				yield return null;
			}

			OnExplosionClientRPC();
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, _radius);
		}
	} 
}