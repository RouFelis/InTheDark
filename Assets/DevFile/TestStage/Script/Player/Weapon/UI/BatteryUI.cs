using UnityEngine;
using UnityEngine.UI;

public class BatteryUI : MonoBehaviour
{
    public Slider gaugeSlider; // ������ UI (Slider ������Ʈ)
    public float decreaseRate = 0.1f; // �ʴ� ������
    public float increaseRate = 0.1f; // �ʴ� ������
    public float delayBeforeRecovery = 0.5f; // ȸ�� ��� �ð� (��)
    public float minGauge = 0.001f; // �ּ� ������ ��
    public float maxGauge = 1f; // �ִ� ������ ��

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
        if (Input.GetMouseButtonDown(1)) // ��Ŭ�� ����
        {
            isRightClicking = true;
            isRecovering = false;
            recoveryDelayTimer = 0f;
        }

        if (Input.GetMouseButtonUp(1)) // ��Ŭ�� ����
        {
            isRightClicking = false;
        }
    }

    private void DecreaseGauge(float amount)
    {
        gaugeSlider.value -= amount;
        if (gaugeSlider.value <= minGauge)
        {
            gaugeSlider.value = 0f; // �������� 0 ���Ϸδ� �������� ����
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
            gaugeSlider.value = maxGauge; // �ִ밪���� ����
            isRecovering = false;
        }
    }
}
