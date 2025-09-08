using UnityEngine;

public class FallDeathZone : MonoBehaviour
{
    [Header("삭제할 태그 설정")]
    [SerializeField] private string itemTag = "Item"; // 삭제할 오브젝트 태그
    [SerializeField] private string playerTag = "Player"; // 삭제할 오브젝트 태그
    [SerializeField] private string enemyTag = "Enemy"; // 삭제할 오브젝트 태그

    private void OnTriggerEnter(Collider other)
    {
        // 들어온 오브젝트가 targetTag를 가지고 있으면 삭제
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
