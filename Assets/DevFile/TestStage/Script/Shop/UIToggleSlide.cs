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
		if (PlayersManager.Instance.myPlayerDead)
		{
			if (isOn)
			{
                StartCoroutine(TurnOffEffect());
                MenuManager.Instance.SetPause(false);
            }
            return;
		}


        if (Input.GetKeyDown(interacteKey) && !isTransitioning)
        {
            ToggleTV();
        }
    }

    public void ToggleTV()
	{
        if (isOn)
        {
            StartCoroutine(TurnOffEffect());
            MenuManager.Instance.IsEvenet = false;
            MenuManager.Instance.SetPause(false);
        }
        else
        {
            StartCoroutine(TurnOnEffect());
            MenuManager.Instance.IsEvenet = true;
            MenuManager.Instance.SetPause(true);
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


    //화면 켜기
    System.Collections.IEnumerator TurnOnEffect()
    {
        isTransitioning = true;

        if (tvOnClip && audioSource)
            audioSource.PlayOneShot(tvOnClip); //오디오 넣고

        MenuManager.Instance.SetPause(true);

        // 번쩍이면서 켜지게끔
        float timer = 0f;
        while (timer < flashDuration)
        {
            timer += Time.deltaTime;
            float t = timer / flashDuration;
            uiCanvasGroup.alpha = flashCurve.Evaluate(t);
            yield return null;
        }
        uiCanvasGroup.alpha = 1f;

        // 여기서 확장시키기
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

    //화면끄기
    System.Collections.IEnumerator TurnOffEffect()
    {
        isTransitioning = true;

        if (tvOffClip && audioSource)
            audioSource.PlayOneShot(tvOffClip); //오디오 넣고

        MenuManager.Instance.SetPause(false);

        // Shrink
        float timer = 0f;
        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = timer / transitionDuration;
            float scaleY = 1 - scaleCurve.Evaluate(t);
            uiTransform.localScale = new Vector3(originalScale.x, Mathf.Max(scaleY, 0.01f), originalScale.z);
            uiCanvasGroup.alpha = 1f - t; // 페이드 아웃!
            yield return null;
        }

        //알파값 0으로 맞춰서 안보이게 하기.
        uiCanvasGroup.alpha = 0f;
        uiTransform.localScale = new Vector3(originalScale.x, 0.01f, originalScale.z);
        isOn = false;
        isTransitioning = false;
    }
}
