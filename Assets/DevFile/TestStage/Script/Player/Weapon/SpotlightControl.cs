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
    public float gaugeThreshold = 0.2f; // 10% (0.1) ���� ���� ��

    public NetworkVariable<bool> isRightClickHeld = new NetworkVariable<bool>(false, writePerm: NetworkVariableWritePermission.Owner );
    public NetworkVariable<bool> isRecovering = new NetworkVariable<bool>(false, writePerm: NetworkVariableWritePermission.Owner );
    public NetworkVariable<bool> isClickBlocked = new NetworkVariable<bool>(false, writePerm: NetworkVariableWritePermission.Owner );
    public NetworkVariable<float> recoveryDelayTimer = new NetworkVariable<float>(0f, writePerm: NetworkVariableWritePermission.Owner );


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
        if (IsOwner)
        {
            HandleRightClickInput();
            HandleGauge();
        }
        UpdateSpotlightState();
    }

    private IEnumerator initUI()
	{
        // PlaceableItemManager ������Ʈ ã��
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
        }
    }
    private void HandleRightClickInput()
    {
        if (Input.GetMouseButtonDown(1) && !isClickBlocked.Value) // ��Ŭ��
        {
            isRightClickHeld.Value = true;
            isRecovering.Value = false;
            recoveryDelayTimer.Value = 0f;
        }

        if (Input.GetMouseButtonUp(1)) // ��Ŭ�� ����
        {
            isRightClickHeld.Value = false;
        }
    }


    private void HandleGauge()
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
    }

    private void UpdateSpotlightState()
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
        if (!isRightClickHeld.Value)
        {
            SetLightValues(defaultInnerAngle, defaultOuterAngle, defaultIntensity);
        }
    }

}
