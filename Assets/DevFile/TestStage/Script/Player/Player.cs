using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(NetworkObject))]
public class Player : playerMoveController, IHealth, ICharacter
{
    private const int Renderable = 11;
    private const int DisRenderable = 12;
    private const float DefaultVignetteIntensity = 0f;

    [Header("References (attach these components)")]
    [SerializeField] public PlayerStats stats;
    [SerializeField] public PlayerNetworkData networkData;
    [SerializeField] public PlayerDamageHandler damageHandler;
    [SerializeField] public PlayerUIHandler uiHandler;
    [SerializeField] public PlayerLifeCycle lifeCycle;
    [SerializeField] public PlayerMicController micController;
    [SerializeField] public GameManagerAndInteractor gamemanager;

    // Restored original references used by other systems
    [SerializeField] private SaveSystem saveSystem;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Rigidbody rigidbody;
    [SerializeField] public GameObject firstPersonObject;
    [SerializeField] public GameObject thirdPersonObject;
    [SerializeField] private GameObject dieTargetGameObject;
    [SerializeField] public SpotlightControl spotlightControl;
    [SerializeField] private NetworkRagdollController netRagdollController;
    [SerializeField] public List<MonoBehaviour> dieEnableMonoBehaviorScripts;
    [SerializeField] public List<NetworkBehaviour> dieEnableNetworkBehaviorScripts;
    [SerializeField] private UIAnimationManager uiAniManager;
    [SerializeField] private AnimationRelay animationRelay;

    private Volume postProcessingVolume;
    private Vignette vignette;
    private Coroutine activeHitEffect;
    private Vector3 originalCameraPosition;
    private Image healthBar;

    // 상태 추적
    [HideInInspector] public bool isMyCharacter = false;

    private HashSet<string> destroySceneNames;

    // Events (restored)
    public event Action OnDataChanged;
    public event Action OnDieLocal;
    public event Action OnDieEffects;
    public event Action OnReviveLocal;
    public static event Action OnDie;

    public string Name
    {
        get => networkData.PlayerName.Value.ToString();
        set
        {
            if (networkData.PlayerName.Value.ToString() != value)
            {
                networkData.SetNameServerRpc(value);
                OnDataChanged?.Invoke();
            }
        }
    }

    public int Level
    {
        get => networkData.Level.Value;
        set
        {
            if (networkData.Level.Value != value)
            {
                networkData.Level.Value = value;
                OnDataChanged?.Invoke();
            }
        }
    }

    public int Experience
    {
        get => networkData.Experience.Value;
        set
        {
            if (networkData.Experience.Value != value)
            {
                networkData.Experience.Value = value;
                OnDataChanged?.Invoke();
            }
        }
    }

    public bool IsDead => networkData.IsDead;
    public float Health => networkData.Health.Value;

    public void NotifyDataChanged()
    {
        OnDataChanged?.Invoke();
    }

    public override void Start()
    {
        base.Start();

        // Safety: ensure components are set
        if (!stats) stats = GetComponent<PlayerStats>();
        if (!networkData) networkData = GetComponent<PlayerNetworkData>();
        if (!damageHandler) damageHandler = GetComponent<PlayerDamageHandler>();
        if (!uiHandler) uiHandler = GetComponent<PlayerUIHandler>();
        if (!lifeCycle) lifeCycle = GetComponent<PlayerLifeCycle>();

        StartCoroutine(WaitSpawnForInit());
    }

	private IEnumerator WaitSpawnForInit()
	{
		yield return new WaitForSeconds(0.25f);

		// Initialize subsystems
		networkData.Initialize(this);
		damageHandler.Initialize(this, stats, networkData, uiHandler);
		uiHandler.Initialize(this, stats);
		lifeCycle.Initialize(this, networkData, damageHandler, uiHandler);

        if (IsOwner)
        {
            isMyCharacter = true;
            Name = FindAnyObjectByType<PlayerIDManager>().PlayerName;
            micController = FindAnyObjectByType<PlayerMicController>();
            lifeCycle.BindLocalEvents(micController);

            SetLayers(firstPersonObject, Renderable);
            SetLayers(thirdPersonObject, DisRenderable);
        }
        else
        {
            SetLayers(firstPersonObject, DisRenderable);
            SetLayers(thirdPersonObject, Renderable);
        }

        rigidbody = rigidbody ?? GetComponent<Rigidbody>();
        if (rigidbody != null) rigidbody.isKinematic = true;
    }


