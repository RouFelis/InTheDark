using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "IsEnemyInSight", menuName = "EQS/IsEnemyInSight")]
public class EQS_IsEnemyInSight : ScriptableEQS
{
	[SerializeField]
	private float _maxDistance;

	[SerializeField]
	private float _angle;

	private List<AIBox> _enemies = new();

	public override void OnAwake()
	{
		_enemies.Clear();

		var enemies = FindObjectsByType<AIBox>(FindObjectsSortMode.None);

		_enemies.AddRange(enemies);
	}

	public override void OnUpdate(EQSConsole.Item item)
	{
		if (_enemies.Count is 0)
		{
			return;
		}

		var itemDistance = _maxDistance;

		foreach (var enemy in _enemies)
		{
			// direction
			var myDirection = enemy.transform.position - item.Position;
			var enemyDirection = item.Position - enemy.transform.position;

			// condition
			var isFocus = Physics.Raycast(item.Position, myDirection, out var hit, _maxDistance);
			var isSight = Vector3.Angle(enemyDirection, enemy.transform.forward) < _angle;

			if (isFocus && isSight && hit.collider.gameObject.CompareTag("Item"))
			{
				itemDistance = Mathf.Min(itemDistance, hit.distance);
			}
		}

		if (IsDisplayable)
		{
			// color
			var percent = Mathf.Min(itemDistance / _maxDistance, 1.0F);
			var color = new Color(1.0F - percent, 0.0F, percent);

			item.Color = color;
		}
	}
}
