using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.Netcode.Components;

namespace InTheDark.Prototypes
{
	// 이름은 산경쓰지 말도록 하자...
	public class ChargeSkillManager : NetworkBehaviour
	{
		[SerializeField]
		private float _duration;

		[SerializeField]
		private float _initialCooldown;
		
		[SerializeField]
		private NetworkVariable<float> _time;

		[SerializeField]
		private NetworkVariable<float> _cooldown;

		//[SerializeField]
		//private float _minSpeed;

		//[SerializeField]
		//private float _speed;

		[SerializeField]
		private float _magnification;

		//[SerializeField]
		//private bool _isRunning = false;

		[SerializeField]
		private AnimationCurve _speedCurve;

		[SerializeField]
		private EnemyPrototypePawn _pawn;

		[SerializeField]
		private CharacterController _controller;

		[SerializeField]
		private NavMeshAgent _agent;

		[SerializeField]
		private bool _isRunning = false;

		//[SerializeField]
		//private Vector3 _velocity;

		//public float Cooldown
		//{
		//	get
		//	{
		//		return _cooldown.Value;
		//	}
		//}

		public bool IsEnable
		{
			get
			{
				var target = _pawn.Target;
				var isCooldown = Mathf.Approximately(_cooldown.Value, 0.0F);

				var isEnable = target && isCooldown && !_isRunning;

				return isEnable;
			}
		}

		//private void OnCollisionEnter(Collision collision)
		//{
		//	collision.gameObject.CompareTag("");
		//}

		private void Awake()
		{
			_time = new();
			_cooldown = new();
		}

		private void OnUpdate()
		{
			//if (_isRunning && IsServer)
			//{
			//	var time = _time.Value;
			//	var lerped = Mathf.Lerp(_minSpeed, _speed, time);
			//	//var velocity = direction.normalized * lerped;

			//	Debug.Log($"둥 + {name}");
			//	_controller.Move(_velocity * lerped * Time.deltaTime);

			//	_time.Value += Time.deltaTime;
			//}

			var c = _cooldown.Value;

			if (IsServer && 0.0F < c)
			{
				var value = Mathf.Max(0.0F, c - Time.deltaTime);

				_cooldown.Value = value;
			}
		}

		public override void OnNetworkSpawn()
		{
			UpdateManager.OnUpdate += OnUpdate;
		}

		public override void OnNetworkDespawn()
		{
			UpdateManager.OnUpdate -= OnUpdate;
		}

		//public IEnumerator ActiveAnother()
		//{
		//	var target = _pawn.Target;

		//	Debug.Log("1번 포트");

		//	if (target)
		//	{
		//		var direction = target.transform.position - transform.position;
		//		var velocity = direction.normalized * _speed;

		//		Debug.Log($"2번 포트 + {velocity}");

		//		_velocity = velocity;

		//		transform.LookAt(target.transform);

		//		_agent.isStopped = true;
		//		_rigidbody.isKinematic = true;
		//		_isRunning = true;
		//		//_rigidbody.linearVelocity = velocity;

		//		yield return new WaitForSeconds(_time);

		//		Debug.Log("3번 포트");

		//		_velocity = Vector3.zero;

		//		_agent.isStopped = false;
		//		_rigidbody.isKinematic = false;
		//		_isRunning = false;
		//		//_rigidbody.linearVelocity = Vector3.zero;
		//	}
		//}

		//public async UniTask Active(CancellationToken token)
		//{
		//	var target = _pawn.Target;

		//	Debug.Log($"1번 포트 + {target.name} + {target.transform.position}");

		//	if (target)
		//	{
		//		_time.Value = 0.0F;

		//		var time = _time.Value;

		//		//var lerped = Mathf.Lerp(time, _speed, time + _duration);
		//		var direction = target.transform.position - transform.position;
		//		//var velocity = direction.normalized * lerped;

		//		//Debug.Log($"2번 포트 + {velocity}");

		//		//_velocity = velocity;
		//		_velocity = direction.normalized;

		//		transform.LookAt(target.transform);

		//		_agent.isStopped = true;
		//		//_rigidbody.isKinematic = true;
		//		_isRunning = true;
		//		//_rigidbody.linearVelocity = velocity;

		//		Debug.Log($"3번 포트 + {_velocity} + {_isRunning}");

		//		await UniTask.Delay(TimeSpan.FromSeconds(_duration), false, PlayerLoopTiming.Update, token, false);

		//		Debug.Log("4번 포트");

		//		_velocity = Vector3.zero;

		//		_agent.isStopped = false;
		//		//_rigidbody.isKinematic = false;
		//		_isRunning = false;
		//		//_rigidbody.linearVelocity = Vector3.zero;
		//	}
		//}

		public IEnumerator Active()
		{
			var target = _pawn.Target;

			Debug.Log($"1번 포트 + {target.name} + {target.transform.position} // {transform.position}");

			if (target && IsServer)
			{
				var direction = target.transform.position - transform.position;
				var normalized = direction.normalized;

				_time.Value = 0.0F;

				transform.LookAt(target.transform.position);

				//Debug.Log($"3번 포트 + {_velocity} + {_isRunning}");

				//while (_time.Value < _duration)
				{
					_agent.isStopped = false;
					_agent.speed = 10f;                // 돌진 속도
					_agent.acceleration = 2f;  // 원하는 경우 가속도도 설정
					_agent.SetDestination(target.transform.position);
				}
				
				_cooldown.Value = _initialCooldown;
			}

			yield return null;
		}

		//private async UniTask OnActive()
		//{
		//	transform.LookAt(_pawn.Target?.transform);

		//	_rigidbody.linearVelocity = Vector3.zero;
		//}
	} 
}