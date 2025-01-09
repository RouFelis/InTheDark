using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;

public class SpotlightControl : NetworkBehaviour
{
    [Header("Spotlight Settings")]
    public Light weaponLight; // 무기 역할을 하는 조명
    public float zoomedInnerAngle = 20f; // 우클릭 시 Inner Angle
    public float zoomedOuterAngle = 30f; // 우클릭 시 Outer Angle
    public float zoomedIntensity = 2000f;   // 우클릭 시 Intensity

    public float defaultInnerAngle = 32f;
    public float defaultOuterAngle = 70f;
    public float defaultIntensity = 600f;

    [Header("Gauge Settings")]
    public Image gaugeImage; // 게이지 UI (Slider 컴포넌트)
    public float decreaseRate = 0.1f; // 초당 감소율
    public float increaseRate = 0.1f; // 초당 증가율
    public float delayBeforeRecovery = 0.5f; // 회복 대기 시간 (초)
    public float minGauge = 0.001f; // 최소 게이지 값
    public float maxGauge = 1f; // 최대 게이지 값
    public float gaugeThreshold = 0.1f; // 10% (0.1) 이하 제한 값

    private bool isRightClickHeld = false;
    private bool isRecovering = false;
    private bool isClickBlocked = false; // 우클릭 차단 여부
    private float recoveryDelayTimer = 0f;

    void Start()
    {
        // Spotlight 초기화 확인
        if (weaponLight == null)
        {
            Debug.LogError("Weapon Light가 연결되지 않았습니다!");
        }
        if(IsOwner)
          StartCoroutine(initUI());
    }

    void Update()
    {
        HandleRightClickInput();
        HandleGauge();
    }

    private IEnumerator initUI()
	{
        Debug.Log("크아아아악");
        // PlaceableItemManager 오브젝트 찾기
        while (gaugeImage == null)
        {
            Debug.Log("크아아아악2");
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
        }
        Debug.Log("크아아아악3");
        Debug.Log("Gauge Slider가 연결 완료!");
    }

    private void HandleRightClickInput()
    {
        if (Input.GetMouseButtonDown(1) && !isClickBlocked) // 우클릭
        {
            isRightClickHeld = true;
            isRecovering = false; // 충전 상태를 중단
            recoveryDelayTimer = 0f; // 충전 대기시간 초기화
            SetLightValues(zoomedInnerAngle, zoomedOuterAngle, zoomedIntensity);
        }

        if (Input.GetMouseButtonUp(1)) // 우클릭 해제
        {
            ResetRightClick();
        }
    }

    private void HandleGauge()
    {
        if (isRightClickHeld)
        {
            DecreaseGauge(Time.deltaTime * decreaseRate);
        }
        else if (!isRecovering && gaugeImage.fillAmount < maxGauge)
        {
            recoveryDelayTimer += Time.deltaTime;
            if (recoveryDelayTimer >= delayBeforeRecovery)
            {
                StartRecovery();
            }
        }

        if (isRecovering)
        {
            RecoverGauge(Time.deltaTime * increaseRate);
        }

        // 게이지 상태에 따른 우클릭 차단 여부 업데이트
        if (gaugeImage.fillAmount < gaugeThreshold)
        {
            isClickBlocked = true; // 게이지 10% 미만에서 우클릭 차단
        }
        else if (gaugeImage.fillAmount >= gaugeThreshold && isClickBlocked)
        {
            isClickBlocked = false; // 게이지 10% 이상이 되면 우클릭 가능
        }
    }

    private void DecreaseGauge(float amount)
    {
        gaugeImage.fillAmount -= amount;
        if (gaugeImage.fillAmount <= minGauge)
        {
            gaugeImage.fillAmount = 0f; // 게이지가 0 이하로는 떨어지지 않음
            ResetRightClick(); // 우클릭 상태 해제
        }
    }

    private void StartRecovery()
    {
        isRecovering = true;
    }

    private void RecoverGauge(float amount)
    {
        gaugeImage.fillAmount += amount;
        if (gaugeImage.fillAmount >= maxGauge)
        {
            gaugeImage.fillAmount = maxGauge; // 최대값으로 제한
            isRecovering = false;
        }
    }

    private void ResetRightClick()
    {
        isRightClickHeld = false;
        SetLightValues(defaultInnerAngle, defaultOuterAngle, defaultIntensity);
    }

    private void SetLightValues(float innerAngle, float outerAngle, float intensity)
    {
        weaponLight.innerSpotAngle = innerAngle;
        weaponLight.spotAngle = outerAngle;
        weaponLight.intensity = intensity;
    }

    public void UpdateDefaultValues(float innerAngle, float outerAngle, float intensity)
    {
        // 강화된 무기 데이터를 받아 기본 값을 업데이트
        defaultInnerAngle = innerAngle;
        defaultOuterAngle = outerAngle;
        defaultIntensity = intensity;

        // 우클릭이 아닐 경우 즉시 적용
        if (!isRightClickHeld)
        {
            SetLightValues(defaultInnerAngle, defaultOuterAngle, defaultIntensity);
        }
    }

}
