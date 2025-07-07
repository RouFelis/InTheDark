using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeAnimation : UIAnimation
{
    public Image fadeImage;
    public float fadeDuration = 1f;

    private void Awake()
    {
        if (fadeImage != null)
        {
            // 시작 시 완전히 투명하게
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
        }
    }

    public override void StartEffect()
	{
        StartCoroutine(FadeInOut());
    }

	private IEnumerator FadeInOut()
	{
		FadeIn();

		yield return new WaitForSeconds(1.5f);

        FadeOut();
    }

    public void FadeIn()
    {
        StartCoroutine(Fade(0f, 3f));
    }

    public void FadeOut()
    {
        StartCoroutine(Fade(5f, 0f));
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float time = 0f;
        Color color = fadeImage.color;

        while (time < fadeDuration)
        {
            float t = time / fadeDuration;
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeImage.color = color;
            time += Time.deltaTime;
            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color;
    }
}
