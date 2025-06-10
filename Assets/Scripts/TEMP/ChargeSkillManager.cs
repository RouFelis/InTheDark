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
		private NetworkVariable<float> _time = new();

		[SerializeField]
		private NetworkVariable<float> _cooldown = new();

		[SerializeField]
		private float _magnification;

		[SerializeField]
		private AnimationCurve _speedCurve = new();

		[SerializeField]
		private EnemyPrototypePawn _pawn;

		[SerializeField]
		private CharacterController _controller;

		[SerializeField]
		private NavMeshAgent _agent;

		[SerializeField]
		private bool _isRunning = false;

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

		private void OnCollisionEnter(Collision collision)
		{
			
		}

		private void OnUpdate()
		{
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

		public IEnumerator Active()
		{
			if (_pawn.Target && IsServer)
			{
				var direction = _pawn.Target.transform.position - transform.position;
				var normalized = direction.normalized;

				_time.Value = 0.0F;
				_isRunning = true;
				_agent.isStopped = true;

				transform.LookAt(_pawn.Target.transform.position);

				while (_time.Value < _duration)
				{
					var t = _time.Value / _duration;
					var speed = _magnification * _speedCurve.Evaluate(t);

					_controller.Move(speed * normalized * Time.deltaTime);

					_time.Value += Time.deltaTime;

					yield return null;
				}

				_agent.isStopped = false;
				_isRunning = false;
				_cooldown.Value = _initialCooldown;
			}

			yield return null;
		}
	} 
}