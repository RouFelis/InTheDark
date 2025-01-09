using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;

public class SpotlightControl : NetworkBehaviour
{
    [Header("Spotlight Settings")]
    public Light weaponLight; // ���� ������ �ϴ� ����
    public float zoomedInnerAngle = 20f; // ��Ŭ�� �� Inner Angle
    public float zoomedOuterAngle = 30f; // ��Ŭ�� �� Outer Angle
    public float zoomedIntensity = 2000f;   // ��Ŭ�� �� Intensity

    public float defaultInnerAngle = 32f;
    public float defaultOuterAngle = 70f;
    public float defaultIntensity = 600f;

    [Header("Gauge Settings")]
    public Image gaugeImage; // ������ UI (Slider ������Ʈ)
    public float decreaseRate = 0.1f; // �ʴ� ������
    public float increaseRate = 0.1f; // �ʴ� ������
    public float delayBeforeRecovery = 0.5f; // ȸ�� ��� �ð� (��)
    public float minGauge = 0.001f; // �ּ� ������ ��
    public float maxGauge = 1f; // �ִ� ������ ��
    public float gaugeThreshold = 0.1f; // 10% (0.1) ���� ���� ��

    private bool isRightClickHeld = false;
    private bool isRecovering = false;
    private bool isClickBlocked = false; // ��Ŭ�� ���� ����
    private float recoveryDelayTimer = 0f;

    void Start()
    {
        // Spotlight �ʱ�ȭ Ȯ��
        if (weaponLight == null)
        {
            Debug.LogError("Weapon Light�� ������� �ʾҽ��ϴ�!");
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
        Debug.Log("ũ�ƾƾƾ�");
        // PlaceableItemManager ������Ʈ ã��
        while (gaugeImage == null)
        {
            Debug.Log("ũ�ƾƾƾ�2");
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
        }
        Debug.Log("ũ�ƾƾƾ�3");
        Debug.Log("Gauge Slider�� ���� �Ϸ�!");
    }

    private void HandleRightClickInput()
    {
        if (Input.GetMouseButtonDown(1) && !isClickBlocked) // ��Ŭ��
        {
            isRightClickHeld = true;
            isRecovering = false; // ���� ���¸� �ߴ�
            recoveryDelayTimer = 0f; // ���� ���ð� �ʱ�ȭ
            SetLightValues(zoomedInnerAngle, zoomedOuterAngle, zoomedIntensity);
        }

        if (Input.GetMouseButtonUp(1)) // ��Ŭ�� ����
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

        // ������ ���¿� ���� ��Ŭ�� ���� ���� ������Ʈ
        if (gaugeImage.fillAmount < gaugeThreshold)
        {
            isClickBlocked = true; // ������ 10% �̸����� ��Ŭ�� ����
        }
        else if (gaugeImage.fillAmount >= gaugeThreshold && isClickBlocked)
        {
            isClickBlocked = false; // ������ 10% �̻��� �Ǹ� ��Ŭ�� ����
        }
    }

    private void DecreaseGauge(float amount)
    {
        gaugeImage.fillAmount -= amount;
        if (gaugeImage.fillAmount <= minGauge)
        {
            gaugeImage.fillAmount = 0f; // �������� 0 ���Ϸδ� �������� ����
            ResetRightClick(); // ��Ŭ�� ���� ����
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
            gaugeImage.fillAmount = maxGauge; // �ִ밪���� ����
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
        // ��ȭ�� ���� �����͸� �޾� �⺻ ���� ������Ʈ
        defaultInnerAngle = innerAngle;
        defaultOuterAngle = outerAngle;
        defaultIntensity = intensity;

        // ��Ŭ���� �ƴ� ��� ��� ����
        if (!isRightClickHeld)
        {
            SetLightValues(defaultInnerAngle, defaultOuterAngle, defaultIntensity);
        }
    }

}
