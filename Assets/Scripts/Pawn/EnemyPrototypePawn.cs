using InTheDark.Prototypes;

using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;

public class EnemyPrototypePawn : NetworkPawn, ICharacter, IDamaged
{
	[SerializeField]
	private int _angle = 30;

	[SerializeField]
	private int _distance = 10;

	[SerializeField]
	private bool _isDead = false;

	[SerializeField]
	private NetworkVariable<int> _health = new NetworkVariable<int>();

	[SerializeField]
	private NetworkVariable<float> _resistance = new NetworkVariable<float>(30);

	[SerializeField]
	private NetworkVariable<int> _damage = new NetworkVariable<int>(5);

	[SerializeField]
	private NetworkVariable<float> _cooldown = new NetworkVariable<float>(5.0f);

	private List<LightSource> _sighted = new List<LightSource>();

	public string Name { get; set; }

	public string Land {  get; set; }

	public int Health 
	{
		get=> _health.Value; 

		set => _health.Value = value;
	}

	public int Damage { get; set; }

	// IsHost, IsClient, IsServer, IsOwner 다 true 들어옴 ㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋㅋ

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		_resistance.OnValueChanged += OnResistanceChanged;

		UpdateManager.OnUpdate += OnUpdate;
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();

		_resistance.OnValueChanged -= OnResistanceChanged;

		UpdateManager.OnUpdate -= OnUpdate;
	}

	private void OnUpdate()
	{
		_cooldown.Value = Math.Max(_cooldown.Value - Time.deltaTime, 0.0f);
	}

	private void OnResistanceChanged(float oldValue, float newValue)
	{
		if (!_isDead && (newValue < 0.0f || Mathf.Approximately(newValue, 0.0f)))
		{
			Dead();
		}
	}

	public void OnLightInsighted()
	{
		var current = _sighted[0];

		if (_sighted.Count > 1)
		{
			for (var i = 1; i < _sighted.Count; i++)
			{
				var source = _sighted[i];
				var direction = source.transform.position - transform.position;
				var isOccultation = Physics.Raycast(transform.position, direction, out var hit, _distance);
				var isSight = Vector3.Angle(direction, transform.forward) < _angle;

				if (hit.collider == source && isOccultation && isSight && current < source)
				{
					current = source;
				}
			}
		}

		_resistance.Value -= Time.deltaTime * current.DamagePercent;

		_sighted.Clear();
	}

	public void OnLightInsighted(LightSource light)
	{
		_sighted.Add(light);
	}

	public void TakeDamage(int amount)
	{
		throw new NotImplementedException();
	}

	public void Attack(ICharacter target)
	{
		throw new NotImplementedException();
	}

	// 기존 정의해둔 것 말고 새로 함수 작성함
	// 아니 근데 이거 공격 관련해서 구현해야 만들 수 있는데
	public void AttackPrototype(NetworkPawn target)
	{
		// 대충 공격했다고 이벤트 알림^^
	}

	// 갸아아악
	public void AttackPrototype(IDamaged target)
	{
		if (_cooldown.Value < 0.0F || Mathf.Approximately(_cooldown.Value, 0.0F))
		{
			// 이거 어차피 저쪽에 구현 안 되어 있을걸?
			//target.TakeDamage(_damage.Value);

			Debug.Log("HIT!!!");

			_cooldown.Value = 5.0F;
		}
	}

	public void Dead()
	{
		_isDead = true;

		NetworkObject.Despawn();

		Destroy(gameObject);
	}

	public void Die()
	{
		throw new NotImplementedException();
	}
}
