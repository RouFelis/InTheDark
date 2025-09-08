using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class GameFailAnimation : UIAnimation
{
    [Header("AudioSource")]
    [SerializeField] private AudioSource alarmSource;
    [SerializeField] private AudioSource otherSource;
    [SerializeField] private AudioClip alarmSound;
    [SerializeField] private AudioClip lightOffSound;
    [SerializeField] private AudioClip gasSound;
    [SerializeField] private AudioClip gameOverVoiceSound;

    [Header("Light")]
    [SerializeField] private Light warningLight_1;
    [SerializeField] private Light warningLight_2;
    [SerializeField] private float onTime = 0.2f;   // 켜져있는 시간
    [SerializeField] private float offTime = 0.2f;  // 꺼져있는 시간
    [SerializeField] private float maxIntensity = 5f;

    [SerializeField] private Light roomLight_1;
    [SerializeField] private Light roomLight_2;
    [SerializeField] private float roomLightIntensity = 5f;

    [Header("fog")]
    [SerializeField] private Volume volume; // HDRP Volume 오브젝트 넣어주기
    [SerializeField] private float fogFloat = 0;
    private Fog fog;



    private Coroutine blinkCoroutine_1;
    private Coroutine blinkCoroutine_2;

    private void Start()
    {
        warningLight_1.intensity = 0f;
        warningLight_2.intensity = 0f;

        // VolumeProfile에서 Fog 컴포넌트 가져오기
        if (volume.profile.TryGet(out fog))
        {
            Debug.Log("Fog component found!");
        }
        else
        {
            Debug.LogWarning("Fog component not found in this volume profile.");
        }
    }

    public void SetFogDensity(float density)
    {
        if (fog != null)
        {
            fog.meanFreePath.value = Mathf.Lerp(5f, 500f, 1f - density);
        }
    }


    public IEnumerator GraduallyIncreaseFog(float duration)
    {
        float startDensity = 0.1f;
        float endDensity = 0.999f;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentDensity = Mathf.Lerp(startDensity, endDensity, t);

            SetFogDensity(currentDensity);

            yield return null;
        }

        // 마지막 값 보정
        SetFogDensity(endDensity);
    }

    private IEnumerator BlinkLight(Light light)
    {
        while (true)
        {
            // 켜짐
            light.intensity = maxIntensity;
            yield return new WaitForSeconds(onTime);

            // 꺼짐
            light.intensity = 0f;
            yield return new WaitForSeconds(offTime);
        }
    }

    protected override IEnumerator PlayRoutine()
    {
        // 1. 빨간 경광등, 경보음 시작
        blinkCoroutine_1 = StartCoroutine(BlinkLight(warningLight_1));
        blinkCoroutine_2 = StartCoroutine(BlinkLight(warningLight_2));

        alarmSource.clip = alarmSound;
        alarmSource.loop = true;
        alarmSource.Play();
        yield return new WaitForSeconds(2f);

        roomLight_1.intensity = 0;
        roomLight_2.intensity = 0;
        otherSource.PlayOneShot(lightOffSound);
        yield return new WaitForSeconds(3f);

        otherSource.PlayOneShot(gasSound);
   
        //0.1에서 0.999까지 점진적 증가하도록...
        StartCoroutine(GraduallyIncreaseFog(5f));

        yield return new WaitForSeconds(1f);

        //2. 글리치
        UIAnimationManager.Instance.Glitch(true);


        otherSource.PlayOneShot(gameOverVoiceSound);

        yield return new WaitForSeconds(gameOverVoiceSound.length + 1f);


        //3. fade out
        UIAnimationManager.Instance.FadeOutAnimation();

        yield return new WaitForSeconds(1f);


        // 6. 빨간 경광등, 경보음  종료
        if (blinkCoroutine_1 != null)
        {
            StopCoroutine(blinkCoroutine_1);
            blinkCoroutine_1 = null;
        }
        if (blinkCoroutine_2 != null)
        {
            StopCoroutine(blinkCoroutine_2);
            blinkCoroutine_2 = null;
        }

        warningLight_1.intensity = 0f;
        warningLight_2.intensity = 0f;

        alarmSource.Stop();
        otherSource.Stop();

        SetFogDensity(0.1f);

        yield return new WaitForSeconds(3f);


        roomLight_1.intensity = roomLightIntensity;
        roomLight_2.intensity = roomLightIntensity;

        // 7. fade in
        UIAnimationManager.Instance.FadeInAnimation();
        UIAnimationManager.Instance.HealthbarOff();


        PlayersManager.Instance.AllPlayerSetPos();


        Debug.Log("부활 완료!");
    }

    /*  protected override IEnumerator PlayRoutine()
      {
          // 1. 빨간 경광등, 경보음 시작
          blinkCoroutine_1 = StartCoroutine(BlinkLight(warningLight_1));
          blinkCoroutine_2 = StartCoroutine(BlinkLight(warningLight_2));

          roomLight_1.intensity = 0;
          roomLight_2.intensity = 0;

          alarmSource.clip = alarmSound;
          alarmSource.loop = true;
          alarmSource.Play();
          yield return new WaitForSeconds(3f);

          //2. 글리치
          UIAnimationManager.Instance.Glitch(true);
          yield return new WaitForSeconds(1f);

          //3. fade out
          UIAnimationManager.Instance.FadeOutAnimation();

          otherSource.PlayOneShot(doorSound);
          //3. 문 열리는 소리
          yield return new WaitForSeconds(4f);

          // 5. 총소리 (플레이어들의 죽음 암시)
          otherSource.PlayOneShot(gunSound);
          yield return new WaitForSeconds(6f);

          // 6. 빨간 경광등, 경보음  종료
          if (blinkCoroutine_1 != null)
          {
              StopCoroutine(blinkCoroutine_1);
              blinkCoroutine_1 = null;
          }
          if (blinkCoroutine_2 != null)
          {
              StopCoroutine(blinkCoroutine_2);
              blinkCoroutine_2 = null;
          }
          warningLight_1.intensity = 0f;
          warningLight_2.intensity = 0f;
          alarmSource.Stop();

          yield return new WaitForSeconds(3f);


          roomLight_1.intensity = roomLightIntensity;
          roomLight_2.intensity = roomLightIntensity;

          // 7. fade in
          UIAnimationManager.Instance.FadeInAnimation();
          UIAnimationManager.Instance.HealthbarOff();


          PlayersManager.Instance.AllPlayerSetPos();


          Debug.Log("부활 완료!");
      }*/
}
