using UnityEngine;
using System.Collections;

public class UIToggleSlide : MonoBehaviour
{
    public CanvasGroup uiCanvasGroup;
    public RectTransform uiTransform;
    public float flashDuration = 0.2f;
    public float transitionDuration = 0.4f;
    public AnimationCurve flashCurve;
    public AnimationCurve scaleCurve;

    public AudioSource audioSource;
    public AudioClip tvOnClip;
    public AudioClip tvOffClip;

    private Vector3 originalScale;
    private bool isOn = false;
    private bool isTransitioning = false;
    private KeyCode interacteKey;


    void Start()
    {
        originalScale = uiTransform.localScale;
        uiCanvasGroup.alpha = 0f;
        uiTransform.localScale = new Vector3(originalScale.x, 0.01f, originalScale.z);
        SetKey();
        KeySettingsManager.Instance.KeyCodeChanged += SetKey;
    }

    void Update()
    {
        var menu = MenuManager.Instance;

        if (Input.GetKeyDown(interacteKey) && !isTransitioning && !menu.IsPaused && !menu.IsEvenet)
        {
            if (isOn)
                StartCoroutine(TurnOffEffect());
            else
                StartCoroutine(TurnOnEffect());
        }
    }


    private void SetKey()
	{
        interacteKey = KeySettingsManager.Instance.ShopKey;
    }

    public void TurnOffButton()
	{
        StartCoroutine(TurnOffEffect());
    }


    System.Collections.IEnumerator TurnOnEffect()
    {
        isTransitioning = true;

        if (tvOnClip && audioSource)
            audioSource.PlayOneShot(tvOnClip);

        MenuManager.Instance.SetPause(true);

        // Flash
        float timer = 0f;
        while (timer < flashDuration)
        {
            timer += Time.deltaTime;
            float t = timer / flashDuration;
            uiCanvasGroup.alpha = flashCurve.Evaluate(t);
            yield return null;
        }
        uiCanvasGroup.alpha = 1f;

        // Expand
        timer = 0f;
        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = timer / transitionDuration;
            float scaleY = scaleCurve.Evaluate(t);
            uiTransform.localScale = new Vector3(originalScale.x, scaleY, originalScale.z);
            yield return null;
        }

        uiTransform.localScale = originalScale;
        isOn = true;
        isTransitioning = false;
    }

    System.Collections.IEnumerator TurnOffEffect()
    {
        isTransitioning = true;

        if (tvOffClip && audioSource)
            audioSource.PlayOneShot(tvOffClip);

        MenuManager.Instance.SetPause(false);

        // Shrink
        float timer = 0f;
        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = timer / transitionDuration;
            float scaleY = 1 - scaleCurve.Evaluate(t);
            uiTransform.localScale = new Vector3(originalScale.x, Mathf.Max(scaleY, 0.01f), originalScale.z);
            uiCanvasGroup.alpha = 1f - t; // fade out
            yield return null;
        }

        uiCanvasGroup.alpha = 0f;
        uiTransform.localScale = new Vector3(originalScale.x, 0.01f, originalScale.z);
        isOn = false;
        isTransitioning = false;
    }
}
