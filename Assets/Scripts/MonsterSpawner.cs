using InTheDark;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

public class MonsterSpawner : MonoBehaviour
{
    // 몬스터 생성 최대 거리
    [SerializeField]
    private float _radius;

    // 프리팹
    [SerializeField]
    private EnemyPrototype _enemyPrototypePrefab;

    public void Spawn()
    {
		var position = GetRandomPositionInNavMesh();

		Instantiate(_enemyPrototypePrefab, position, Quaternion.identity);
	}

    private Vector3 GetRandomPositionInNavMesh()
    {
		var result = Vector3.zero;

		for (var i = 0; i < 30; i++)
		{
			var direction = Random.insideUnitSphere * _radius;
			var isOnNavMesh = NavMesh.SamplePosition(direction, out var hit, _radius, NavMesh.AllAreas);

			if (isOnNavMesh)
			{
				result = hit.position;

				break;
			}
		}

		return result;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, _radius);
	}
}
