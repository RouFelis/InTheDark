using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;
using SaintsField.Playa;
using SaintsField;

public class SpotlightControl : WeaponSystem
{
    [Header("Spotlight Settings")]
    [LayoutStart("Spotlight Settings", ELayout.FoldoutBox)]

    public Light firstPersonWeaponLight, thirdPersonWeaponLight;

    public float zoomedInnerAngle = 20f, zoomedOuterAngle = 30f, zoomedIntensity = 2000f;
    public float defaultInnerAngle = 32f, defaultOuterAngle = 70f, defaultIntensity = 600f;

    public float maxZoomDuration = 5.0f, maxResetDuration = 2.0f;
    public float zoomSpeedMultiplier = 1.0f, resetSpeedMultiplier = 1.0f;
    public float flashIntensity = 5000f, flashOuterAngle = 180f;
    public float flashExpandDuration = 0.2f, flashFadeDuration = 0.5f;

    public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve resetCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve recoveryCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // �߰��� �κ�: ���� ���� �ӵ� Ŀ��

    [Header("Audio Settings")]
    [LayoutStart("Audio Settings", ELayout.FoldoutBox)]
    public AudioSource audioSource;
    public AudioClip zoomLoopClip, flashSoundClip;
    public float minPitch = 0.8f, maxPitch = 1.5f;

    public delegate void FlashEventHandler();
    public static event FlashEventHandler OnFlash;

    [Header("Color Settings")] // �߰��� �κ�
    [LayoutStart("Color Settings", ELayout.FoldoutBox)]
    public Color defaultColor = Color.white;
    public Color zoomedColor = Color.red;
    public Color flashColor = Color.yellow; 


    //private CanvasGroup flashPanel;
    private bool isResetting = false,  hasFlashed = false;
    private float zoomProgress = 0f, lastEaseT = -1f;


    #region ����� ���
    /*    [Header("Gauge Settings")]
        public Image gaugeImage; // ������ UI (Slider ������Ʈ)
        public float decreaseRate = 0.1f; // �ʴ� ������
        public float increaseRate = 0.1f; // �ʴ� ������
        public float delayBeforeRecovery = 0.5f; // ȸ�� ��� �ð� (��)
        public float minGauge = 0.001f; // �ּ� ������ ��
        public float maxGauge = 1f; // �ִ� ������ ��
        public float gaugeThreshold = 0.2f; // 10% (0.1) ���� ���� ��*/
    #endregion


    public NetworkVariable<bool> isZooming = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isFlashing = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isRightClickHeld = new NetworkVariable<bool>(false, writePerm: NetworkVariableWritePermission.Owner );


    public override void Start()
    {
        base.Start();
        // Spotlight �ʱ�ȭ Ȯ��
        if (thirdPersonWeaponLight == null)
        {
            Debug.LogError("Weapon Light�� ������� �ʾҽ��ϴ�!");
        }
        if (firstPersonWeaponLight == null)
        {
            Debug.LogError("Weapon Light�� ������� �ʾҽ��ϴ�!");
        }
        //if (IsOwner)
          StartCoroutine(initUI());
        if (!IsOwner)
            isFlashing.OnValueChanged += OtherClientTriggerFlashEffect;
    }

    //��� �ٲ㼭 ���
/*    void Update()
    {
        if (IsOwner&& !player.IsDead)
        {
            HandleRightClickInput();           
        }
       // UpdateSpotlightState();
    }
*/

    private IEnumerator initUI()
	{/*
        while (flashPanel == null)
        {
			try
			{
                flashPanel = GameObject.Find("flashPanel").GetComponent<CanvasGroup>();
                flashPanel.gameObject.SetActive(false);
                Debug.Log("Find flashPanel");
            }
            catch
			{
                Debug.Log("Serch flashPanel...");
            }
            yield return null;
        }*/

        yield return null;
        SetLightValues(defaultInnerAngle, defaultOuterAngle, defaultIntensity);

        if (IsOwner)
        {
            thirdPersonWeaponLight.gameObject.SetActive(false);
        }
        else
        {
            firstPersonWeaponLight.gameObject.SetActive(false);
        }

    }

