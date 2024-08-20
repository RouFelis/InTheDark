using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class RandomNavMeshSpawner : MonoBehaviour
{
    [SerializeField] ItemDataList itemDataList; 

    //public List<SpawnableObject> spawnableObjects; // ������ ������Ʈ ���
    public int numberOfObjects = 10;               // ������ ������Ʈ ��
    public float range = 50.0f;                    // ������Ʈ�� ������ ����
    public float minDistanceBetweenObjects = 5.0f; // ������Ʈ �� �ּ� �Ÿ�

    private List<Vector3> spawnedPositions = new List<Vector3>();

    public void test()
    {
        for (int i = 0; i < numberOfObjects; i++)
        {
            GameObject objectToSpawn = GetRandomObjectByWeight();
            Vector3 randomPosition = GetRandomNavMeshPosition();
            if (objectToSpawn != null && randomPosition != Vector3.zero)
            {
                Instantiate(objectToSpawn, randomPosition, Quaternion.identity);
            }
        }
    }
    GameObject GetRandomObjectByWeight()
    {
        float totalWeight = 0;
        foreach (var spawnableObject in itemDataList.inventoryItemList)
        {
            totalWeight += spawnableObject.weight;
        }

        float randomValue = Random.value * totalWeight;
        float cumulativeWeight = 0;

        foreach (var spawnableObject in itemDataList.inventoryItemList)
        {
            cumulativeWeight += spawnableObject.weight;
            if (randomValue < cumulativeWeight)
            {
                return spawnableObject.ObjectPrefab;
            }
        }

        return null;
    }

    Vector3 GetRandomNavMeshPosition()
    {
        int attempts = 0;
        while (attempts < 30) // ������ �õ� Ƚ�� ����
        {
            Vector3 randomDirection = Random.onUnitSphere * Random.Range(0.5f * range, range);
            randomDirection.y = 0; // y�� ����

            randomDirection += transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, range, NavMesh.AllAreas))
            {
                if (IsFarEnoughFromOthers(hit.position))
                {
                    spawnedPositions.Add(hit.position);
                    return hit.position;
                }
            }

            attempts++;
        }
        return Vector3.zero; // ��ȿ�� ��ġ�� ã�� ���ϸ� Vector3.zero ��ȯ
    }

    bool IsFarEnoughFromOthers(Vector3 position)
    {
        foreach (Vector3 otherPosition in spawnedPositions)
        {
            if (Vector3.Distance(position, otherPosition) < minDistanceBetweenObjects)
            {
                return false;
            }
        }
        return true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}