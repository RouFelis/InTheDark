using UnityEngine;

public class FallDeathZone : MonoBehaviour
{
    [Header("������ �±� ����")]
    [SerializeField] private string itemTag = "Item"; // ������ ������Ʈ �±�
    [SerializeField] private string playerTag = "Player"; // ������ ������Ʈ �±�
    [SerializeField] private string enemyTag = "Enemy"; // ������ ������Ʈ �±�

    private void OnTriggerEnter(Collider other)
    {
        // ���� ������Ʈ�� targetTag�� ������ ������ ����
        if (other.CompareTag(itemTag))
        {
            Destroy(other.gameObject);
        }
        if (other.CompareTag(playerTag))
		{
            var tempPlayer = other.GetComponent<Player>();
            tempPlayer.TakeDamage(100000000);
        }
        if (other.CompareTag(enemyTag))
		{
            var tempEnemy = other.GetComponent<EnemyPrototypePawn>();
            tempEnemy.TakeDamage(1000000000, null);
        }
    }
}
