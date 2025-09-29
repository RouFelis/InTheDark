using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.AI;

namespace InTheDark.Prototypes
{
    public class LizardChargeAttack : EnemyWeapon
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
		private float _stunTime;

		[SerializeField]
		private NetworkVariable<float> _time = new();

		//[SerializeField]
		//private NetworkVariable<float> _cooldown = new();

		[SerializeField]
		private float _speed;

		//[SerializeField]
		//private AnimationCurve _speedCurve = new();

		//[SerializeField]
		//private EnemyPrototypePawn _pawn;

		[SerializeField]
		private CharacterController _controller;

		[SerializeField]
		private NavMeshAgent _agent;

		[SerializeField]
		private bool _isRunning = false;

		private List<Player> _onClash = new();


		//추가한 사항들
		[SerializeField]
		private AnimationCurve _knockbackCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); //커브 애니메이션 (X,Z)

		[SerializeField]
		private AnimationCurve _powerCurve = AnimationCurve.EaseInOut(0, 0.04F, 1, -0.06F); //커브 애니메이션 (Y)

		[SerializeField]
		private float _knockbackY = 10f; //점프 힘 ㅇㅇ;;

		[SerializeField]
		private CapsuleCollider _capsuleCollider;

		[SerializeField]
		private LayerMask _layerMask;
		private LayerMask _playerLayer;
		private LayerMask _wallLayer;

		private Vector3 _start;
		private Vector3 _end;

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

		//public bool IsTargetNearby
		//{
		//	get
		//	{
		//		var target = _pawn.Target;

		//		if (!target)
		//		{
		//			return false;
		//		}

		//		var direction = target.transform.position - transform.position;
		//		var isNearBy = target ? Vector3.Distance(target.transform.position, transform.position) <= _range : false;
		//		var isOccultation = Physics.Raycast(transform.position, direction, out var hit, _range);
		//		var isAttackable = isNearBy && isOccultation && hit.collider.CompareTag("Player");

		//		return isAttackable;
		//	}
		//}

		private void Awake()
		{
			_playerLayer = LayerMask.NameToLayer("Player");
			_wallLayer = LayerMask.NameToLayer("Wall");
		}

		private void CheckPlayerClash()
		{
			if (_capsuleCollider == null) return;

			//Debug.Log("들어오긴함?");

			// 월드 기준 중심 위치 계산
			Vector3 centerWorld = transform.TransformPoint(_capsuleCollider.center);
			float radius = _capsuleCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
			float height = _capsuleCollider.height * transform.lossyScale.y;


			// 캡슐의 두 끝 지점 계산
			Vector3 up = transform.up;
			//float halfHeight = height * 0.5F;
			float halfHeight = height * 0.5F; // 반지름이 높이의 절반을 초과하지 않도록 조정
			Vector3 point1 = centerWorld + up * halfHeight;
			Vector3 point2 = centerWorld - up * halfHeight;

			_start = point1;
			_end = point2;

			// 플레이어만 감지
			var hits = Physics.OverlapCapsule(point1, point2, radius, _layerMask, QueryTriggerInteraction.Ignore);

			//Debug.Log($"[{transform.lossyScale}++{centerWorld}]{point1}~{point2} 내에서 맞긴 함? => {hits.Length}?");

			foreach (var hit in hits)
			{
				Debug.Log($"snrk 맞긴 함? => {hit.name} + {hit.gameObject.layer}?");

				Debug.Log($"[{hit.gameObject.layer}] {hit.gameObject.layer == _playerLayer} // {hit.gameObject.layer == _wallLayer}");

				if (hit.gameObject.layer == _playerLayer)
				{
					var player = hit.GetComponent<Player>();

					Debug.Log($"어~ {player} 어서오고~");

					if (player != null && !_onClash.Contains(player))
					{
						Vector3 pushDir = (player.transform.position - transform.position).normalized;
						StartCoroutine(OnCollisionPlayer(player, pushDir));

						StopCharge();
					}
				}
				else if (hit.gameObject.layer == _wallLayer)
				{
					StopCharge();

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
			if (_onClash.Contains(player)) yield break;
			_onClash.Add(player);

			if (!player.IsDead)
			{
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

		protected override async UniTask OnAttack(IHealth target)
		{
			//
			//base.OnAttack(target);
			//

			if (_animator)
			{
				_animator.SetTrigger(EnemyPrototypePawn.ATTACK_TRIGGER);
			}

			//_animator?.SetTrigger(ATTACK_TRIGGER);

			//await UniTask.Delay(TimeSpan.FromSeconds(_delay));

			await UniTask.NextFrame();

			StartCoroutine(Active());
		}

		public IEnumerator Active()
		{
			if (_pawn.Target && IsServer)
			{
				var direction = _pawn.Target.transform.position - transform.position;

				// 방향 벡터에서 Y 제거 (수평 이동)
				Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z);
				if (flatDirection == Vector3.zero)
					flatDirection = transform.forward; // 타겟과 겹쳐 있을 경우 현재 방향으로 돌진

				Vector3 normalized = flatDirection.normalized;
				Quaternion lookRotation = Quaternion.LookRotation(normalized);

				Debug.Log("빙글빙글 춤을춰요 뱅뱅");

				transform.rotation = lookRotation;

				_time.Value = 0.0f;
				_isRunning = true;
				_agent.isStopped = true;

				float clashCheckTimer = 0f;

				while (_time.Value < _duration && _isRunning)
				{
					// 중간에 충돌 등으로 돌진이 중단된 경우
					//if (!_isRunning)
					//{
					//	yield break;
					//}

					//clashCheckTimer += Time.deltaTime;

					//if (clashCheckTimer >= 0.05f)
					//{
					//	clashCheckTimer = 0f;
					//	CheckPlayerClash();
					//}

					var t = _time.Value / _duration;
					//var speed = _magnification * _speedCurve.Evaluate(t);
					var speed = _speed;
					//var height = 0.5F * 0.015F;
					//var power = Mathf.InverseLerp(height, -height, t) + 15.0F;
					var power = _powerCurve.Evaluate(t);

					//Debug.Log($"{_time.Value} secends... :: {speed}/s...");

					transform.rotation = lookRotation;

					_controller.Move(speed * normalized * Time.deltaTime + new Vector3(0.0F, power, 0.0F));

					CheckPlayerClash();

					_time.Value += Time.deltaTime;

					yield return null;
				}

				//_agent.isStopped = false;
				//_isRunning = false;
				//_cooldown.Value = _initialCooldown;

				Debug.Log("높이높이 하늘높이 팔짝");

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
				//player.TakeDamage(1.234F, null);

				player.TakeDamage(_damage, _pawn.attackSound);

				var message = $"EnemyWeapon: {name} attacked {player} with {_damage} damage. (Player Health Left => {player.Health})";

				Debug.Log(message);

				StartCoroutine(player.SetStun(_flightTime, _flightSpeed, _knockbackCurve, _knockbackY, direction));
			}
		}

		protected override void OnDrawGizmos()
		{
			base.OnDrawGizmos();

			var center = Vector3.Lerp(_start, _end, 0.5F);
			var radius = Vector3.Distance(_start, center);

			Gizmos.DrawWireSphere(center, radius);
		}
	}
}