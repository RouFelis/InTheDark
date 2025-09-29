using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using InTheDark.Prototypes;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Collections;
using Unity.Netcode;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

// 일단 만들어 두기만....
//public ref struct EnemyTarget
//{
//	public bool IsEnable;
//	public Player Player;

//	public static implicit operator bool(EnemyTarget target)
//	{
//		var isEnalbe = target.IsEnable;

//		return isEnalbe;
//	}

//	public static implicit operator Player(EnemyTarget target)
//	{
//		var player = target.Player;

//		return player;
//	}

//	public EnemyTarget(bool isEnable, Player player)
//	{
//		IsEnable = isEnable;
//		Player = player;
//	}

//	public EnemyTarget(NetworkBehaviourReference reference)
//	{
//		IsEnable = reference.TryGet(out Player player);
//		Player = player;
//	}

//	public void Deconstruct(out bool isEnable, out Player player)
//	{
//		isEnable = IsEnable;
//		player = Player;
//	}
//}

public class EnemyPrototypePawn : NetworkPawn, IHealth
{
	public const string DEFAULT_STATE = "Normal";

	public const string WALKING_STATE = "IsWalking";
	public const string ATTACK_TRIGGER = "OnAttack";

	public delegate void EnemyDieDelegate(NetworkBehaviourReference reference);
	public delegate void PlayerKilledEnemyDelegate();

	public float InitializeCooldownValue;
	public int InitializeHealthValue;
	//[Obsolete] public float InitializeResistanceValue;
	public float InitializeMoveSpeed;

	//추가. 24 2 14
	[SerializeField]
	private Renderer objectRenderer;
	[SerializeField]
	private VisualEffect damagedVFXgraph;
	[SerializeField]
	private VisualEffect dieVFXgraph;
	[SerializeField]
	private Material[] skinnedMaterials;


	private float dissolveRate = 0.025f;
	private float refreshRate = 0.025f;


	[SerializeField]
	private NetworkVariable<bool> _isDead = new NetworkVariable<bool>(false);

	//[SerializeField]
	//private NetworkVariable<bool> _isActive = new NetworkVariable<bool>(true);

	[SerializeField]
	private NetworkVariable<float> _health = new NetworkVariable<float>();

	[SerializeField]
	private NetworkVariable<float> _maxHealth = new NetworkVariable<float>();

	// Health로 기능 이전 예정
	//[SerializeField, Obsolete]
	//private NetworkVariable<float> _resistance = new NetworkVariable<float>();

	//[SerializeField]
	//private NetworkVariable<int> _damage = new NetworkVariable<int>();

	//[SerializeField]
	//private NetworkVariable<float> _cooldown = new NetworkVariable<float>();

	[SerializeField]
	private float _sensitivity = 0.0F;

	[SerializeField]
	private NetworkVariable<FixedString128Bytes> _state = new(DEFAULT_STATE);

	[SerializeField]
	private NetworkVariable<float> _pathfindingDelay = new();

	public AudioClip attackSound;  //타격음 삽입해야해용
	public AudioClip hitSound; // 피격음ㄴ

	[SerializeField]
	private NavMeshAgent _agent;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private AudioSource _audioSource;

	[SerializeField]
	private BehaviorTree _behaviorTree;

	[SerializeField]
	private EnemyDeathTrigger _deathTrigger;

	[SerializeField]
	private EnemyLightInsightedTrigger _lightInsightedTrigger;

	[SerializeField]
	private EnemyTakeDamageTrigger[] _takeDamageTrigger;

	[SerializeField]
	private Loot[] _loots;

	private NetworkVariable<NetworkBehaviourReference> _target = new NetworkVariable<NetworkBehaviourReference>();

	//private CancellationTokenSource _onAttack;

	//private List<LightSource> _sighted = new List<LightSource>();

	private Coroutine _navMeshAgentCoroutine;
	private Coroutine _setDestinationDelayCoroutine;

	public static event EnemyDieDelegate OnEnemyDie;
	public static event PlayerKilledEnemyDelegate OnEnemyDieWithPlayer;

	public bool IsDead
	{
		get
		{
			return _isDead.Value;
		}

		set
		{
			_isDead.Value = value;
		}
	}

	//public bool IsActive
	//{
	//	get
	//	{
	//		return _isActive.Value;
	//	}

	//	set
	//	{
	//		_isActive.Value = value;
	//	}
	//}

	public float Health
	{
		get => _health.Value;

		set => _health.Value = value;
	}

