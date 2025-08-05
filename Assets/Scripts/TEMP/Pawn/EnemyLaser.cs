using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using UnityEngine.VFX;

namespace InTheDark.Prototypes
{
	//[RequireComponent(typeof(LineRenderer))]
	public class EnemyLaser : EnemyWeapon
	{
		private const string VFX_IS_FIRING = "isFiring";

		public float Delay = 0.5F;

		public float duration;
		public float maxDistance = 100f;

		public bool IsFiring = false;

		public VisualEffect LaserVFX;
		public Transform LaserTransform;
		//public LayerMask TargetLayerMask;

		private void Awake()
		{
			if (!LaserVFX)
			{
				LaserVFX = GetComponentInChildren<VisualEffect>();
			}

			IsFiring = false;

			LaserVFX.SetBool(VFX_IS_FIRING, IsFiring);
			LaserVFX.Stop();
		}

		protected override async UniTask OnAttack(IHealth target)
		{
			var time = 0.0F;
			var navMeshAgent = GetComponent<NavMeshAgent>();
			var player = _pawn.Target?.GetComponent<Player>();

			var targetPos = player.FirstPersonCamera.transform.position;
			var tempPos = LaserTransform.transform.position + new Vector3(0.0F, 0.05F, 0.0F);

			var chargingDis = Vector3.Distance(LaserTransform.position, tempPos);
			var chargingPos = LaserTransform.transform.InverseTransformPoint(tempPos);

			if (IsFiring)
			{
				return;
			}

			IsFiring = true;

			LaserVFX.Reinit();
			LaserVFX.SetBool(VFX_IS_FIRING, IsFiring);
			LaserVFX.Play();

			transform.LookAt(targetPos);

			_animator?.SetTrigger("OnAttack");

			LaserVFX.SetVector3("Direction", transform.up);
			LaserVFX.SetFloat("Length", chargingDis);
			LaserVFX.SetVector3("TargetPos", chargingPos);

			_pawn.StopMove();

			navMeshAgent.enabled = false;

			while (duration + Delay > time && !_pawn.IsDead)
			{
				if (time > Delay)
				{
					var raycast = Physics.Raycast(transform.position, LaserTransform.forward, out var hit, maxDistance);
					var hitPlayer = hit.collider?.GetComponent<Player>();

					//Debug.Log($"playerPos: {player?.transform.position}, cameraPos: {player?.FirstPersonCamera.transform.position}");

					var targetPosition = raycast
						? hitPlayer
							? hitPlayer.FirstPersonCamera.transform.position
							: hit.point
						: transform.position + transform.forward * maxDistance;

					var distance = Vector3.Distance(LaserTransform.position, targetPosition);
					var position = LaserTransform.transform.InverseTransformPoint(targetPosition);
					var direction = targetPosition - LaserTransform.transform.position;

					_pawn.StopMove();

					LaserVFX.SetVector3("Direction", direction.normalized);
					LaserVFX.SetFloat("Length", distance);
					LaserVFX.SetVector3("TargetPos", position);

					Debug.Log($"player: {hitPlayer}, target: {target}, hit: {hit.collider}/{hit.collider?.name}");

					if (IsTargetNearby && hitPlayer && hitPlayer.Equals(target))
					{
						target.TakeDamage(_damage, _pawn.attackSound);
					}
				}

				time = Mathf.Min(time + Time.deltaTime, duration + Delay);

				await UniTask.NextFrame();
			}

			if (IsFiring)
			{
				IsFiring = false;

				LaserVFX.SetBool(VFX_IS_FIRING, IsFiring);
				LaserVFX.Stop();

				navMeshAgent.enabled = true;
			}
		}

		//private void OnDrawGizmos()
		//{
		//	var isHit = Physics.Raycast(LaserTransform.position, LaserTransform.forward.normalized, out RaycastHit hit, maxDistance);
		//	var targetPosition = isHit
		//		? hit.point // (O)
		//		: transform.position + transform.forward * maxDistance; // (X)

		//	Gizmos.color = Color.green;
		//	Gizmos.DrawLine(LaserTransform.position, hit.point);
		//}
	}
}