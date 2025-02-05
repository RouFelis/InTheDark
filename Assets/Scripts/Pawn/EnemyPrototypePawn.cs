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

public class EnemyPrototypePawn : NetworkPawn, IHealth
{
	public const string DEFAULT_STATE = "Normal";

	public const string WALKING_STATE = "IsWalking";

	public float InitializeCooldownValue;
	public int InitializeHealthValue;
	[Obsolete] public float InitializeResistanceValue;

	[SerializeField]
	private NetworkVariable<bool> _isDead = new NetworkVariable<bool>(false);

	[SerializeField]
	private NetworkVariable<bool> _isActive = new NetworkVariable<bool>(true);

	[SerializeField]
	private NetworkVariable<float> _health = new NetworkVariable<float>();

	// Health로 기능 이전 예정
	[SerializeField, Obsolete]
	private NetworkVariable<float> _resistance = new NetworkVariable<float>();

	[SerializeField]
	private NetworkVariable<int> _damage = new NetworkVariable<int>();

	[SerializeField]
	private NetworkVariable<float> _cooldown = new NetworkVariable<float>();

	[SerializeField]
	private NetworkVariable<FixedString128Bytes> _state = new(DEFAULT_STATE);

	public AudioClip attackSound;  //타격음 삽입해야해용

	[SerializeField]
	private NavMeshAgent _agent;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private EnemyDeathTrigger _deathTrigger;

	[SerializeField]
	private EnemyLightInsightedTrigger _lightInsightedTrigger;

	[SerializeField]
	private Loot[] _loots;

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

	public override void OnDestroy()
	{
		base.OnDestroy();

		_onAttack?.Cancel();
		_onAttack?.Dispose();
	}

	public override void OnNetworkSpawn()
	{
		//base.OnNetworkSpawn();

		_isDead.OnValueChanged += OnIsDeadChanged;
		_resistance.OnValueChanged += OnResistanceChanged;

		UpdateManager.OnUpdate += OnUpdate;
	}

	public override void OnNetworkDespawn()
	{
		//base.OnNetworkDespawn();

		_isDead.OnValueChanged -= OnIsDeadChanged;
		_resistance.OnValueChanged -= OnResistanceChanged;

		UpdateManager.OnUpdate -= OnUpdate;
	}

	private void OnUpdate()
	{
		_cooldown.Value = Math.Max(_cooldown.Value - Time.deltaTime, 0.0F);
	}

	// 네트워크에서 못 찾을 수 있으니 완전히 끝나기 전엔 Despawn 하면 안댐
	private void OnIsDeadChanged(bool previousValue, bool newValue)
	{
		if (!previousValue.Equals(newValue))
		{
			//if (newValue)
			//{
			//	gameObject.SetActive(false);
			//}
			//else
			//{
			//	gameObject.SetActive(true);
			//}

			gameObject.SetActive(!newValue);
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

	private void OnResistanceChanged(float oldValue, float newValue)
	{
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
	//	//		var isSight = Vector3.Angle(direction, transform.forward) < _angle;

	//	//		if (hit.collider == source && isOccultation && isSight && current < source)
	//	//		{
	//	//			current = source;
	//	//		}
	//	//	}
	//	//}

	//	//_resistance.Value -= Time.deltaTime * current.DamagePercent;

	//	//_sighted.Clear();
	//}

	public void OnLightInsighted(LightSource light)
	{
		//_sighted.Add(light);

		_lightInsightedTrigger.OnUpdate(this, light);
	}

	public void TakeDamage(float amount , AudioClip hitSound)
	{
		throw new NotImplementedException();
	}

	//public void Attack(ICharacter target)
	//{
	//	throw new NotImplementedException();
	//}

	//public void AttackPrototype(NetworkPawn target)
	//{
	//	// 대충 공격했다고 이벤트 알림^^
	//}

	// 갸아아악
	public void AttackPrototype(IHealth target)
	{
		if (_cooldown.Value < 0.0F || Mathf.Approximately(_cooldown.Value, 0.0F))
		{
			_cooldown.Value = InitializeCooldownValue;

			//target.TakeDamage(_damage.Value , attackSound);
			//_animator?.SetTrigger("OnAttack");

			OnAttackWithAnimaiton(target).Forget();
		}
	}

	private async UniTaskVoid OnAttackWithAnimaiton(IHealth target)
	{
		_animator?.SetTrigger("OnAttack");

		await UniTask.Delay(TimeSpan.FromSeconds(1.0F));

		target.TakeDamage(_damage.Value, attackSound);
		Debug.Log("HIT!!!");
	}

	public void Dead()
	{
		//_isDead.Value = true;

		//NetworkObject.Despawn();

		//Destroy(gameObject);

		//_deathTrigger.OnUpdate(this);
	}

	public void Die()
	{
		_onAttack?.Cancel();
		_onAttack?.Dispose();

		_deathTrigger.OnUpdate(this);
	}

	public void StartMove()
	{
		_animator?.SetBool(WALKING_STATE, true);
	}

	public void StopMove()
	{
		_agent?.ResetPath();
		_animator?.SetBool(WALKING_STATE, false);
	}
}
