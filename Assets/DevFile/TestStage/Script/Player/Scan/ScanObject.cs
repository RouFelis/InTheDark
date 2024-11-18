using UnityEngine;

public class ScanObject : MonoBehaviour
{
    public float growSpeed;
    public float lifeTime;
    public float currentLifeTime = 0;


	// �ʱ�ȭ �޼���� �ӵ��� ���� �ð��� ����
	public void Initialize(float growSpeed, float lifeTime)
    {
        this.growSpeed = growSpeed;
        this.lifeTime = lifeTime;
        currentLifeTime = 0;
    }

    private void Update()
    {
        // �ð��� ������ ���� ������Ʈ ũ�� ����
        transform.localScale += Vector3.one * growSpeed * Time.deltaTime;

        // ���� �ð��� �ʰ��ϸ� ������Ʈ ����
        currentLifeTime += Time.deltaTime;
        if (currentLifeTime >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}
