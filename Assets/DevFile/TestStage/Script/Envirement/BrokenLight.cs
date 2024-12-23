using UnityEngine;
using System.Collections;

public class BrokenLight : MonoBehaviour
{
    public Light lightSource; // ���� �ҽ�
    public float minIntensity = 0.0f; // �ּ� ���
    public float maxIntensity = 2.0f; // �ִ� ���
    public int minFlickerCount = 2; // �ּ� ������ Ƚ��
    public int maxFlickerCount = 5; // �ִ� ������ Ƚ��
    public float flickerTotalDuration = 1.0f; // ������ �� ���� �ð�
    public float delayAfterFlicker = 2.0f; // ������ �� ��� �ð�

    void Start()
    {
        if (lightSource == null)
        {
            lightSource = GetComponent<Light>();
        }

        StartCoroutine(FlickerLightRoutine());
    }

    private IEnumerator FlickerLightRoutine()
    {
        while (true)
        {
            // ���� ������ Ƚ��
            int flickerCount = Random.Range(minFlickerCount, maxFlickerCount);

            // �� �������� ���� ��� (�� ���� �ð� / ������ Ƚ��)
            float flickerInterval = flickerTotalDuration / (flickerCount * 2); // On/Off ���� ����

            for (int i = 0; i < flickerCount; i++)
            {
                // ������ �Ѱ� ���� ���� ����
                lightSource.intensity = Random.Range(minIntensity, maxIntensity);
                yield return new WaitForSeconds(flickerInterval);

                // ������ ��
                lightSource.intensity = 0f;
                yield return new WaitForSeconds(flickerInterval);
            }

            // ������ �� ���
            lightSource.intensity = maxIntensity; // ������ ���� ����
            yield return new WaitForSeconds(delayAfterFlicker);
        }
    }
}
