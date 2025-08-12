using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class Bomb : NetworkBehaviour
{
	[SerializeField]
	protected float _damage;

	[SerializeField]
	protected float _range;

	[SerializeField]
	protected float _delay = 0.9F;

	[SerializeField]
	private float _minLightIntensity;

	[SerializeField]
	private float _maxLightIntensity;

	[SerializeField]
	private float _radius;

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

	private int _size;

	private Collider[] _colliders = new Collider[16];

	[Rpc(SendTo.Everyone)]
	public void ExploseClientRPC()
	{
		StartCoroutine(OnExplose());
	}

	[Rpc(SendTo.Server)]
	private void ExploseServerRPC()
	{
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

				Debug.Log($"{player.name}({player.OwnerClientId})°¡ Æø¹ß¿¡ ÈÛ¾µ¸².");
			}

			_colliders[i] = default;
		}

		DestroyClientRPC();
	}

	[Rpc(SendTo.Everyone)]
	private void DestroyClientRPC()
	{
		Destroy(gameObject);
	}

	private IEnumerator OnExplose()
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

		_audioSource.PlayOneShot(_exploseAudioClip);
		_exploseEffect.Play();


	}
}