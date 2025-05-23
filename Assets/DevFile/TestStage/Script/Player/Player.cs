using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.VFX;
using SaintsField.Playa;

[RequireComponent(typeof(NetworkObject))]
public class Player : playerMoveController, IHealth, ICharacter
{
    private const int Renderable = 11;
    private const int DisRenderable = 12;
    private const float DefaultVignetteIntensity = 0f;

    [Header("Network Settings")]
    [LayoutStart("Network Settings", ELayout.FoldoutBox)]
    [SerializeField] private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>();
    [SerializeField] private NetworkVariable<int> experience = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<int> level = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<float> currentHealth = new NetworkVariable<float>(100f, writePerm:NetworkVariableWritePermission.Server);

    [Header("Player Settings")]
    [LayoutStart("Player Settings", ELayout.FoldoutBox)]
    public float maxHealth = 100f;
     public float cameraShakeMagnitude = 0.00015f;
     public float cameraShakeDuration = 0.3f;

    [Header("References")]
    [LayoutStart("References", ELayout.FoldoutBox)]
    [SerializeField] private SaveSystem saveSystem;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Rigidbody rigidbody;
    [SerializeField] private GameObject firstPersonObject;
    [SerializeField] private GameObject thirdPersonObject;
    [SerializeField] private GameObject dieTargetGameObject;
    [SerializeField] private SpotlightControl spotlightControl;
    [SerializeField] private NetworkRagdollController netRagdollController;
    [SerializeField] private List<MonoBehaviour> dieEnableMonoBehaviorScripts;
    [SerializeField] private List<NetworkBehaviour> dieEnableNetworkBehaviorScripts;
    [SerializeField] private UIAnimationManager uiAniManager;
    [SerializeField] private AnimationRelay animationRelay;


    [LayoutStart("FallDamage", ELayout.FoldoutBox)]
    public float fallThreshold = 5f;       // 데미지 발생 최소 낙하 거리
    public float damageMultiplier = 10f;   // 거리당 데미지 배율
    public float groundCheckDistance = 0.2f;

    public float collisionSpeedThreshold = 8f; //이 속도 이상으로 오브젝트에 충돌하면 데미지 적용 (m/s)
    public float collisionDamageMultiplier = 5f; // 충돌 속도 초과분당 데미지 계수

    // 상태 추적
    private bool wasGrounded = true;
    private bool isFalling = false;
    private float peakY;



    private HashSet<string> destroySceneNames;

    private Volume postProcessingVolume;
    private Vignette vignette;
    private Coroutine activeHitEffect;
    private Vector3 originalCameraPosition;
    private Image healthBar;


    public event Action OnDataChanged;
    public event Action OnDieLocal;
    public event Action OnDieEffects;
    public event Action OnReviveLocal;
	public static event Action OnDie;


    public string Name
    {
        get => playerName.Value.ToString();
        set
        {
            if (playerName.Value != value)
            {
                playerName.Value = value;
                OnDataChanged?.Invoke();
            }
        }
    }

    public int Level
    {
        get => level.Value;
        set
        {
            if (level.Value != value)
            {
                level.Value = value;
                OnDataChanged?.Invoke();
            }
        }
    }

    public int Experience
    {
        get => experience.Value;
        set
        {
            if (experience.Value != value)
            {
                experience.Value = value;
                OnDataChanged?.Invoke();
            }
        }
    }

    public bool IsDead => currentHealth.Value <= 0;
    public string PlayerName => playerName.Value.ToString();
    public float Health => currentHealth.Value;

    public override void Start()
    {
        base.Start();
        InitializePlayerLayers();
        currentHealth.OnValueChanged += HandleHealthChanged;


        OnReviveLocal += () => { SetDieScirpt(true); Revive(); };
        OnDieEffects += DieEffect;

        rigidbody.isKinematic = true;
    }

	public override void FixedUpdate()
	{
		if (!IsDead) base.FixedUpdate();

		if (!IsOwner) return;

		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
            firstpersonAnimator.SetTrigger("AttackTrigger");
            thirdpersonAnimator.SetTrigger("AttackTrigger");
            Debug.Log("테스트");
            //animationRelay.OnAttackHit();
        }

