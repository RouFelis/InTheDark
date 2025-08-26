using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GameFailAnimation : UIAnimation
{
    [Header("AudioSource")]
    [SerializeField] private AudioSource alarmSource;
    [SerializeField] private AudioSource otherSource;
    [SerializeField] private AudioClip alarmSound;
    [SerializeField] private AudioClip doorSound;
    [SerializeField] private AudioClip gunSound;

    [Header("Light")]
    [SerializeField] private Light warningLight_1;
    [SerializeField] private Light warningLight_2;
    [SerializeField] private float onTime = 0.2f;   // �����ִ� �ð�
    [SerializeField] private float offTime = 0.2f;  // �����ִ� �ð�
    [SerializeField] private float maxIntensity = 5f;

    [SerializeField] private Light roomLight_1;
    [SerializeField] private Light roomLight_2;
    [SerializeField] private float roomLightIntensity = 5f;

    private Coroutine blinkCoroutine_1;
    private Coroutine blinkCoroutine_2;

    private void Start()
    {
        warningLight_1.intensity = 0f;
        warningLight_2.intensity = 0f;
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
    }
}
