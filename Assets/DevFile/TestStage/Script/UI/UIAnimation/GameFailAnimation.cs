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
    [SerializeField] private float onTime = 0.2f;   // �����ִ� �ð�
    [SerializeField] private float offTime = 0.2f;  // �����ִ� �ð�
    [SerializeField] private float maxIntensity = 5f;

    [SerializeField] private Light roomLight_1;
    [SerializeField] private Light roomLight_2;
    [SerializeField] private float roomLightIntensity = 5f;

    [Header("fog")]
    [SerializeField] private Volume volume; // HDRP Volume ������Ʈ �־��ֱ�
    [SerializeField] private float fogFloat = 0;
    private Fog fog;



    private Coroutine blinkCoroutine_1;
    private Coroutine blinkCoroutine_2;

    private void Start()
    {
        warningLight_1.intensity = 0f;
        warningLight_2.intensity = 0f;

        // VolumeProfile���� Fog ������Ʈ ��������
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

        // ������ �� ����
        SetFogDensity(endDensity);
    }

    private IEnumerator BlinkLight(Light light)
    {
        while (true)
        {
            // ����
            light.intensity = maxIntensity;
            yield return new WaitForSeconds(onTime);

            // ����
            light.intensity = 0f;
            yield return new WaitForSeconds(offTime);
        }
    }

    protected override IEnumerator PlayRoutine()
    {
        // 1. ���� �汤��, �溸�� ����
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
   
        //0.1���� 0.999���� ������ �����ϵ���...
        StartCoroutine(GraduallyIncreaseFog(5f));

        yield return new WaitForSeconds(1f);

        //2. �۸�ġ
        UIAnimationManager.Instance.Glitch(true);


        otherSource.PlayOneShot(gameOverVoiceSound);

        yield return new WaitForSeconds(gameOverVoiceSound.length + 1f);


        //3. fade out
        UIAnimationManager.Instance.FadeOutAnimation();

        yield return new WaitForSeconds(1f);


        // 6. ���� �汤��, �溸��  ����
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


        Debug.Log("��Ȱ �Ϸ�!");
    }

    /*  protected override IEnumerator PlayRoutine()
      {
          // 1. ���� �汤��, �溸�� ����
          blinkCoroutine_1 = StartCoroutine(BlinkLight(warningLight_1));
          blinkCoroutine_2 = StartCoroutine(BlinkLight(warningLight_2));

          roomLight_1.intensity = 0;
          roomLight_2.intensity = 0;

          alarmSource.clip = alarmSound;
          alarmSource.loop = true;
          alarmSource.Play();
          yield return new WaitForSeconds(3f);

          //2. �۸�ġ
          UIAnimationManager.Instance.Glitch(true);
          yield return new WaitForSeconds(1f);

          //3. fade out
          UIAnimationManager.Instance.FadeOutAnimation();

          otherSource.PlayOneShot(doorSound);
          //3. �� ������ �Ҹ�
          yield return new WaitForSeconds(4f);

          // 5. �ѼҸ� (�÷��̾���� ���� �Ͻ�)
          otherSource.PlayOneShot(gunSound);
          yield return new WaitForSeconds(6f);

          // 6. ���� �汤��, �溸��  ����
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


          Debug.Log("��Ȱ �Ϸ�!");
      }*/
}
