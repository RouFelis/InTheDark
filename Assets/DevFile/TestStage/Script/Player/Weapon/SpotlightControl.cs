using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;

public class SpotlightControl : WeaponSystem
{
    [Header("Spotlight Settings")]
    public Light firstPersonWeaponLight; // ���� ������ �ϴ� ����
    public Light thirdPersonWeaponLight; // ���� ������ �ϴ� ����
    public float zoomedInnerAngle = 20f; // ��Ŭ�� �� Inner Range
    public float zoomedOuterAngle = 30f; // ��Ŭ�� �� Outer Range
    public float zoomedIntensity = 2000f;   // ��Ŭ�� �� Intensity

    public float defaultInnerAngle = 32f;
    public float defaultOuterAngle = 70f;
    public float defaultIntensity = 600f;

    public float zoomDuration = 0.5f;
    public float flashIntensity = 5000f;
    public float flashDuration = 0.1f;

    private Coroutine zoomCoroutine;
    private bool currentZoomState = false;

    public delegate void FlashEventHandler();
    public static event FlashEventHandler OnFlash; // �÷��� �̺�Ʈ


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
    public NetworkVariable<bool> isRightClickHeld = new NetworkVariable<bool>(false, writePerm: NetworkVariableWritePermission.Owner );
    //public NetworkVariable<bool> isRecovering = new NetworkVariable<bool>(false, writePerm: NetworkVariableWritePermission.Owner );
    //public NetworkVariable<bool> isClickBlocked = new NetworkVariable<bool>(false, writePerm: NetworkVariableWritePermission.Owner );
    //public NetworkVariable<float> recoveryDelayTimer = new NetworkVariable<float>(0f, writePerm: NetworkVariableWritePermission.Owner );


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
        if (IsOwner)
          StartCoroutine(initUI());
    }

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
	{
/*        // PlaceableItemManager ������Ʈ ã��
        while (gaugeImage == null)
        {
            GameObject obj = GameObject.Find("Battery_Icon_4_Slider");
            if (obj == null)
            {
                Debug.LogError("GameObject 'Battery_Icon_4_Slider' not found!");
            }
            else
            {
                gaugeImage = obj.GetComponent<Image>();
                Debug.Log("GameObject 'Battery_Icon_4_Slider' found!");
            }
            
            Debug.LogError("Gauge Slider�� ������� �ʾҽ��ϴ�!");
            yield return null;
        }*/

        yield return new WaitForSeconds(0.5f);

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

    private void HandleRightClickInput()
    {
/*        if (Input.GetMouseButtonDown(1) && !isClickBlocked.Value) // ��Ŭ��
        {
            isRightClickHeld.Value = true;
         //   isRecovering.Value = false;
            recoveryDelayTimer.Value = 0f;
        }

        if (Input.GetMouseButtonUp(1)) // ��Ŭ�� ����
        {
            isRightClickHeld.Value = false;
        }*/
    }

    void Update()
    {
        if (!IsOwner)
        {
            HandleFlash();

            return;
        }

        if (Input.GetMouseButtonDown(1) && !isZooming.Value && !player.IsDead) // ��Ŭ�� ����
        {
            isRightClickHeld.Value = true;
            isZooming.Value = true;
        }
        else if (Input.GetMouseButtonUp(1) && isZooming.Value && !player.IsDead) // ��Ŭ�� ����
        {
            isRightClickHeld.Value = false;
            isZooming.Value = false;
        }
        HandleFlash();
    }

    private void HandleFlash()
	{
        if (currentZoomState == isZooming.Value) return; // ���� ��ȭ�� ������ �������� ����

        currentZoomState = isZooming.Value;

        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
        }

        if (isZooming.Value)
        {
            zoomCoroutine = StartCoroutine(ZoomAndFlashEffect());
        }
        else
        {
            zoomCoroutine = StartCoroutine(ResetZoomEffect());
        }
    }
   
    IEnumerator ZoomAndFlashEffect()
    {
        yield return StartCoroutine(AdjustLight(firstPersonWeaponLight, zoomedInnerAngle, zoomedOuterAngle, zoomedIntensity, zoomDuration));
        yield return StartCoroutine(AdjustLight(thirdPersonWeaponLight, zoomedInnerAngle, zoomedOuterAngle, zoomedIntensity, zoomDuration));

        if (!isZooming.Value) // �߰��� �����Ǹ� ��� ����
        {
            StartCoroutine(ResetZoomEffect());
            yield break;
        }

        yield return StartCoroutine(FlashEffect(firstPersonWeaponLight));
        yield return StartCoroutine(FlashEffect(thirdPersonWeaponLight));
    }

    IEnumerator ResetZoomEffect()
    {
        yield return StartCoroutine(AdjustLight(firstPersonWeaponLight, defaultInnerAngle, defaultOuterAngle, defaultIntensity, zoomDuration));
        yield return StartCoroutine(AdjustLight(thirdPersonWeaponLight, defaultInnerAngle, defaultOuterAngle, defaultIntensity, zoomDuration));
    }

    IEnumerator AdjustLight(Light light, float targetInner, float targetOuter, float targetIntensity, float duration)
    {
        if (light == null) yield break;

        float time = 0f;
        float startInner = light.innerSpotAngle;
        float startOuter = light.spotAngle;
        float startIntensity = light.intensity;

        while (time < duration)
        {
            if (!isZooming.Value && targetIntensity == zoomedIntensity) yield break; // �� ���� �����Ǹ� ��� ����

            time += Time.deltaTime;
            float t = time / duration;
            float easeT = 1f - Mathf.Pow(1f - t, 3f); // �������� ȿ�� ����
            light.innerSpotAngle = Mathf.Lerp(startInner, targetInner, easeT);
            light.spotAngle = Mathf.Lerp(startOuter, targetOuter, easeT);
            light.intensity = Mathf.Lerp(startIntensity, targetIntensity, easeT);
            yield return null;
        }
    }

    IEnumerator FlashEffect(Light light)
    {
        if (light == null) yield break;
        float originalIntensity = light.intensity;
        light.intensity = flashIntensity;
        //yield return new WaitForSeconds(flashDuration);
        yield return null;
        
        light.intensity = originalIntensity;
        OnFlash?.Invoke();
    }



    #region ����� ���
   /* private void HandleGauge()
    {
        if (isRightClickHeld.Value)
        {
            DecreaseGauge(Time.deltaTime * decreaseRate);
        }
        else if (!isRecovering.Value && gaugeImage.fillAmount < maxGauge)
        {
            recoveryDelayTimer.Value += Time.deltaTime;
            if (recoveryDelayTimer.Value >= 0.5f) // delayBeforeRecovery
            {
                isRecovering.Value = true;
            }
        }

        if (isRecovering.Value)
        {
            RecoverGauge(Time.deltaTime * increaseRate);
        }

        // ������ ���¿� ���� ��Ŭ�� ���� ���� ������Ʈ
        if (gaugeImage.fillAmount < gaugeThreshold)
        {
            isClickBlocked.Value = true;
        }
        else if (gaugeImage.fillAmount >= gaugeThreshold && isClickBlocked.Value)
        {
            isClickBlocked.Value = false;
        }
    }

    private void DecreaseGauge(float amount)
    {
        gaugeImage.fillAmount -= amount;
        if (gaugeImage.fillAmount <= minGauge)
        {
            gaugeImage.fillAmount = 0f; // �������� 0 ���Ϸδ� �������� ����
            isRightClickHeld.Value = false; // ��Ŭ�� ���� ����
        }
    }

    private void RecoverGauge(float amount)
    {
        gaugeImage.fillAmount += amount;
        if (gaugeImage.fillAmount >= maxGauge)
        {
            gaugeImage.fillAmount = maxGauge; // �ִ밪���� ����
            isRecovering.Value = false;
        }
    }*/
    #endregion


    /*        private void UpdateSpotlightState()
            {
                if (isRightClickHeld.Value)
                {
                    SetLightValues(zoomedInnerAngle, zoomedOuterAngle, zoomedIntensity);
                }
                else
                {
                    SetLightValues(defaultInnerAngle, defaultOuterAngle, defaultIntensity);
                }
            }
    */
    private void SetLightValues(float innerAngle, float outerAngle, float intensity)
        {
            firstPersonWeaponLight.innerSpotAngle = innerAngle;
            firstPersonWeaponLight.spotAngle = outerAngle;
            firstPersonWeaponLight.intensity = intensity;

            thirdPersonWeaponLight.innerSpotAngle = innerAngle;
            thirdPersonWeaponLight.spotAngle = outerAngle;
            thirdPersonWeaponLight.intensity = intensity;
        }

/*        public void UpdateDefaultValues(float innerAngle, float outerAngle, float intensity)
        {
            // ��ȭ�� ���� �����͸� �޾� �⺻ ���� ������Ʈ
            defaultInnerAngle = innerAngle;
            defaultOuterAngle = outerAngle;
            defaultIntensity = intensity;

            // ��Ŭ���� �ƴ� ��� ��� ����
            if (!isRightClickHeld.Value)
            {
                SetLightValues(defaultInnerAngle, defaultOuterAngle, defaultIntensity);
            }
        }*/

}
