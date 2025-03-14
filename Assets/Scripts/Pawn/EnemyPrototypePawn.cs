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

public class EnemyPrototypePawn : NetworkPawn, IHealth
{
	public const string DEFAULT_STATE = "Normal";

	public const string WALKING_STATE = "IsWalking";
	public const string ATTACK_TRIGGER = "OnAttack";

	public float InitializeCooldownValue;
	public int InitializeHealthValue;
	[Obsolete] public float InitializeResistanceValue;
	public float InitializeMoveSpeed;

	//�߰�. 24 2 14
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

	[SerializeField]
	private NetworkVariable<bool> _isActive = new NetworkVariable<bool>(true);

	[SerializeField]
	private NetworkVariable<float> _health = new NetworkVariable<float>();

	[SerializeField]
	private NetworkVariable<float> _maxHealth = new NetworkVariable<float>();

	// Health�� ��� ���� ����
	[SerializeField, Obsolete]
	private NetworkVariable<float> _resistance = new NetworkVariable<float>();

	[SerializeField]
	private NetworkVariable<int> _damage = new NetworkVariable<int>();

	[SerializeField]
	private NetworkVariable<float> _cooldown = new NetworkVariable<float>();

	[SerializeField]
	private NetworkVariable<FixedString128Bytes> _state = new(DEFAULT_STATE);

	public AudioClip attackSound;  //Ÿ���� �����ؾ��ؿ�
	public AudioClip hitSound; // �ǰ�����

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
	private Loot[] _loots;

	private NetworkVariable<NetworkBehaviourReference> _target = new NetworkVariable<NetworkBehaviourReference>();

	private CancellationTokenSource _onAttack;

	//private List<LightSource> _sighted = new List<LightSource>();

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

	public bool IsActive
	{
		get
		{
			return _isActive.Value;
		}

		set
		{
			_isActive.Value = value;
		}
	}

	public float Health 
	{
		get=> _health.Value; 

		set => _health.Value = value;
	}

	[Obsolete("InitializeHealthValue ���� ���!")]
	public float MaxHealth
	{
		get => _maxHealth.Value;

		set => _maxHealth.Value = value;
	}

	[Obsolete]
	public float Resistance
	{
		get
		{
			return _resistance.Value;
		}

		set
		{
			_resistance.Value = value;
		}
	}

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
			var isEnable = _target.Value.TryGet(out Player value);

