using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;

public class SpotlightControl : WeaponSystem
{
    [Header("Spotlight Settings")]
    public Light firstPersonWeaponLight; // 무기 역할을 하는 조명
    public Light thirdPersonWeaponLight; // 무기 역할을 하는 조명
    public float zoomedInnerAngle = 20f; // 우클릭 시 Inner Range
    public float zoomedOuterAngle = 30f; // 우클릭 시 Outer Range
    public float zoomedIntensity = 2000f;   // 우클릭 시 Intensity

    public float defaultInnerAngle = 32f;
    public float defaultOuterAngle = 70f;
    public float defaultIntensity = 600f;

    public float zoomDuration = 0.5f;
    public float flashIntensity = 5000f;
    public float flashDuration = 0.1f;

    private Coroutine zoomCoroutine;
    private bool currentZoomState = false;

    public delegate void FlashEventHandler();
    public static event FlashEventHandler OnFlash; // 플래시 이벤트


    #region 사라진 기능
    /*    [Header("Gauge Settings")]
        public Image gaugeImage; // 게이지 UI (Slider 컴포넌트)
        public float decreaseRate = 0.1f; // 초당 감소율
        public float increaseRate = 0.1f; // 초당 증가율
        public float delayBeforeRecovery = 0.5f; // 회복 대기 시간 (초)
        public float minGauge = 0.001f; // 최소 게이지 값
        public float maxGauge = 1f; // 최대 게이지 값
        public float gaugeThreshold = 0.2f; // 10% (0.1) 이하 제한 값*/
    #endregion


    public NetworkVariable<bool> isZooming = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isRightClickHeld = new NetworkVariable<bool>(false, writePerm: NetworkVariableWritePermission.Owner );
    //public NetworkVariable<bool> isRecovering = new NetworkVariable<bool>(false, writePerm: NetworkVariableWritePermission.Owner );
    //public NetworkVariable<bool> isClickBlocked = new NetworkVariable<bool>(false, writePerm: NetworkVariableWritePermission.Owner );
    //public NetworkVariable<float> recoveryDelayTimer = new NetworkVariable<float>(0f, writePerm: NetworkVariableWritePermission.Owner );


    public override void Start()
    {
        base.Start();
        // Spotlight 초기화 확인
        if (thirdPersonWeaponLight == null)
        {
            Debug.LogError("Weapon Light가 연결되지 않았습니다!");
        }
        if (firstPersonWeaponLight == null)
        {
            Debug.LogError("Weapon Light가 연결되지 않았습니다!");
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
/*        // PlaceableItemManager 오브젝트 찾기
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
            
            Debug.LogError("Gauge Slider가 연결되지 않았습니다!");
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
/*        if (Input.GetMouseButtonDown(1) && !isClickBlocked.Value) // 우클릭
        {
            isRightClickHeld.Value = true;
         //   isRecovering.Value = false;
            recoveryDelayTimer.Value = 0f;
        }

        if (Input.GetMouseButtonUp(1)) // 우클릭 해제
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

        if (Input.GetMouseButtonDown(1) && !isZooming.Value && !player.IsDead) // 우클릭 시작
        {
            isRightClickHeld.Value = true;
            isZooming.Value = true;
        }
        else if (Input.GetMouseButtonUp(1) && isZooming.Value && !player.IsDead) // 우클릭 해제
        {
            isRightClickHeld.Value = false;
            isZooming.Value = false;
        }
        HandleFlash();
    }

    private void HandleFlash()
	{
        if (currentZoomState == isZooming.Value) return; // 상태 변화가 없으면 실행하지 않음

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

        if (!isZooming.Value) // 중간에 해제되면 즉시 복귀
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
            if (!isZooming.Value && targetIntensity == zoomedIntensity) yield break; // 줌 도중 해제되면 즉시 종료

            time += Time.deltaTime;
            float t = time / duration;
            float easeT = 1f - Mathf.Pow(1f - t, 3f); // 느려지는 효과 적용
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



    #region 사라진 기능
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

        // 게이지 상태에 따른 우클릭 차단 여부 업데이트
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
            gaugeImage.fillAmount = 0f; // 게이지가 0 이하로는 떨어지지 않음
            isRightClickHeld.Value = false; // 우클릭 상태 해제
        }
    }

    private void RecoverGauge(float amount)
    {
        gaugeImage.fillAmount += amount;
        if (gaugeImage.fillAmount >= maxGauge)
        {
            gaugeImage.fillAmount = maxGauge; // 최대값으로 제한
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
            // 강화된 무기 데이터를 받아 기본 값을 업데이트
            defaultInnerAngle = innerAngle;
            defaultOuterAngle = outerAngle;
            defaultIntensity = intensity;

            // 우클릭이 아닐 경우 즉시 적용
            if (!isRightClickHeld.Value)
            {
                SetLightValues(defaultInnerAngle, defaultOuterAngle, defaultIntensity);
            }
        }*/

}
