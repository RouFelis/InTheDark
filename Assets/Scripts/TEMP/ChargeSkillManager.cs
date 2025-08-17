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
		private float _range;

		[SerializeField]
		private float _duration;

		[SerializeField]
		private float _flightTime;

		[SerializeField]
		private float _flightSpeed;

		[SerializeField]
		private float _initialCooldown;

		[SerializeField]
		private float _stunTime;
		
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

		[SerializeField]
		private NetworkVariable<bool> _isStunning = new();

		private List<Player> _onClash = new();


		//�߰��� ���׵�
		[SerializeField]
		private AnimationCurve _knockbackCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); //Ŀ�� �ִϸ��̼� (X,Z)

		[SerializeField]
		private float _knockbackY = 10f; //���� �� ����;;

		[SerializeField]
		private CapsuleCollider _capsuleCollider;

		[SerializeField]
		private LayerMask _layerMask;
		private LayerMask playerLayer;
		private LayerMask wallLayer;

		public bool IsEnable
		{
			get
			{
				var target = _pawn.Target;
				var isCooldown = Mathf.Approximately(_cooldown.Value, 0.0F);

				var isEnable = target && isCooldown && !_isRunning && IsTargetNearby;

				return isEnable;
			}
		}

		public bool IsTargetNearby
		{
			get
			{
				var target = _pawn.Target;

				if (!target)
				{
					return false;
				}

				var direction = target.transform.position - transform.position;
				var isNearBy = target ? Vector3.Distance(target.transform.position, transform.position) <= _range : false;
				var isOccultation = Physics.Raycast(transform.position, direction, out var hit, _range);
				var isAttackable = isNearBy && isOccultation && hit.collider.CompareTag("Player");

				return isAttackable;
			}
		}


		private void Awake()
		{
			playerLayer = LayerMask.NameToLayer("Player");
			wallLayer = LayerMask.NameToLayer("Wall");
		}

		/*private void OnCollisionEnter(Collision collision)
		{
			*//*if (_isRunning && collision.transform.root.CompareTag("Player"))
			{
				var contact = collision.contacts[0];
				var hitNormal = contact.normal;

				var pushDir = -hitNormal.normalized;

				var player = collision.transform.root.GetComponent<Player>();

				Debug.Log($"{collision.transform.root.name} �΋H��");

				StartCoroutine(OnCollisionPlayer(player, pushDir));
			}*//*
			// 7.30 ��1�� ����
			if (!_isRunning)
				return;

			var root = collision.transform.root;

			// �÷��̾�� �ε����� ��
			if (root.CompareTag("Player"))
			{
				var contact = collision.contacts[0];
				var hitNormal = contact.normal;
				var pushDir = -hitNormal.normalized;
				var player = root.GetComponent<Player>();

				Debug.Log($"{root.name} �΋H��");

				StartCoroutine(OnCollisionPlayer(player, pushDir));
			}
			else if(root.CompareTag("Wall"))
			{
				// �ٸ� ������Ʈ�� �浹���� �� ���� ����
				Debug.Log($"���÷��̾� �浹: {root.name}, ���� ������");

				StopCharge();
			}
		}*/


		private void CheckPlayerClash()
		{
			if (_capsuleCollider == null) return;

			// ���� ���� �߽� ��ġ ���
			Vector3 centerWorld = transform.TransformPoint(_capsuleCollider.center);
			float radius = _capsuleCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
			float height = _capsuleCollider.height * transform.lossyScale.y;


			// ĸ���� �� �� ���� ���
			Vector3 up = transform.up;
			float halfHeight = Mathf.Max(0, height / 2f - radius); // ĸ�� ���� ����
			Vector3 point1 = centerWorld + up * halfHeight;
			Vector3 point2 = centerWorld - up * halfHeight;

			// �÷��̾ ����
			var hits = Physics.OverlapCapsule(point1, point2, radius, _layerMask, QueryTriggerInteraction.Ignore);

			foreach (var hit in hits)
			{
				if (hit.gameObject.layer == playerLayer)
				{
					var player = hit.GetComponent<Player>();
					if (player != null && !_onClash.Contains(player))
					{
						Vector3 pushDir = (player.transform.position - transform.position).normalized;
						StartCoroutine(OnCollisionPlayer(player, pushDir));
					}
				}
				else if(hit.gameObject.layer == wallLayer)
				{
					// ���� �ڵ� �߰�
					StopCharge();
					StunServerRPC();

					Debug.Log("���Ӹ�������");
				}
			}
		}

		//���� ���� �浹�� ���ߴ� �ڵ�
		private void StopCharge()
		{
			if (!_isRunning) return;

			_isRunning = false;

			if (_agent != null)
				_agent.isStopped = false;

			_cooldown.Value = _initialCooldown;
		}

		private IEnumerator OnCollisionPlayer(Player player, Vector3 direction)
		{
			/*		var time  = 0.0F;
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

							Debug.Log($"{player}�� {count}��° �浹 ó���� {power}��ŭ ���ư�!");

							cc.Move(power);
						}

						count++;

						yield return count;
					}

					_onClash.Remove(player);

					yield return null;*/
			// 7.30 ���������ϴ� ^^7

			if (_onClash.Contains(player)) yield break;
			_onClash.Add(player);

			if (!player.IsDead)
			{
				//player.TakeDamage(1.234F, null);

				//yield return StartCoroutine(player.SetStun(_flightTime, _flightSpeed, _knockbackCurve, _knockbackY , direction));

				OnClashPlayerRPC(player, direction);
			}

			_onClash.Remove(player);
		}

		private void OnUpdate()
		{
			var c = _cooldown.Value;

			if (IsServer && 0.0F < c && !_isRunning)
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
				/*				var direction = _pawn.Target.transform.position - transform.position;
								var normalized = direction.normalized;

								_time.Value = 0.0F;
								_isRunning = true;
								_agent.isStopped = true;

								//transform.LookAt(_pawn.Target.transform.position);

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
								_cooldown.Value = _initialCooldown;*/
				var direction = _pawn.Target.transform.position - transform.position;

				// ���� ���Ϳ��� Y ���� (���� �̵�)
				Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z);
				if (flatDirection == Vector3.zero)
					flatDirection = transform.forward; // Ÿ�ٰ� ���� ���� ��� ���� �������� ����

				Vector3 normalized = flatDirection.normalized;
				Quaternion lookRotation = Quaternion.LookRotation(normalized);
				transform.rotation = lookRotation;

				_time.Value = 0.0f;
				_isRunning = true;
				_agent.isStopped = true;

				float clashCheckTimer = 0f;

				while (_time.Value < _duration)
				{
					// �߰��� �浹 ������ ������ �ߴܵ� ���
					if (!_isRunning)
					{
						yield break;
					}

					clashCheckTimer += Time.deltaTime;

					if (clashCheckTimer >= 0.05f)
					{
						clashCheckTimer = 0f;
						CheckPlayerClash();
					}

					var t = _time.Value / _duration;
					var speed = _magnification * _speedCurve.Evaluate(t);

					//Debug.Log($"{_time.Value} secends... :: {speed}/s...");

					transform.rotation = lookRotation;

					_controller.Move(speed * normalized * Time.deltaTime - new Vector3(0.0F, 20.0F, 0.0F));

					_time.Value += Time.deltaTime;

					yield return null;
				}

				_agent.isStopped = false;
				_isRunning = false;
				_cooldown.Value = _initialCooldown;

				StopCharge();
			}

			yield return null;
		}

		[Rpc(SendTo.Server)]
		public void OnClashPlayerRPC(NetworkBehaviourReference reference, Vector3 direction)
		{
			var isEnable = reference.TryGet(out Player player);

			if (isEnable)
			{
				player.TakeDamage(1.234F, null);

				StartCoroutine(player.SetStun(_flightTime, _flightSpeed, _knockbackCurve, _knockbackY, direction));
			}
		}

		[Rpc(SendTo.Server)]
		private void StunServerRPC()
		{
			if (!_isStunning.Value)
			{
				StartCoroutine(Stun());
			}
		}

		[Rpc(SendTo.Everyone)]
		private void StartStunClientRPC()
		{
			_pawn.StopMove();

			if (_agent)
				_agent.isStopped = true;

			if (_pawn.BehaviorTree)
				_pawn.BehaviorTree.DisableBehavior();
		}

		[Rpc(SendTo.Everyone)]
		private void EndStunClientRPC()
		{
			if (_agent)
				_agent.isStopped = false;

			if (_pawn.BehaviorTree)
				_pawn.BehaviorTree.EnableBehavior();
		}

		private IEnumerator Stun()
		{
			_isStunning.Value = true;
			StartStunClientRPC();

			yield return new WaitForSeconds(_stunTime);

			_isStunning.Value = false;
			EndStunClientRPC();

			yield return null;
		}
	} 
}