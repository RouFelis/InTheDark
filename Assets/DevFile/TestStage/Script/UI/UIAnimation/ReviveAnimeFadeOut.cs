using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class ReviveAnimeFadeOut : UIAnimation
{
    public Image fadeImage;
    public float fadeDuration = 1f;
    public float startAlpha = 1f;
    public float endAlpha = 0f;
    [SerializeField] private Volume postProcessingVolume;
    [SerializeField] private Vignette vignette;

    [SerializeField] private GameObject glitchVolume;

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

	protected override IEnumerator PlayRoutine()
    {
        glitchVolume.SetActive(true);
        float time = 0f;
        Color color = fadeImage.color;

        yield return new WaitForSeconds(1f);

        while (time < fadeDuration)
        {
            float t = time / fadeDuration;
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeImage.color = color;
            time += Time.deltaTime;
            yield return null;
        }

        glitchVolume.SetActive(false);
        color.a = endAlpha;
        fadeImage.color = color;
    }
}
