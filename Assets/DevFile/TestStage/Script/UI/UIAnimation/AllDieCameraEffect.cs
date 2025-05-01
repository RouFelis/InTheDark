using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class AllDieCameraEffect : UIAnimation
{
    [Header("Target UI")]
    [SerializeField] private RectTransform uiRoot;
    [SerializeField] private CanvasGroup uiGroup;

    [Header("Scale Settings")]
    [SerializeField] private float scaleUpSize = 1.5f;         // ó�� Ȯ�� ����
    [SerializeField] private float scaleDelay = 0.5f;          // �󸶳� ��ٸ��� (�� �پ��� ��)

    [Header("Shake Settings")]
    [SerializeField] private float shakeAmount = 20f;
    [SerializeField] private int shakeRepeatCount = 10;
    [SerializeField] private float shakeInterval = 0.05f;

    [Header("Blink Settings")]
    [SerializeField] private bool enableBlink = true;
    [SerializeField] private int blinkRepeatCount = 6;
    [SerializeField] private float blinkInterval = 0.1f;

    [SerializeField] private bool IsLastAnimation;

    private Vector3 originalScale;
    private Vector3 originalPos;


    void Start()
	{
		if (uiRoot == null || uiGroup == null)
		{
			Debug.LogError("UI Root �Ǵ� CanvasGroup�� ������� �ʾҽ��ϴ�!");
			return;
		}

		originalScale = uiRoot.localScale;
		originalPos = uiRoot.anchoredPosition3D;

        gameObject.SetActive(false);
	}

    public override void StartEffect()
	{
        base.StartEffect();

        StartCoroutine(PlayIntroSequence());
    }

    IEnumerator PlayIntroSequence()
    {
        // Step 1: Ȯ��
        uiRoot.localScale = originalScale * scaleUpSize;

        // Step 2: ���� �ð� ���
        yield return new WaitForSeconds(scaleDelay);

        // Step 3: �� ���� ���� ũ��� ���̱� (��)
        uiRoot.localScale = originalScale;

        // Step 4: ����ũ + ������
        StartCoroutine(ShakeEffect());
        if (enableBlink)
            StartCoroutine(BlinkEffect());

        float totalDuration = Mathf.Max(shakeRepeatCount * shakeInterval, blinkRepeatCount * blinkInterval);
        yield return new WaitForSeconds(totalDuration);

        // ����
        uiRoot.anchoredPosition3D = originalPos;
        uiGroup.alpha = 1f;

        yield return new WaitForSeconds(2.5f);

		if (IsLastAnimation)
		{
            ActionInvoke();
        }
    }

    IEnumerator ShakeEffect()
    {
        for (int i = 0; i < shakeRepeatCount; i++)
        {
            Vector2 offset = new Vector2(
                Random.Range(-shakeAmount, shakeAmount),
                Random.Range(-shakeAmount, shakeAmount)
            );

            uiRoot.anchoredPosition3D = originalPos + (Vector3)offset;
            yield return new WaitForSeconds(shakeInterval);
        }

        uiRoot.anchoredPosition3D = originalPos;
    }

    IEnumerator BlinkEffect()
    {
        for (int i = 0; i < blinkRepeatCount; i++)
        {
            uiGroup.alpha = (i % 2 == 0) ? 0f : 1f;
            yield return new WaitForSeconds(blinkInterval);
        }

        uiGroup.alpha = 1f;
    }
}
