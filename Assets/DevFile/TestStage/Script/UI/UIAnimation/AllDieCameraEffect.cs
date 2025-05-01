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
    [SerializeField] private float scaleUpSize = 1.5f;         // 처음 확대 비율
    [SerializeField] private float scaleDelay = 0.5f;          // 얼마나 기다릴지 (툭 줄어들기 전)

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
			Debug.LogError("UI Root 또는 CanvasGroup이 연결되지 않았습니다!");
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
        // Step 1: 확대
        uiRoot.localScale = originalScale * scaleUpSize;

        // Step 2: 일정 시간 대기
        yield return new WaitForSeconds(scaleDelay);

        // Step 3: 한 번에 원래 크기로 줄이기 (툭)
        uiRoot.localScale = originalScale;

        // Step 4: 쉐이크 + 깜빡임
        StartCoroutine(ShakeEffect());
        if (enableBlink)
            StartCoroutine(BlinkEffect());

        float totalDuration = Mathf.Max(shakeRepeatCount * shakeInterval, blinkRepeatCount * blinkInterval);
        yield return new WaitForSeconds(totalDuration);

        // 복원
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
