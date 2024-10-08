using InTheDark.Prototypes;

using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.AI;

public class EnemyPrototypePawn : NetworkPawn, ICharacter, IDamaged
{
	[SerializeField]
	private int _angle = 30;

	[SerializeField]
	private int _distance = 10;

	[SerializeField]
	private NetworkVariable<int> _health = new NetworkVariable<int>();

	[SerializeField]
	private NetworkVariable<float> _resistance = new NetworkVariable<float>(30);

	[SerializeField]
	private NetworkVariable<EnemyAttackPrototype> _test = new NetworkVariable<EnemyAttackPrototype>();

	[SerializeField]
	private EnemyAttackPrototype _attackModule = new EnemyAttackPrototype();

	private List<LightSource> _sighted = new List<LightSource>();

	public string Name { get; set; }

	public int Health 
	{
		get=> _health.Value; 

		set => _health.Value = value;
	}

	public int Damage { get; set; }

	// IsHost, IsClient, IsServer, IsOwner �� true ���� ��������������������������������������

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		_resistance.OnValueChanged += OnResistanceChanged;

		if (IsHost)
		{
			if (_test.Value == null)
			{
				_test.Value = new EnemyAttackPrototype()
				{
					Cooldown = 100.0f,
					Name = name
				};
			}
		}

		Logger.Instance?.LogInfo($"{name} has been spawned!");

		Debug.Log($"{name} has been spawned!");
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();

		_resistance.OnValueChanged -= OnResistanceChanged;
	}

	private void OnResistanceChanged(float oldValue, float newValue)
	{
		if (newValue < 0.0f || Mathf.Approximately(newValue, 0.0f))
		{
			Dead();
		}
	}

	public void OnLightInsighted()
	{
		var damage = Time.deltaTime;
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

		_resistance.Value -= damage * current.DamagePercent;

		_sighted.Clear();
	}

	public void OnLightInsighted(LightSource light)
	{
		//_resistance.Value -= Time.deltaTime;

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

	// ���� �����ص� �� ���� ���� �Լ� �ۼ���
	// �ƴ� �ٵ� �̰� ���� �����ؼ� �����ؾ� ���� �� �ִµ�
	public void AttackPrototype(NetworkPawn target)
	{
		// ���� �����ߴٰ� �̺�Ʈ �˸�^^
	}

	public void Dead()
	{
		NetworkObject.Despawn();

		Destroy(gameObject);
	}
}
