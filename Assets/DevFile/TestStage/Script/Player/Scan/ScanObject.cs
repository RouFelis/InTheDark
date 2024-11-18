using UnityEngine;

public class ScanObject : MonoBehaviour
{
    public float growSpeed;
    public float lifeTime;
    public float currentLifeTime = 0;


	// 초기화 메서드로 속도와 지속 시간을 설정
	public void Initialize(float growSpeed, float lifeTime)
    {
        this.growSpeed = growSpeed;
        this.lifeTime = lifeTime;
        currentLifeTime = 0;
    }

    private void Update()
    {
        // 시간이 지남에 따라 오브젝트 크기 증가
        transform.localScale += Vector3.one * growSpeed * Time.deltaTime;

        // 지속 시간을 초과하면 오브젝트 삭제
        currentLifeTime += Time.deltaTime;
        if (currentLifeTime >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}
