using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class DieCameraEffect : UIAnimation
{
    [Header("Target UI")]
    [SerializeField] private RectTransform uiRoot;
    [SerializeField] private CanvasGroup uiGroup;

    [Header("Scale Settings")]
    [SerializeField] private float scaleUpSize = 1.5f;         // 처음 확대 비율
    [SerializeField] private float scaleDelay = 0.5f;          // 얼마나 기다릴지 (툭 줄어들기 전)

    [SerializeField] private bool IsLastAnimation;

    private Vector3 originalScale;


    void Start()
	{
		if (uiRoot == null || uiGroup == null)
		{
			Debug.LogError("UI Root 또는 CanvasGroup이 연결되지 않았습니다!");
			return;
		}

		originalScale = uiRoot.localScale;
        gameObject.SetActive(false);
	}


    protected override IEnumerator PlayRoutine()
    {
        // Step 1: 확대
        uiRoot.localScale = originalScale * scaleUpSize;

        // Step 2: 일정 시간 대기
        yield return new WaitForSeconds(scaleDelay);

        // Step 3: 한 번에 원래 크기로 줄이기 (툭)
        uiRoot.localScale = originalScale;

        yield return new WaitForSeconds(2f);

        if (IsLastAnimation)
        {
            FinishAnimation(true);
        }
        FinishAnimation();
    }

}
