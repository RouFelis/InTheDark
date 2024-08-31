using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;

public class EnemyPrototypePawn : NetworkBehaviour
{
	private NetworkVariable<Vector3> _networkPosition = new NetworkVariable<Vector3>();
	private NetworkVariable<Quaternion> _networkRotation = new NetworkVariable<Quaternion>();

	private void OnEnable()
	{
		_networkPosition.OnValueChanged += OnPositionChanged;
		_networkRotation.OnValueChanged += OnRotationChanged;
	}

	private void OnDisable()
	{
		_networkPosition.OnValueChanged -= OnPositionChanged;
		_networkRotation.OnValueChanged -= OnRotationChanged;
	}

	private void LateUpdate()
	{
		OnLateUpdate();
	}

	private void OnLateUpdate()
	{
		if (IsClient)
		{
			transform.position = _networkPosition.Value;
			transform.rotation = _networkRotation.Value;
		}
		else
		{
			_networkPosition.Value = transform.position;
			_networkRotation.Value = transform.rotation;
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
}