    public void ToogleLight()
	{
        thirdPersonWeaponLight.gameObject.SetActive(!thirdPersonWeaponLight.gameObject.activeInHierarchy);
        firstPersonWeaponLight.gameObject.SetActive(!firstPersonWeaponLight.gameObject.activeInHierarchy);
    }

/*    private void HandleRightClickInput()
    {
        if (Input.GetMouseButtonDown(1) && !isClickBlocked.Value) // ��Ŭ��
        {
            isRightClickHeld.Value = true;
         //   isRecovering.Value = false;
            recoveryDelayTimer.Value = 0f;
        }

        if (Input.GetMouseButtonUp(1)) // ��Ŭ�� ����
        {
            isRightClickHeld.Value = false;
        }
    }*/


    void Update()
    {
        OtherClientZoomHandle();
        ZoomHandle();

        float t = Mathf.Clamp01(zoomProgress / maxZoomDuration);
        float easeT = isZooming.Value ? zoomCurve.Evaluate(t) : resetCurve.Evaluate(t);
        if (!Mathf.Approximately(easeT, lastEaseT))
        {
            lastEaseT = easeT;
            ApplyZoom(easeT);
            HandleZoomAudio(easeT);
        }
    }



    private void OtherClientZoomHandle()
	{
        if (!IsOwner)
        {
            if (isResetting || isFlashing.Value) return;

            if (isZooming.Value)
            {
                zoomProgress = Mathf.Min(zoomProgress + Time.deltaTime * zoomSpeedMultiplier, maxZoomDuration);
                if (zoomProgress >= maxZoomDuration && !hasFlashed)
                {
                    hasFlashed = true;
                    
                }
            }
            else
            {
                zoomProgress = Mathf.Max(zoomProgress - Time.deltaTime * resetSpeedMultiplier, 0f);
                if (zoomProgress <= 0.01f && !isFlashing.Value) hasFlashed = false;
            }
        }
    }

    private void OtherClientTriggerFlashEffect(bool previous, bool current)
    {
        if (!current)
            return;

        OnFlash?.Invoke();
        StartCoroutine(PlayFlashSoundAfterDelay(0.01f));
        StartCoroutine(FlashEffect(firstPersonWeaponLight));
        StartCoroutine(FlashEffect(thirdPersonWeaponLight));
    }


    private void ZoomHandle()
	{
        if (!IsOwner || isResetting || isFlashing.Value) return;

        if (Input.GetMouseButton(1))
        {
            isZooming.Value = true;
            zoomProgress = Mathf.Min(zoomProgress + Time.deltaTime * zoomSpeedMultiplier, maxZoomDuration);
            if (zoomProgress >= maxZoomDuration && !hasFlashed)
            {
                hasFlashed = true;
                TriggerFlashEffect();
            }
        }
        else
        {
            isZooming.Value = false;
            zoomProgress = Mathf.Max(zoomProgress - Time.deltaTime * resetSpeedMultiplier, 0f);
            if (zoomProgress <= 0.01f && !isFlashing.Value) hasFlashed = false;
        }

    }


    private void ApplyZoom(float t)
    {
        float targetInner = Mathf.Lerp(defaultInnerAngle, zoomedInnerAngle, t);
        float targetOuter = Mathf.Lerp(defaultOuterAngle, zoomedOuterAngle, t);
        float targetIntensity = Mathf.Lerp(defaultIntensity, zoomedIntensity, t);

        Color targetColor = Color.Lerp(defaultColor, zoomedColor, t); // �߰��� �κ� : ���� �� ���� ����

        firstPersonWeaponLight.innerSpotAngle = thirdPersonWeaponLight.innerSpotAngle = targetInner;
        firstPersonWeaponLight.spotAngle = thirdPersonWeaponLight.spotAngle = targetOuter;
        firstPersonWeaponLight.intensity = thirdPersonWeaponLight.intensity = targetIntensity;
        firstPersonWeaponLight.color = thirdPersonWeaponLight.color = targetColor; // �߰��� �κ� : ���� �Ϸ�Ǹ� ���� ����

        //3��Ī
        thirdPersonWeaponLight.innerSpotAngle = thirdPersonWeaponLight.innerSpotAngle = targetInner;
        thirdPersonWeaponLight.spotAngle = thirdPersonWeaponLight.spotAngle = targetOuter;
        thirdPersonWeaponLight.intensity = thirdPersonWeaponLight.intensity = targetIntensity;
        thirdPersonWeaponLight.color = thirdPersonWeaponLight.color = targetColor; 
    }



