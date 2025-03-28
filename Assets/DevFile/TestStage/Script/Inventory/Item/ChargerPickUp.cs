using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Unity.Netcode;

public class ChargerPickUp : PickupItem
{
    [Header("게이지 설정")]
    public float maxGauge = 100f; // 최대 게이지
    public float gaugeIncreaseRate = 30f; // 초당 증가량
    public float gaugeDecreaseRate = 30f; // 초당 감소량

    [Header("비행 설정")]
    public float maxFlightSpeed = 20f; // 최대 비행 속도 (초기 속도)
    public float speedDamping = 5f; // 속도 감소율
    public float flightDuration = 3f; // 비행 지속 시간

    [Header("사운드 설정")]
    public AudioSource audioSource; // 게이지 차오를 때 재생할 사운드
    public AudioClip gaugeSound; // 게이지 차오를 때 재생할 사운드
    public AudioClip flightStartSound; // 비행 시작 시 재생할 사운드
    public float minPitch = 1.0f; // 최소 피치값
    public float maxPitch = 2.0f; // 최대 피치값

    [HideInInspector] public Image gaugeBar; // UI 게이지 바
    [HideInInspector] public Transform cameraTransform; // 카메라의 Transform (Inspector에서 할당 가능)

    private float currentGauge = 0f;
    private NetworkVariable<bool> isFlying = new NetworkVariable<bool>(value: false , writePerm:NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> isUsingItem = new NetworkVariable<bool>(value: false , writePerm:NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> flightEnding = new NetworkVariable<bool>(value: true, writePerm:NetworkVariableWritePermission.Owner);

    private bool isGaugeSoundPlaying = false; // 게이지 사운드 중복 방지
    private float currentFlightSpeed;
    private Vector3 moveDirection;
    private CharacterController controller;


    protected override void Start()
    {
        base.Start();        
        gaugeBar = GameObject.Find("ItemGauge").GetComponent<Image>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        if (!isFlying.Value) // 비행 중이 아닐 때 게이지 조작
        {
            if (isUsingItem.Value)
            {
                currentGauge += gaugeIncreaseRate * Time.deltaTime;

                // 게이지 증가 사운드 재생 (중복 방지)
                if (!isGaugeSoundPlaying && gaugeSound != null && audioSource != null)
                {
                    audioSource.loop = true;
                    audioSource.clip = gaugeSound;
                    audioSource.Play();
                    isGaugeSoundPlaying = true;
                }

                // 피치값을 게이지 퍼센트에 맞게 조절
                if (audioSource != null)
                {
                    float gaugePercent = currentGauge / maxGauge;
                    audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, gaugePercent);
                }
            }
            else
            {
                currentGauge -= gaugeDecreaseRate * Time.deltaTime;

                // 클릭을 멈추면 게이지 사운드 중지
                isGaugeSoundPlaying = false;
                audioSource.Stop();
            }

            currentGauge = Mathf.Clamp(currentGauge, 0, maxGauge);

            if (gaugeBar != null)
            {
                gaugeBar.fillAmount = currentGauge / maxGauge;
            }

            if (currentGauge >= maxGauge)
            {
                StartFlight();
            }
        }

        isUsingItem.Value = false; // 매 프레임 UseItem 호출 여부 초기화
    }

    void FixedUpdate()
    {
        if (isFlying.Value)
        {
            controller.Move(moveDirection * currentFlightSpeed * Time.deltaTime);

            // 서서히 속도 감소
            currentFlightSpeed = Mathf.Lerp(currentFlightSpeed, 0, speedDamping * Time.deltaTime);
        }
    }


    void StartFlight()
    {
        if (isFlying.Value || !flightEnding.Value || cameraTransform == null) return; // 중복 호출 & 카메라 확인

        isFlying.Value = true;
        flightEnding.Value = false;
        currentFlightSpeed = maxFlightSpeed; // 처음에는 최대 속도

        // 카메라가 바라보는 방향을 기준으로 전진 방향 설정 (수평 방향만 고려)
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0; // 위/아래 각도 무시 (수평 방향만 반영)
        moveDirection = cameraForward.normalized; // 정규화하여 방향 벡터 설정

        // 플레이어의 방향을 카메라의 전진 방향과 동일하게 설정
        transform.rotation = Quaternion.LookRotation(moveDirection);

        // 비행 시작 효과음 재생
        if (flightStartSound != null && audioSource != null)
        {
            audioSource.Stop(); // 기존 게이지 사운드 중지
            audioSource.pitch = 1.0f; // 피치값 원래대로 복구
            audioSource.loop = false;
            audioSource.PlayOneShot(flightStartSound);
        }

        // 게이지 충전 사운드 중복 방지 변수 리셋
        isGaugeSoundPlaying = false;

        Invoke(nameof(EndFlight), flightDuration); // 일정 시간 후 착지 처리
    }

    void EndFlight()
    {
        if (flightEnding.Value) return; // 중복 호출 방지

        flightEnding.Value = true;
        isFlying.Value = false;
        currentGauge = 0; // 게이지 초기화
    }


    public override void UseItem(NetworkInventoryController controller)
	{
		if (this.controller == null)
		{
            this.controller = controller.gameObject.GetComponent<CharacterController>();
        }

        isUsingItem.Value = true;


        //base.UseItem(controller);

        Debug.Log("아이템 사용중. (차징)");
	}
}
