using UnityEngine;

public class ObjectLifeTimer : MonoBehaviour
{
    [SerializeField] private float destroyTime = 3f; // 인스펙터에서 시간 조정 가능

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }
}