        FallDamage();
    }


	#region 부딛힐 떄 데미지 추가해봄.
	private void FallDamage()
    {
        // 1) 그라운드 판정 (CharacterController.isGrounded 우선)
        bool grounded = IsGrounded();

        // 2) “지면 → 공중” 순간: 최고 지점 저장
        if (wasGrounded && !grounded)
        {
            peakY = transform.position.y;
        }

        // 3) 공중에서 아래로 향하는 순간에만 하강 플래그 켜기
        if (!grounded && !isFalling && characterController.velocity.y < -0.1f)
        {
            isFalling = true;
        }

        // 4) “공중 → 지면” 순간: 실제 하강했을 때만 데미지 계산
        if (grounded && !wasGrounded && isFalling)
        {
            isFalling = false;

            float fallDistance = peakY - transform.position.y;
            float effective = Mathf.Max(0f, fallDistance - fallThreshold);

            if (effective > 0f)
            {
                float dmg = effective * damageMultiplier;
                TakeDamage(dmg, null);
            }
        }

        wasGrounded = grounded;
    }


    // CharacterController 충돌 이벤트 핸들러
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // 지면(ground)과 부딪힌 것은 제외하려면, 노멀 벡터로 필터링
        // hit.normal.y > 0.5f 이면 거의 바닥(또는 완만한 경사)이므로 무시
        if (hit.normal.y > 0.5f) return;

        // 현재 속도 크기 (magnitude) 확인
        float speed = characterController.velocity.magnitude;

        if (speed >= collisionSpeedThreshold)
        {
            float excess = speed - collisionSpeedThreshold;
            float dmg = excess * collisionDamageMultiplier;
            TakeDamage(dmg, null);
        }
    }

	#endregion


	/// <summary>
	/// 플레이어 레이어 초기 설정
	/// </summary>
	private void InitializePlayerLayers()
    {
        if (IsOwner)
        {
            SetLayers(firstPersonObject, Renderable);
            SetLayers(thirdPersonObject, DisRenderable);
            StartCoroutine(InitializeSaveSystem());
            StartCoroutine(InitializeUI());
            StartCoroutine(InitializeAniSystem());
        }
        else
        {
            SetLayers(firstPersonObject, DisRenderable);
            SetLayers(thirdPersonObject, Renderable);
        }
    }


    private IEnumerator InitializeUI()
    {
        while (healthBar == null)
        {
            try
            {
                healthBar = GameObject.Find("HealthBar").GetComponent<Image>();
            }
            catch
            {
                Debug.Log("HealthBar Serching");
            }
            yield return null;
        }
    }

    private IEnumerator InitializeAniSystem()
    {
        while (uiAniManager == null)
        {
            try
            {
                uiAniManager = FindAnyObjectByType<UIAnimationManager>();
            }
            catch
            {
                Debug.Log("UIAnimationManager Serching...");
            }
            yield return null;
        }
    }


    /// <summary>
    /// 저장 시스템 초기화 코루틴
    /// </summary>
    private IEnumerator InitializeSaveSystem()
    {
        while (saveSystem == null)
        {
            try
            {
                saveSystem = FindAnyObjectByType<SaveSystem>();
            }
            catch
            {
                Debug.Log("SaveSystem Serching");
            }
            yield return null;
        }

        postProcessingVolume = GameObject.Find("Sky and Fog Global Volume")?.GetComponent<Volume>();
        if (postProcessingVolume?.profile.TryGet(out vignette) == true)
        {
            vignette.intensity.value = DefaultVignetteIntensity;
        }

        originalCameraPosition = FirstPersonCamera.transform.localPosition;
        KeySettingsManager.Instance.localPlayer = this;

        playerName.OnValueChanged += (newValue, oldValue) => saveSystem.SavePlayerData(this);
        experience.OnValueChanged += (newValue, oldValue) => saveSystem.SavePlayerData(this);
        level.OnValueChanged += (newValue, oldValue) => saveSystem.SavePlayerData(this);
    }


    /// <summary>
    /// 체력 변경 이벤트 핸들러
    /// </summary>
    private void HandleHealthChanged(float previous, float current)
    {
        Debug.Log($"Health changed: {previous} -> {current}");
        if (current <= 0) Die();
    }


    /// <summary>
    /// 데미지 처리 메서드
    /// </summary>
    public void TakeDamage(float amount, AudioClip hitSound)
    {
        if (IsDead) return;

        currentHealth.Value -= amount;
        UpdateHealthDisplay();

        if (hitSound != null) audioSource.PlayOneShot(hitSound);
    }


    /// <summary>
    /// 체력 관련 시각 효과 업데이트
    /// </summary>
    private void UpdateHealthDisplay()
    {
        StartCoroutine(CameraShake());
        StartHitEffect();
        UpdateHealthBar();
    }

    /// <summary>
    /// 피격 효과 시작
    /// </summary>
    private void StartHitEffect()
    {
        if (activeHitEffect != null) StopCoroutine(activeHitEffect);
        activeHitEffect = StartCoroutine(HitEffectCoroutine());
    }

    /// <summary>
    /// 카메라 흔들림 효과 코루틴
    /// </summary>
    private IEnumerator CameraShake()
    {
        var elapsed = 0f;
        while (elapsed < cameraShakeDuration)
        {
            elapsed += Time.deltaTime;
            var offset = UnityEngine.Random.insideUnitSphere * cameraShakeMagnitude;
            FirstPersonCamera.transform.localPosition = originalCameraPosition + offset;
            yield return null;
        }
        FirstPersonCamera.transform.localPosition = originalCameraPosition;
    }


    /// <summary>
    /// 화면 비네팅 효과 코루틴
    /// </summary>
    private IEnumerator HitEffectCoroutine()
    {
        const float peakIntensity = 0.6f;
        const float attackDuration = 0.1f;
        const float decayDuration = 1f;
        const float holdTime = 2f;

        yield return AdjustVignette(DefaultVignetteIntensity, peakIntensity, attackDuration);
        yield return new WaitForSeconds(holdTime);
        yield return AdjustVignette(peakIntensity, DefaultVignetteIntensity, decayDuration);
    }


    /// <summary>
    /// 비네팅 효과 점진적 변경 코루틴
    /// </summary>
    private IEnumerator AdjustVignette(float start, float end, float duration)
    {
        var elapsed = 0f;
        while (elapsed < duration && vignette != null)
        {
            elapsed += Time.deltaTime;
            vignette.intensity.value = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
    }


    /// <summary>
    /// 체력바 업데이트
    /// </summary>
    private void UpdateHealthBar()
    {
        if (healthBar == null || maxHealth <= 0) return;
        healthBar.fillAmount = Mathf.Clamp01(currentHealth.Value / maxHealth) * 0.5f;
    }

	#region Die
	public void Die() => StartCoroutine(DieSequence());


    /// <summary>
    /// 사망 시퀀스 코루틴
    /// </summary>
    private IEnumerator DieSequence()
    {
        OnDieEffects?.Invoke();
        OnDie?.Invoke();

        if (IsOwner)
        {
			if (PlayersManager.Instance.allPlayersDead)
			{        
                uiAniManager.DieAnimation();
            }
			else
			{
                uiAniManager.AllDieAnimation();
            }
            SetDieScirpt(false);
        }

        yield return new WaitForSeconds(6f);
        if (IsOwner)
        {
            SetAimMode();
            OnDieLocal?.Invoke();            
        }
    }

    private void SetDieScirpt(bool Value)
	{
		foreach (MonoBehaviour monoScirpt in dieEnableMonoBehaviorScripts)
		{
            monoScirpt.enabled = Value;
        }

        foreach (NetworkBehaviour monoScirpt in dieEnableNetworkBehaviorScripts)
        {
            monoScirpt.enabled = Value;
        }


        characterController.enabled = Value;
        bodyCollider.enabled = Value;
    }


    /// <summary>
    /// 사망 시 특수 효과 적용
    /// </summary>
    private void DieEffect()
    {
        SetAimMode(true, dieTargetGameObject);
        SetLayers(thirdPersonObject, Renderable);
        SetLayers(firstPersonObject, DisRenderable);

		if (IsOwner)
        {
            spotlightControl.ToogleLight();
        }

        netRagdollController.DieServerRpc(transform.position);
    }


    public void SetPlayerDieView(bool value)
    {/*
		firstPersonObject.gameObject.SetActive(!firstPersonObject.activeInHierarchy);
		thirdPersonObject.gameObject.SetActive(!thirdPersonObject.activeInHierarchy);
*/

        camTarget.gameObject.SetActive(value);
        spotlightControl.firstPersonWeaponLight.gameObject.SetActive(value);
        spotlightControl.thirdPersonWeaponLight.gameObject.SetActive(!value);



        if (value)
        {
            SetLayers(thirdPersonObject, DisRenderable);
            SetLayers(firstPersonObject, Renderable);
        }
        else
        {
            SetLayers(firstPersonObject, DisRenderable );
            SetLayers(thirdPersonObject, Renderable);
        }
    }

    private void Revive()
    {
        healthBar.gameObject.SetActive(true);
    }
#endregion

	/// <summary>
	/// 플레이어 데이터 로드 서버 RPC
	/// </summary>
	[ServerRpc(RequireOwnership = false)]
    public void LoadPlayerDataServerRPC()
    {
        var path = $"{Application.persistentDataPath}/playerdata.json";
        if (!File.Exists(path)) return;

        var json = saveSystem.useEncryption
            ? saveSystem.EncryptDecrypt(File.ReadAllText(path))
            : File.ReadAllText(path);

        var data = JsonUtility.FromJson<PlayerData>(json);
        playerName.Value = data.playerName;
        experience.Value = data.experience;
        level.Value = data.level;
    }

    private static void SetLayers(GameObject target, int layer)
    {
        target.layer = layer;
        foreach (Transform child in target.transform)
        {
            SetLayers(child.gameObject, layer);
        }
    }

}

[Serializable]
public struct PlayerData
{
    public string playerName;
    public int experience;
    public int level;
    public WeaponInstance weaponInstance;
}