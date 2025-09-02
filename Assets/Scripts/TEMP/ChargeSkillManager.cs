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


		//추가한 사항들
		[SerializeField]
		private AnimationCurve _knockbackCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); //커브 애니메이션 (X,Z)

		[SerializeField]
		private float _knockbackY = 10f; //점프 힘 ㅇㅇ;;

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

				Debug.Log($"{collision.transform.root.name} 부딫힘");

				StartCoroutine(OnCollisionPlayer(player, pushDir));
			}*//*
			// 7.30 수1정 ㄷㄷ
			if (!_isRunning)
				return;

			var root = collision.transform.root;

			// 플레이어에게 부딪혔을 때
			if (root.CompareTag("Player"))
			{
				var contact = collision.contacts[0];
				var hitNormal = contact.normal;
				var pushDir = -hitNormal.normalized;
				var player = root.GetComponent<Player>();

				Debug.Log($"{root.name} 부딫힘");

				StartCoroutine(OnCollisionPlayer(player, pushDir));
			}
			else if(root.CompareTag("Wall"))
			{
				// 다른 오브젝트에 충돌했을 때 돌진 중지
				Debug.Log($"비플레이어 충돌: {root.name}, 돌진 중지됨");

				StopCharge();
			}
		}*/


		private void CheckPlayerClash()
		{
			if (_capsuleCollider == null) return;

			// 월드 기준 중심 위치 계산
			Vector3 centerWorld = transform.TransformPoint(_capsuleCollider.center);
			float radius = _capsuleCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
			float height = _capsuleCollider.height * transform.lossyScale.y;


			// 캡슐의 두 끝 지점 계산
			Vector3 up = transform.up;
			float halfHeight = Mathf.Max(0, height / 2f - radius); // 캡슐 형태 유지
			Vector3 point1 = centerWorld + up * halfHeight;
			Vector3 point2 = centerWorld - up * halfHeight;

			// 플레이어만 감지
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
					// 기절 코드 추가
					StopCharge();
					StunServerRPC();

					Debug.Log("나머리쿵해쪄");
				}
			}
		}

		//돌진 벽에 충돌시 멈추는 코드
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

							Debug.Log($"{player}가 {count}번째 충돌 처리로 {power}만큼 날아감!");

							cc.Move(power);
						}

						count++;

						yield return count;
					}

					_onClash.Remove(player);

					yield return null;*/
			// 7.30 수정했읍니다 ^^7

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

				// 방향 벡터에서 Y 제거 (수평 이동)
				Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z);
				if (flatDirection == Vector3.zero)
					flatDirection = transform.forward; // 타겟과 겹쳐 있을 경우 현재 방향으로 돌진

				Vector3 normalized = flatDirection.normalized;
				Quaternion lookRotation = Quaternion.LookRotation(normalized);
				transform.rotation = lookRotation;

				_time.Value = 0.0f;
				_isRunning = true;
				_agent.isStopped = true;

				float clashCheckTimer = 0f;

				while (_time.Value < _duration)
				{
					// 중간에 충돌 등으로 돌진이 중단된 경우
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