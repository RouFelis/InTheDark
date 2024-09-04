using InTheDark.Prototypes;
using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;

public class EnemyPrototypePawn : NetworkBehaviour, ICharacter, IDamaged
{
	private NetworkVariable<Vector3> _networkPosition = new NetworkVariable<Vector3>();
	private NetworkVariable<Quaternion> _networkRotation = new NetworkVariable<Quaternion>();

	public string Name { get; set; }

	public int Health { get; set; }

	public int Damage { get; set; }

	private void OnEnable()
	{
		UpdateManager.OnUpdate += OnUpdate;

		_networkPosition.OnValueChanged += OnPositionChanged;
		_networkRotation.OnValueChanged += OnRotationChanged;
	}

	private void OnDisable()
	{
		UpdateManager.OnUpdate -= OnUpdate;

		_networkPosition.OnValueChanged -= OnPositionChanged;
		_networkRotation.OnValueChanged -= OnRotationChanged;
	}

	protected virtual void OnUpdate()
	{
		// IsHost, IsClient, IsServer, IsOwner 陥 true 級嬢身 せせせせせせせせせせせせせせせせせせせ

		if (IsHost)
		{
			_networkPosition.Value = transform.position;
			_networkRotation.Value = transform.rotation;
		}
		else
		{
			transform.position = _networkPosition.Value;
			transform.rotation = _networkRotation.Value;
		}
	}

	private void OnPositionChanged(Vector3 oldValue, Vector3 newValue)
	{
		if (IsClient)
		{
			transform.position = newValue;
		}
	}

	private void OnRotationChanged(Quaternion oldValue, Quaternion newValue)
	{
		if (IsClient)
		{
			transform.rotation = newValue;
		}
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		Debug.Log($"{name} has been spawned!");
	}

	public void TakeDamage(int amount)
	{
		throw new NotImplementedException();
	}

	public void Attack(ICharacter target)
	{
		throw new NotImplementedException();
	}
}
