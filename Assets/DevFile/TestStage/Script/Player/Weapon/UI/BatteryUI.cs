using UnityEngine;
using UnityEngine.UI;

public class BatteryUI : MonoBehaviour
{
    public Slider gaugeSlider; // 게이지 UI (Slider 컴포넌트)
    public float decreaseRate = 0.1f; // 초당 감소율
    public float increaseRate = 0.1f; // 초당 증가율
    public float delayBeforeRecovery = 0.5f; // 회복 대기 시간 (초)
    public float minGauge = 0.001f; // 최소 게이지 값
    public float maxGauge = 1f; // 최대 게이지 값

    private bool isRightClicking = false;
    private bool isRecovering = false;
    private float recoveryDelayTimer = 0f;

    void Update()
    {
        HandleInput();

        if (isRightClicking)
        {
            DecreaseGauge(Time.deltaTime * decreaseRate);
        }
        else if (!isRecovering && gaugeSlider.value < maxGauge)
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
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(1)) // 우클릭 시작
        {
            isRightClicking = true;
            isRecovering = false;
            recoveryDelayTimer = 0f;
        }

        if (Input.GetMouseButtonUp(1)) // 우클릭 해제
        {
            isRightClicking = false;
        }
    }

    private void DecreaseGauge(float amount)
    {
        gaugeSlider.value -= amount;
        if (gaugeSlider.value <= minGauge)
        {
            gaugeSlider.value = 0f; // 게이지가 0 이하로는 떨어지지 않음
        }
    }

    private void StartRecovery()
    {
        isRecovering = true;
        recoveryDelayTimer = 0f;
    }

    private void RecoverGauge(float amount)
    {
        gaugeSlider.value += amount;
        if (gaugeSlider.value >= maxGauge)
        {
            gaugeSlider.value = maxGauge; // 최대값으로 제한
            isRecovering = false;
        }
    }
}
