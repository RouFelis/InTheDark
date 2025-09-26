using UnityEngine;

public class ObjectLifeTimer : MonoBehaviour
{
    [SerializeField] private float destroyTime = 3f; // �ν����Ϳ��� �ð� ���� ����

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }
}
