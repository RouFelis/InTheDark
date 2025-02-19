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


    public delegate void FlashEventHandler();
    public static event FlashEventHandler OnFlash; // 플래시 이벤트

    [Header("Zoom Value Settings")]
    public float maxZoomDuration = 5.0f;
    public float zoomSpeedMultiplier = 1.0f;
    public float resetSpeedMultiplier = 1.0f;


    public float flashIntensity = 5000f;
    public float flashOuterAngle = 90f;

    public float flashExpandDuration = 0.01f;
    public float flashFadeDuration = 0.5f;



    private bool isResetting = false;
    private float zoomProgress = 0f;
    private float lastEaseT = -1f;
    private bool hasFlashed = false;
    private bool isFlashing = false;

    public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve resetCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);


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
        if (!IsOwner || isResetting || isFlashing) return;

        if (Input.GetMouseButton(1) && !isResetting)
        {
            isZooming.Value = true;
            zoomProgress = Mathf.Min(zoomProgress + (Time.deltaTime * zoomSpeedMultiplier), maxZoomDuration);

            // 줌이 최대값에 도달하면 플래시 실행
            if (zoomProgress >= maxZoomDuration && !hasFlashed)
            {
                hasFlashed = true;
                TriggerFlashEffect();
            }
        }
        else
        {
            isZooming.Value = false;
            zoomProgress = Mathf.Max(zoomProgress - (Time.deltaTime * resetSpeedMultiplier), 0f);

            // 줌이 완전히 해제되었을 때 플래시 상태 초기화
            if (zoomProgress <= 0.01f && !isFlashing)
            {
                hasFlashed = false;
            }
        }

        float t = zoomProgress / maxZoomDuration;
        t = Mathf.Clamp01(t);
        float easeT = isZooming.Value ? zoomCurve.Evaluate(t) : resetCurve.Evaluate(t);

        if (Mathf.Approximately(easeT, lastEaseT)) return;

        lastEaseT = easeT;
        ApplyZoom(easeT);
    }

    // 줌을 적용하는 함수
    private void ApplyZoom(float t)
    {
        float targetInner = Mathf.Lerp(defaultInnerAngle, zoomedInnerAngle, t);
        float targetOuter = Mathf.Lerp(defaultOuterAngle, zoomedOuterAngle, t);
        float targetIntensity = Mathf.Lerp(defaultIntensity, zoomedIntensity, t);

        firstPersonWeaponLight.innerSpotAngle = targetInner;
        firstPersonWeaponLight.spotAngle = targetOuter;
        firstPersonWeaponLight.intensity = targetIntensity;

        thirdPersonWeaponLight.innerSpotAngle = targetInner;
        thirdPersonWeaponLight.spotAngle = targetOuter;
        thirdPersonWeaponLight.intensity = targetIntensity;
    }

    // 플래시 효과 실행
    private void TriggerFlashEffect()
    {
        OnFlash?.Invoke();
        isFlashing = true;
        isZooming.Value = false;
        StartCoroutine(FlashEffect(firstPersonWeaponLight));
        StartCoroutine(FlashEffect(thirdPersonWeaponLight));
        StartCoroutine(DisableZoomDuringFlash());
    }

    // 플래시 효과 코루틴 (밝기 증가 후 점진적 감소)
    private IEnumerator FlashEffect(Light light)
    {
        if (light == null) yield break;

        float time = 0f;
        while (time < flashExpandDuration)
        {
            time += Time.deltaTime;
            float t = time / flashExpandDuration;
            light.intensity = Mathf.Lerp(defaultIntensity, flashIntensity, t);
            light.spotAngle = Mathf.Lerp(defaultOuterAngle, flashOuterAngle, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        time = 0f;
        while (time < flashFadeDuration)
        {
            time += Time.deltaTime;
            float t = time / flashFadeDuration;
            light.intensity = Mathf.Lerp(flashIntensity, defaultIntensity, t);
            light.spotAngle = Mathf.Lerp(flashOuterAngle, defaultOuterAngle, t);
            yield return null;
        }
    }

    // 플래시 도중 줌 사용을 방지하는 코루틴
    private IEnumerator DisableZoomDuringFlash()
    {
        isResetting = true;
        isFlashing = true;
        yield return new WaitForSeconds(flashExpandDuration + flashFadeDuration + 0.1f);
        zoomProgress = 0; // 줌 상태 초기화
        hasFlashed = false; // 플래시 상태 초기화
        isFlashing = false;
        isResetting = false;
    }



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
