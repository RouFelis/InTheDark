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
	// �̸��� ��澲�� ������ ����...
	public class ChargeSkillManager : NetworkBehaviour
	{
		[SerializeField]
		private float _duration;
		
		[SerializeField]
		private NetworkVariable<float> _time;

		[SerializeField]
		private float _speed;

		//[SerializeField]
		//private bool _isRunning = false;

		[SerializeField]
		private EnemyPrototypePawn _pawn;

		[SerializeField]
		private CharacterController _controller;

		[SerializeField]
		private NavMeshAgent _agent;

		[SerializeField]
		private bool _isRunning = false;

		[SerializeField]
		private Vector3 _velocity;

		//private void OnCollisionEnter(Collision collision)
		//{
		//	collision.gameObject.CompareTag("");
		//}

		private void Awake()
		{
			_time = new();
		}

		private void Update()
		{
			if (_isRunning)
			{
				Debug.Log($"�� + {name}");
				_controller.Move(_velocity * Time.deltaTime);
			}
		}

		//public IEnumerator ActiveAnother()
		//{
		//	var target = _pawn.Target;

		//	Debug.Log("1�� ��Ʈ");

		//	if (target)
		//	{
		//		var direction = target.transform.position - transform.position;
		//		var velocity = direction.normalized * _speed;

		//		Debug.Log($"2�� ��Ʈ + {velocity}");

		//		_velocity = velocity;

		//		transform.LookAt(target.transform);

		//		_agent.isStopped = true;
		//		_rigidbody.isKinematic = true;
		//		_isRunning = true;
		//		//_rigidbody.linearVelocity = velocity;

		//		yield return new WaitForSeconds(_time);

		//		Debug.Log("3�� ��Ʈ");

		//		_velocity = Vector3.zero;

		//		_agent.isStopped = false;
		//		_rigidbody.isKinematic = false;
		//		_isRunning = false;
		//		//_rigidbody.linearVelocity = Vector3.zero;
		//	}
		//}

		public async UniTask Active(CancellationToken token)
		{
			var target = _pawn.Target;

			Debug.Log($"1�� ��Ʈ + {target.name} + {target.transform.position}");

			if (target)
			{
				var time = _time.Value;

				var lerped = Mathf.Lerp(time, _speed, time + _duration);
				var direction = target.transform.position - transform.position;
				var velocity = direction.normalized * lerped;

				Debug.Log($"2�� ��Ʈ + {velocity}"); 

				_velocity = velocity;

				transform.LookAt(target.transform);

				_agent.isStopped = true;
				//_rigidbody.isKinematic = true;
				_isRunning = true;
				//_rigidbody.linearVelocity = velocity;

				Debug.Log($"3�� ��Ʈ + {_velocity} + {_isRunning}");

				await UniTask.Delay(TimeSpan.FromSeconds(_duration), false, PlayerLoopTiming.Update, token, false);

				Debug.Log("4�� ��Ʈ");

				_velocity = Vector3.zero;

				_agent.isStopped = false;
				//_rigidbody.isKinematic = false;
				_isRunning = false;
				//_rigidbody.linearVelocity = Vector3.zero;
			}
		}

		//private async UniTask OnActive()
		//{
		//	transform.LookAt(_pawn.Target?.transform);

		//	_rigidbody.linearVelocity = Vector3.zero;
		//}
	} 
}