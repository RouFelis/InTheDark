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
		private float _flightTime;

		[SerializeField]
		private float _flightSpeed;

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

		private List<Player> _onClash = new();

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
			if (_isRunning && collision.transform.root.CompareTag("Player"))
			{
				var contact = collision.contacts[0];
				var hitNormal = contact.normal;

				var pushDir = -hitNormal.normalized;

				var player = collision.transform.root.GetComponent<Player>();

				Debug.Log($"{collision.transform.root.name} 부딫힘");

				StartCoroutine(OnCollisionPlayer(player, pushDir));
			}
			//else
			//{
			//	Debug.Log($"? 그런거 모름");
			//}
		}

		private IEnumerator OnCollisionPlayer(Player player, Vector3 direction)
		{
			var time = 0.0F;
			var count = 0;

			if (_onClash.Contains(player))
			{
				yield break;
			}

			_onClash.Add(player);

			while (!player.IsDead && time < _flightTime)
			{
				var deltaTime = Time.deltaTime;
				var tempTime = time;

				var cc = player.GetComponent<CharacterController>();

				time = Mathf.Min(time + deltaTime, _flightTime);
				deltaTime = Mathf.Min(tempTime - time, deltaTime);

				if (cc)
				{
					var power = direction * _flightSpeed * deltaTime / _flightTime;

					Debug.Log($"{player}가 {count}번째 충돌 처리로 {power}만큼 날아감!");

					cc.Move(power);
				}

				count++;

				yield return count;
			}

			_onClash.Remove(player);

			yield return null;
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