			return value;
		}

		set
		{
			//var isEnable = _target.Value.TryGet(out Player target);

			//if (!isEnable && IsServer)
			//{
			//	Player.OnDie += OnTargetDie;
			//}

			_target.Value = value;
		}
	}

	private void Start()
	{
		skinnedMaterials = objectRenderer.materials;

		if (_behaviorTree && IsServer)
		{
			_behaviorTree.EnableBehavior();
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();

		//_onAttack?.Cancel();
		//_onAttack?.Dispose();
	}

	public override void OnNetworkSpawn()
	{
		//base.OnNetworkSpawn();

		_isDead.OnValueChanged += OnIsDeadChanged;
		_resistance.OnValueChanged += OnResistanceChanged;

		UpdateManager.OnUpdate += OnUpdate;

		// �̷��� �ϸ� server������ �۵��ϰ� �� �� �ֳ�
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
		//	Debug.Log($"{ServerPermissionHandler.Instance.IsServer} ������~");

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
		_resistance.OnValueChanged -= OnResistanceChanged;

		UpdateManager.OnUpdate -= OnUpdate;

		//if (NetworkManager.Singleton && ServerPermissionHandler.Instance.IsServer)
		//{
		//	Player.OnDie -= OnTargetDie;
		//}

		if (NetworkManager.Singleton && IsServer)
		{
			Player.OnDie -= OnTargetDie;
		}
	}

	private void OnUpdate()
	{
		_cooldown.Value = Math.Max(_cooldown.Value - Time.deltaTime, 0.0F);
	}

	// ��Ʈ��ũ���� �� ã�� �� ������ (�ּ��� ����������) ������ ������ ���� Despawn �ϸ� �ȴ�
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

			StartCoroutine(DieEffect());
		}

		//Debug.LogError("���� �̰� ȣ�� �ȵ�?");

		if (newValue/* && !_isActive.Value*/)
		{
			foreach (var loot in _loots)
			{
				loot.Execute(this);	
			}
		}
	}

	private void OnResistanceChanged(float oldValue, float newValue)
	{
		DamagedEffect();

		if (!_isDead.Value && (newValue < 0.0f || Mathf.Approximately(newValue, 0.0f)))
		{
			//Dead();
			Die();
		}
	}

	//public void OnLightInsighted()
	//{
	//	//var current = _sighted[0];

	//	//if (_sighted.Count > 1)
	//	//{
	//	//	for (var i = 1; i < _sighted.Count; i++)
	//	//	{
	//	//		var source = _sighted[i];
	//	//		var direction = source.transform.position - transform.position;
	//	//		var isOccultation = Physics.Raycast(transform.position, direction, out var hit, _distance);
	//	//		var isSight = Vector3.Range(direction, transform.forward) < _angle;

	//	//		if (hit.collider == source && isOccultation && isSight && current < source)
	//	//		{
	//	//			current = source;
	//	//		}
	//	//	}
	//	//}

	//	//_resistance.Value -= Time.deltaTime * current.DamagePercent;

	//	//_sighted.Clear();
	//}

	public void OnTargetDie()
	{
		var isEnable = _target.Value.TryGet(out Player player);

		if (isEnable && player.IsDead)
		{
			Debug.Log($"Ÿ�� ���Ȯ��");

			_target.Value = default;
		}
	}

	// �ǰ����� ����� �߰��ϸ� �Ǳ� �ѵ�... ��...
	// ��Ȯ���� ���� ����... Ʈ���� �ȿ��ٰ�...
	public void OnLightInsighted(LightSource light)
	{
		//_sighted.Add(light);
		//_lightInsightedTrigger.OnUpdate(this, light);
	}

	public void OnLightInsighted(SpotLight light)
	{
		//_sighted.Add(light);
		_lightInsightedTrigger.OnUpdate(this, light);
	}

	//������ �޾��� �� ����Ʈ.
	public void DamagedEffect()
	{
		//float healthRatio = Mathf.Clamp01(1 - (_resistance.Value / _maxHealth.Value)); // 0~1 ������ ����
		float healthRatio = Mathf.Clamp01(1 - (_resistance.Value / InitializeResistanceValue)); // 0~1 ������ ����

		//Debug.Log($"Damaged : {_resistance.Value}");
		//Debug.Log($"healthRatio : {healthRatio}");

		if (dieVFXgraph != null && damagedVFXgraph != null)
		{
			damagedVFXgraph.Play();
		}

		// ��� ��Ƽ���� ����
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

	public void TakeDamage(float amount , AudioClip hitSound)
	{
		var oldValue = _resistance.Value;
		var newValue = Mathf.Max(oldValue - amount, 0.0F);

		if (oldValue != newValue)
		{
			_resistance.Value = newValue;

			// ����� �ǰ��� ������ �Ǵµ� �ϴ� ����
			_audioSource.PlayOneShot(hitSound);
		}
	}

	//public void Attack(ICharacter value)
	//{
	//	throw new NotImplementedException();
	//}

	//public void AttackPrototype(NetworkPawn value)
	//{
	//	// ���� �����ߴٰ� �̺�Ʈ �˸�^^
	//}

	// ���ƾƾ�
	public void AttackPrototype(IHealth target)
	{
		if (_cooldown.Value < 0.0F || Mathf.Approximately(_cooldown.Value, 0.0F))
		{
			_cooldown.Value = InitializeCooldownValue;

			//value.TakeDamage(_damage.Value , attackSound);
			//_animator?.SetTrigger("OnAttack");

			OnAttackWithAnimaiton(target).Forget();
		}
	}

	// �� �ٵ� �̰� �����غ��� ���� ���ݿ��� ���� �ִ� �ڵ��ε�
	private async UniTaskVoid OnAttackWithAnimaiton(IHealth target)
	{
		using var source = new CancellationTokenSource();

		if (_animator)
		{
			_animator.SetTrigger(ATTACK_TRIGGER);
			_onAttack = source;
		}

		//_animator?.SetTrigger(ATTACK_TRIGGER);

		await UniTask.Delay(TimeSpan.FromSeconds(0.9F), false, PlayerLoopTiming.Update, source.Token, false);

		target.TakeDamage(_damage.Value, attackSound);
		//Debug.Log("HIT!!!");

		_onAttack = default;
	}

	//public void Dead()
	//{
	//	//_isDead.Value = true;

	//	//NetworkObject.Despawn();

	//	//Destroy(gameObject);

	//	//_deathTrigger.OnUpdate(this);
	//}

	public void Die()
	{
		StopMove();

		if (_onAttack != null && !_onAttack.IsCancellationRequested)
		{
			_onAttack?.Cancel();
			_onAttack?.Dispose();
		}

		//_deathTrigger.OnUpdate(this);

		OnDead().Forget();

		async UniTaskVoid OnDead()
		{
			if (_animator)
			{
				_animator.applyRootMotion = true;
				_animator.Play("Dead", 0, 0.0F);
			}

			//await UniTask.WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsName("Dead").);

			_behaviorTree.DisableBehavior();
			_deathTrigger.OnUpdate(this);

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

		//_agent?.ResetPath();
		//_animator?.SetBool(WALKING_STATE, false);
	}
}
