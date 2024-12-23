using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EQSConsole : MonoBehaviour
{
	[Serializable]
	public class Item
	{
		private Vector3 _position;

		private Color _color;

		public Vector3 Position
		{
			get
			{
				return _position;
			}

			private set
			{
				_position = value;
			}
		}

		public Color Color
		{
			get
			{
				return _color;
			}

			set
			{
				_color = value;
			}
		}

		public Item()
		{

		}

		public Item(Vector3 position)
		{
			_position = position;
		}

		public void OnDrawGizmos()
		{
			Gizmos.color = _color;
			Gizmos.DrawSphere(_position, 0.25F);
		}
	}

    [SerializeField]
    private Vector3Int _start;

    [SerializeField]
    private Vector3Int _end;

	[SerializeField]
	private ScriptableEQS[] _scriptableEQS;

	private List<Item> _items = new();

    private void Awake()
    {
        for (var x = _start.x; x < _end.x; x++)
        {
			for (var y = _start.y; y < _end.y; y++)
			{
				for (var z = _start.z; z < _end.z; z++)
				{
					var position = new Vector3(x, y + 0.5F, z);
					var item = new Item(position);

					_items.Add(item);
				}
			}
		}

		foreach (var test in _scriptableEQS)
		{
			test.OnAwake();
		}
    }

	private void Update()
	{
		foreach (var item in _items)
		{
			item.Color = Color.white;

			if (NavMesh.SamplePosition(item.Position, out var hit, 0.5F, NavMesh.AllAreas))
			{
				foreach (var test in _scriptableEQS)
				{
					test.OnUpdate(item);
				}
			}
		}
	}

	private void OnDrawGizmos()
	{
		foreach (var item in _items)
		{
			if (NavMesh.SamplePosition(item.Position, out var hit, 0.5F, NavMesh.AllAreas))
			{
				item?.OnDrawGizmos();
			}
		}
	}
}