    private void HandleZoomAudio(float t)
    {
        if (audioSource == null) return;

        if (isZooming.Value)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = zoomLoopClip;
                audioSource.loop = true;
                audioSource.Play();
            }
            audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, t);
        }
        else if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void TriggerFlashEffect()
    {
        OnFlash?.Invoke();
        isFlashing.Value = true;
        isZooming.Value = false;
        StartCoroutine(PlayFlashSoundAfterDelay(0.01f));
        StartCoroutine(FlashEffect(firstPersonWeaponLight));
        StartCoroutine(FlashEffect(thirdPersonWeaponLight));
        StartCoroutine(DisableZoomDuringFlash());
    }

    private IEnumerator PlayFlashSoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (audioSource.isPlaying)
            audioSource.Stop();

        if (audioSource != null && flashSoundClip != null)
        {
            audioSource.PlayOneShot(flashSoundClip);
        }
    }

    private IEnumerator FlashEffect(Light light)
    {
        if (light == null) yield break;


        for (float time = 0f; time < flashExpandDuration; time += Time.deltaTime)
        {
            float t = time / flashExpandDuration;
            light.intensity = Mathf.Lerp(defaultIntensity, flashIntensity, t);
            light.spotAngle = Mathf.Lerp(defaultOuterAngle, flashOuterAngle, t);
            light.color = flashColor;// �߰��� �κ�: �÷��� Ȯ�� �� ���� ����
            yield return null;
        }

      //  StartCoroutine(ScreenFlashEffect());

        yield return new WaitForSeconds(0.1f);

        for (float time = 0f; time < flashFadeDuration; time += Time.deltaTime)
        {
            float t = time / flashFadeDuration;
            float colorT = recoveryCurve.Evaluate(t); // �߰��� �κ�: ���� ���� �ӵ� ����

            light.intensity = Mathf.Lerp(flashIntensity, defaultIntensity, t);
            light.spotAngle = Mathf.Lerp(flashOuterAngle, defaultOuterAngle, t);
            light.color = Color.Lerp(flashColor, defaultColor, t); // �߰��� �κ�: ���� �������� ����
            yield return null;
        }
    }

    private IEnumerator DisableZoomDuringFlash()
    {
        isResetting = true;
        yield return new WaitForSeconds(flashExpandDuration + flashFadeDuration + 0.1f);
        zoomProgress = 0;
        hasFlashed = false;
        isFlashing.Value = false;
        isResetting = false;
    }

 /*   private IEnumerator ScreenFlashEffect()
    {
        float duration = 3f;
        float elapsed = 0f;

        flashPanel.gameObject.SetActive(true);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            flashPanel.alpha = Mathf.Lerp(0.2f, 0f, elapsed / duration);
            yield return null;
        }

        flashPanel.gameObject.SetActive(false);
        flashPanel.alpha = 0f;
    }*/

    private void SetLightValues(float innerAngle, float outerAngle, float intensity)
    {
        firstPersonWeaponLight.innerSpotAngle = innerAngle;
        firstPersonWeaponLight.spotAngle = outerAngle;
        firstPersonWeaponLight.intensity = intensity;

        thirdPersonWeaponLight.innerSpotAngle = innerAngle;
        thirdPersonWeaponLight.spotAngle = outerAngle;
        thirdPersonWeaponLight.intensity = intensity;
    }



}
