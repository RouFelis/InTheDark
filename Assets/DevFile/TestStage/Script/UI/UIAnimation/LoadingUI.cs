using UnityEngine;
using System.Collections;
using TMPro;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private RectTransform rotatingImage;
    [SerializeField] private TMP_Text loadingText; // TMP_Text 로 바꿀 경우 주석 해제

    private Coroutine loadingCoroutine;
    [SerializeField] private float rotationStep = 30f;
    [SerializeField] private float interval = 0.2f;

    private void OnEnable()
    {
        loadingCoroutine = StartCoroutine(LoadingLoop());
    }

    private void OnDisable()
    {
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }
    }

    private IEnumerator LoadingLoop()
    {
        int dotCount = 0;
        float currentRotation = 0f;

		while (true)
		{
			// 회전
			currentRotation += rotationStep;
			rotatingImage.localRotation = Quaternion.Euler(0f, 0f, -currentRotation);

			// 점 갱신
			dotCount = (dotCount % 3) + 1;
			loadingText.text = "NowLoading" + new string('.', dotCount);

			yield return new WaitForSeconds(interval);
		}
    }
}
