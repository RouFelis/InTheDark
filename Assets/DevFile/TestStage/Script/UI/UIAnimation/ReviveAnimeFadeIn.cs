using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ReviveAnimeFadeIn : UIAnimation
{
    public Image fadeImage;
    public float fadeDuration = 1f;
    public float startAlpha = 1f;
    public float endAlpha = 0f;

	protected override IEnumerator PlayRoutine()
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

        FinishAnimation();
    }
}
