using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class FindBehindableLocation : MonoBehaviour
{
	[SerializeField]
	private float _maxDistance;

	[SerializeField]
	private float _angle;

	[SerializeField]
	private Vector3 _start;

	[SerializeField]
	private Vector3 _end;

	private IEnumerable<Vector3> _located;

	private List<Vector3> _positions = new();

	private void Awake()
	{
		for (var x = _start.x; x < _end.x; x++)
		{
			for (var y = _start.y; y < _end.y; y++)
			{
				for (var z = _start.z; z < _end.z; z++)
				{
					var position = new Vector3(x, y, z);

					_positions.Add(position);
				}
			}
		}
	}

	private void Update()
	{
		OnUpdate();
	}

	private void OnDrawGizmos()
	{
		if (_located is not null)
		{
			foreach (var item in _located)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawSphere(item, 0.25F);
			}
		}
	}

	private void OnUpdate()
	{
		_located = _positions
			.Where(position => NavMesh.SamplePosition(position, out var hit, 0.5F, NavMesh.AllAreas))
			.Where(OnTest)
			.ToList();
	}

	private bool OnTest(Vector3 item)
	{
		// direction
		var myDirection = transform.position - item;
		var enemyDirection = item - transform.position;

		// condition
		var isSight = Vector3.Angle(enemyDirection, transform.forward) < _angle;
		var result = false;

		var isFocus = Physics.Raycast(item, myDirection, out var hit, _maxDistance);

		if (/*hit.collider*/isFocus && isSight)
		{
			result = !hit.collider.CompareTag("Item") && !hit.collider.CompareTag("Player");
		}

		return result;
	}
}