    private void OnDisable()
    {
        lifeCycle.UnbindEvents();
    }

    public override void FixedUpdate()
    {
        if (!networkData.IsDead) base.FixedUpdate();

        if (!IsOwner) return;

        damageHandler.HandleFallDamage();

        if (isEventPlaying.Value || pause) return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            firstpersonAnimator.SetTrigger("AttackTrigger");
            thirdpersonAnimator.SetTrigger("AttackTrigger");
        }
    }

    // Expose method to take damage from other systems
    public void TakeDamage(float amount, AudioClip hitSound = null)
    {
        if (networkData.IsDead) return;

        if (gamemanager == null) gamemanager = FindAnyObjectByType<GameManagerAndInteractor>();
        if (gamemanager != null && !gamemanager.doorState.Value) return;

        Debug.Log($"Damaged : {amount} , Name : {Name} ");

        damageHandler.RequestDamage(amount, hitSound);

        uiHandler?.OnDamageTaken();

        if (hitSound != null) audioSource?.PlayOneShot(hitSound);
    }

    #region Die
    public void Die() => StartCoroutine(DieSequence());

    private IEnumerator DieSequence()
    {
        OnDieEffects?.Invoke();
        OnDie?.Invoke();

        if (IsOwner)
        {
            uiAniManager?.DieAnimation();
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
        if (dieEnableMonoBehaviorScripts != null)
        {
            foreach (MonoBehaviour monoScirpt in dieEnableMonoBehaviorScripts)
            {
                if (monoScirpt != null) monoScirpt.enabled = Value;
            }
        }

        if (dieEnableNetworkBehaviorScripts != null)
        {
            foreach (NetworkBehaviour monoScirpt in dieEnableNetworkBehaviorScripts)
            {
                if (monoScirpt != null) monoScirpt.enabled = Value;
            }
        }

        if (characterController != null) characterController.enabled = Value;
        if (bodyCollider != null) bodyCollider.enabled = Value;
    }

    public void DieEffect()
    {
        SetAimMode(true, dieTargetGameObject);
        SetLayers(thirdPersonObject, Renderable);
        SetLayers(firstPersonObject, DisRenderable);

        if (IsOwner)
        {
            spotlightControl?.ToogleLight();
            netRagdollController?.DieServerRpc(transform.position);
        }
    }

    /// <summary>
    /// false = 3인칭 몸 true = 1인칭 몸
    /// </summary>
    public void SetPlayerDieView(bool value)
    {
        if (firstPersonObject != null) firstPersonObject.gameObject.SetActive(value);
        if (thirdPersonObject != null) thirdPersonObject.gameObject.SetActive(!value);

        if (camTarget != null) camTarget.gameObject.SetActive(value);
        if (spotlightControl != null)
        {
            spotlightControl.firstPersonWeaponLight.gameObject.SetActive(value);
            spotlightControl.thirdPersonWeaponLight.gameObject.SetActive(!value);
        }

        if (value)
        {
            SetLayers(thirdPersonObject, DisRenderable);
            SetLayers(firstPersonObject, Renderable);
        }
        else
        {
            SetLayers(firstPersonObject, DisRenderable);
            SetLayers(thirdPersonObject, Renderable);
        }
    }
    #endregion

    // For revive sequence called by other managers
    public IEnumerator ReviveSequence()
    {
        if (IsOwner)
        {
            // 죽음 관련 스크립트 다시 활성화
            SetDieScirpt(true);

            // 카메라 및 무기 라이트 초기화
            SetPlayerDieView(false); // 다시 3인칭으로
        }

        // ragdoll 상태 해제
        netRagdollController?.ReviveServerRpc();

        // 렌더링 레이어 복구
        SetLayers(thirdPersonObject, Renderable);
        SetLayers(firstPersonObject, DisRenderable);

        yield return null;

        if (IsOwner)
        {
            networkData.SetHealthServerRpc(stats.maxHealth);
            SetAimMode(); // 조준 가능 상태로 전환
            OnReviveLocal?.Invoke();
        }
    }

    private static void SetLayers(GameObject target, int layer)
    {
        if (target == null) return;
        target.layer = layer;
        foreach (Transform child in target.transform) SetLayers(child.gameObject, layer);
    }
	// end Player class
}

[Serializable]
public struct PlayerData
{
    public string playerName;
    public int experience;
    public int level;
    public WeaponInstance weaponInstance;
}