	public float CurrentHealth
	{
		get => _health.Value;

		set => _health.Value = value;
	}

	//[Obsolete("InitializeHealthValue 값을 사용!")]
	//public float MaxHealth
	//{
	//	get => _maxHealth.Value;

	//	set => _maxHealth.Value = value;
	//}

	//[Obsolete]
	//public float Resistance
	//{
	//	get
	//	{
	//		return _resistance.Value;
	//	}

	//	set
	//	{
	//		_resistance.Value = value;
	//	}
	//}

	public string State
	{
		get
		{
			return _state.Value.ToString();
		}

		set
		{
			_state.Value = value;
		}
	}

	public Player Target
	{
		get
		{
			if (NetworkManager.Singleton)
			{
				var isEnable = _target.Value.TryGet(out Player value);

				return value;
			}

			return null;
		}

		set
		{
			//var isEnable = _target.Value.TryGet(out Player target);

			//if (!isEnable && IsServer)
			//{
			//	Player.OnDie += OnTargetDie;
			//}

			if (IsServer)
			{
				_target.Value = value;
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public bool IsMoving
	{
		get
		{
			return _navMeshAgentCoroutine != null;
		}
	}

	public Animator animator => _animator;

	public BehaviorTree BehaviorTree => _behaviorTree;

	private void Start()
	{
		skinnedMaterials = objectRenderer.materials;

		if (_behaviorTree && IsServer)
		{
			_behaviorTree.EnableBehavior();
		}
	}

	//private void Update()
	//{
	//	//if (Target)
	//	//{
	//	//	Debug.Log($"{name}.{GetInstanceID()}의 타겟은 {Target}");
	//	//}
	//}

	//public override void OnDestroy()
	//{
	//	base.OnDestroy();

	//	//_onAttack?.Cancel();
	//	//_onAttack?.Dispose();
	//}

	public override void OnNetworkSpawn()
	{
		//base.OnNetworkSpawn();

		_pathfindingDelay.Value = _sensitivity;

		_isDead.OnValueChanged += OnIsDeadChanged;
		//_resistance.OnValueChanged += OnHealthChanged;
		_health.OnValueChanged += OnHealthChanged;

		//UpdateManager.OnUpdate += OnUpdate;

		// 이렇게 하면 server에서만 작동하게 할 수 있나
		//if (_behaviorTree && IsServer)
		//{
		//	_behaviorTree.EnableBehavior();
		//}

		//if (ServerPermissionHandler.Instance.IsSpawned)
		//{
		//	Debug.Log($"{ServerPermissionHandler.Instance.IsServer} sjhdjhdhskshdkd~");
		//}

		//if (NetworkManager.Singleton && ServerPermissionHandler.Instance.IsServer)
		//{
		//	Debug.Log($"{ServerPermissionHandler.Instance.IsServer} 좋았으~");

		//	Player.OnDie += OnTargetDie;
		//}

		if (NetworkManager.Singleton && IsServer)
		{
			Player.OnDie += OnTargetDie;
		}
	}

	public override void OnNetworkDespawn()
	{
		//base.OnNetworkDespawn();

		_isDead.OnValueChanged -= OnIsDeadChanged;
		//_resistance.OnValueChanged -= OnHealthChanged;
		_health.OnValueChanged -= OnHealthChanged;

		//UpdateManager.OnUpdate -= OnUpdate;

		//if (NetworkManager.Singleton && ServerPermissionHandler.Instance.IsServer)
		//{
		//	Player.OnDie -= OnTargetDie;
		//}

		if (NetworkManager.Singleton && IsServer)
		{
			Player.OnDie -= OnTargetDie;
		}
	}

	//private void OnUpdate()
	//{
	//	_cooldown.Value = Math.Max(_cooldown.Value - Time.deltaTime, 0.0F);
	//}

	// 네트워크에서 못 찾을 수 있으니 (최소한 스테이지가) 완전히 끝나기 전엔 Despawn 하면 안댐
	// 아닌가
	private void OnIsDeadChanged(bool previousValue, bool newValue)
	{
		if (!previousValue.Equals(newValue))
		{
			if (newValue)
			{
				//gameObject.SetActive(false);
				StartCoroutine(DieEffect());
				_behaviorTree?.DisableBehavior();
			}
			else
			{
				gameObject.SetActive(true);
			}

			//gameObject.SetActive(!newValue);

			//StartCoroutine(DieEffect());
		}

		//Debug.LogError("설마 이거 호출 안됨?");

		if (newValue/* && !_isActive.Value*/)
		{
			foreach (var loot in _loots)
			{
				loot.Execute(this);
			}
		}
	}

	private void OnHealthChanged(float oldValue, float newValue)
	{
		DamagedEffect();

		//if (!_isTargetKilled.Value && (newValue < 0.0f || Mathf.Approximately(newValue, 0.0f)))
		//{
		//	Die();
		//}
	}

	public void OnTargetDie()
	{
		var isEnable = _target.Value.TryGet(out Player player);

		if (isEnable && player.IsDead)
		{
			Debug.Log($"타겟 사망확인");

			_target.Value = default;
		}
	}

	public void OnLightInsighted(SpotLight light)
	{
		Debug.Log("인식은 됨?");

		//_sighted.Add(light);
		_lightInsightedTrigger.OnUpdate(this, light);
	}

	//데미지 받았을 때 이펙트.
	public void DamagedEffect()
	{
		//float healthRatio = Mathf.Clamp01(1 - (_resistance.Value / _maxHealth.Value)); // 0~1 값으로 제한
		//float healthRatio = Mathf.Clamp01(1 - (_resistance.Value / InitializeResistanceValue)); // 0~1 값으로 제한
		//float healthRatio = Mathf.Clamp01(1 - (_currentHealth.Value / InitializeHealthValue)); // 0~1 값으로 제한

		float healthRatio = Mathf.InverseLerp(InitializeHealthValue, 0.0F, _health.Value); // 0~1 값으로 제한

		//Debug.Log($"Damaged : {_resistance.Value}");
		//Debug.Log($"healthRatio : {healthRatio}");

		if (dieVFXgraph != null && damagedVFXgraph != null)
		{
			damagedVFXgraph.Play();
		}

		// 모든 머티리얼에 적용
		foreach (Material mat in skinnedMaterials)
		{
			mat.SetFloat("_ColorFillAmount", healthRatio);
		}

		if (_audioSource)
		{
			_audioSource.PlayOneShot(hitSound);
		}
	}

	public IEnumerator DieEffect()
	{
		if (dieVFXgraph != null && damagedVFXgraph != null)
		{
			Debug.Log("Enemy DieEffectPlaying...");
			damagedVFXgraph.Stop();
			dieVFXgraph.Play();
		}

		if (skinnedMaterials.Length > 0)
		{
			float counter = 0;

			while (skinnedMaterials[0].GetFloat("_DissolveAmount") < 1)
			{
				counter += dissolveRate;
				for (int i = 0; i < skinnedMaterials.Length; i++)
				{
					skinnedMaterials[i].SetFloat("_DissolveAmount", counter);
				}
				yield return new WaitForSeconds(refreshRate);
			}
		}
	}

	public void TakeDamage(float amount, AudioClip hitSound)
	{
		var handle = new DamageHandle()
		{
			Target = this,

			Damage = amount
		};

		// 나중에 피격음을 여기로 빼야함
		foreach (var trigger in _takeDamageTrigger)
		{
			trigger?.OnUpdate(handle);
		}

		var oldValue = _health.Value;
		var newValue = Mathf.Max(oldValue - handle.Damage, 0.0F);

		Debug.Log($"테스트 1번 데미지 : {amount}, 테스트 2번 데미지 : {handle.Damage}, 테스트 3번 데미지 : {oldValue - newValue}");

		if (oldValue != newValue)
		{
			_health.Value = newValue;

			if (_audioSource != null)
			{
				// 여기다 피격음 넣으면 되는데 일단 넣음
				_audioSource.PlayOneShot(hitSound);
			}
		}

		if (!_isDead.Value && (newValue < 0.0f || Mathf.Approximately(newValue, 0.0f)))
		{
			Die();

			OnEnemyDieWithPlayer?.Invoke();
		}
	}

	public void Die()
	{
		StopMove();

		//if (_onAttack != null && !_onAttack.IsCancellationRequested)
		//{
		//	_onAttack?.Cancel();
		//	_onAttack?.Dispose();
		//}

		//_deathTrigger.OnUpdate(this);

		OnDead().Forget();

		async UniTaskVoid OnDead()
		{
			if (_animator)
			{
				_animator.applyRootMotion = true;
				//_animator.Play("Dead", 0, 0.0F);
				_animator.SetBool("IsDead", true);
			}

			//await UniTask.WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsName("Dead").);

			_behaviorTree.DisableBehavior();
			_deathTrigger.OnUpdate(this);

			OnEnemyDie?.Invoke(this);

			await UniTask.Delay(TimeSpan.FromSeconds(3.67F));

			gameObject?.SetActive(false);

			_animator.applyRootMotion = false;
		}
	}

	public void StartMove()
	{
		if (_animator)
		{
			_animator.SetBool(WALKING_STATE, true);
		}
	}

	public void StopMove()
	{
		if (_agent)
		{
			_agent.ResetPath();
		}

		if (_animator)
		{
			_animator.SetBool(WALKING_STATE, false);
		}

		_navMeshAgentCoroutine = null;

		//_agent?.ResetPath();
		//_animator?.SetBool(WALKING_STATE, false);
	}

	// 대상만 설정!
	[Rpc(SendTo.Server)]
	public void SetPrioritizePlayerAggroRPC(NetworkBehaviourReference reference)
	{
		var isPlayerEnable = reference.TryGet(out Player player);
		var isTargetEnable = _target.Value.TryGet(out Player target);

		if (isPlayerEnable)
		{
			if (!isTargetEnable)
			{
				//var message = $"타겟이 {player.name}({player.OwnerClientId})로 설정되었습니다.";

				//Debug.Log(message);

				_target.Value = reference;
			}
			//else if (isTargetEnable && target.Equals(player))
			//{
			//	//var message = $"타겟이 이미 {player.name}({player.OwnerClientId})입니다.";

			//	//Debug.Log(message);
			//}
		}
	}

	// 위치 지정까지!
	[Rpc(SendTo.Server)]
	public void SetAggroTargetServerRPC(NetworkBehaviourReference reference)
	{
		var isEnable = reference.TryGet(out Player player);

		SetPrioritizePlayerAggroRPC(reference);

		if (isEnable)
		{
			SetDestinationServerRPC(player.transform.position);
		}
	}

	[Rpc(SendTo.Server)]
	public void SetDestinationServerRPC(Vector3 destination)
	{
		if (IsMoving)
		{
			//var distance = Vector3.Distance(_agent.destination, destination);
			//var weight = Mathf.Min(distance, _sensitivity);

			Debug.Log($"SetDestination Delay : {_setDestinationDelayCoroutine}");

			if (_setDestinationDelayCoroutine == null)
			{
				if (_setDestinationDelayCoroutine != null)
				{
					Debug.Log($"SetDestination dhfhfhfhfhfhfhfhfhfhf : {_setDestinationDelayCoroutine}");
				}

				SetDestination_Internal(destination);

				if (_sensitivity > 0.0F)
				{
					Debug.Log("SetDestination Delay Start");

					_setDestinationDelayCoroutine = StartCoroutine(SetDelay());;
				}
			}

			//_pathfindingDelay.Value += weight;

			//if (_pathfindingDelay.Value >= _sensitivity)
			//{
			//	_pathfindingDelay.Value -= _sensitivity;

			//	SetDestination_Internal(destination);
			//}
		}
		else
		{
			SetDestination_Internal(destination);
		}
	}

	private void SetDestination_Internal(Vector3 destination)
	{
		var isOnNavMesh = NavMesh.SamplePosition(destination, out var hit, 1.0F, NavMesh.AllAreas);
		var position = isOnNavMesh ? destination : hit.position;

		_agent.SetDestination(position);

		if (IsMoving)
		{
			StopCoroutine(_navMeshAgentCoroutine);
		}

		if (!IsMoving)
		{
			_navMeshAgentCoroutine = StartCoroutine(OnNavMeshRunning());
		}
	}

	private IEnumerator OnNavMeshRunning()
	{
		StartMove();

		yield return new WaitUntil(() => _agent.remainingDistance <= _agent.stoppingDistance);

		StopMove();

		yield return null;
	}

	private IEnumerator SetDelay()
	{
		yield return new WaitForSeconds(_sensitivity);

		_setDestinationDelayCoroutine = null;

		yield return null;
	}

	//[Rpc(SendTo.Server)]
	//public void SetDestinationServerRPC(Vector3 position)
	//{
	//	var tmp = NavMesh.SamplePosition(position, out var hit, 1.0F, NavMesh.AllAreas);
	//}

	//[ContextMenu("TakeDamage")]
	//public void Test_TakeDamage()
	//{
	//	TakeDamage(1.0F, null);
	//}
}
