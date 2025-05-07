using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

namespace InTheDark.Prototypes
{
	//[RequireComponent(typeof(LineRenderer))]
	public class EnemyLaser : EnemyWeapon
	{
		private const string VFX_IS_FIRING = "isFiring";

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

			IsFiring = true;

			LaserVFX.Reinit();
			LaserVFX.SetBool(VFX_IS_FIRING, IsFiring);
			LaserVFX.Play();

			transform.LookAt(_pawn.Target.transform.position);

			while (1.0F > time && !_pawn.IsDead)
			{
				time = Mathf.Min(time + Time.deltaTime, 1.0F);

				LaserVFX.SetVector3("Direction", Vector3.zero);
				LaserVFX.SetFloat("Length", 0.0F);
				LaserVFX.SetVector3("TargetPos", new Vector3(3.0F, 0.0F, 0.0F));
			}

			time = 0.0F;

			while (duration > time && !_pawn.IsDead)
			{
				var targetPosition =  Physics.Raycast(LaserTransform.position, LaserTransform.forward.normalized, out RaycastHit hit, maxDistance)
					? hit.point // (O)
					: transform.position + transform.forward * maxDistance; // (X)

				var distance = Vector3.Distance(LaserTransform.position, targetPosition);
				var position = LaserTransform.transform.InverseTransformPoint(targetPosition);
				var direction = targetPosition - LaserTransform.transform.position;

				time = Mathf.Min(time + Time.deltaTime, duration);

				//LaserVFX.SetVector3("Direction", LaserTransform.forward.normalized);
				LaserVFX.SetVector3("Direction", direction.normalized);
				LaserVFX.SetFloat("Length", distance);
				LaserVFX.SetVector3("TargetPos", position);

				if (IsTargetNearby)
				{
					target.TakeDamage(_damage, _pawn.attackSound);
				}

				await UniTask.NextFrame();
			}

			if (IsFiring)
			{
				IsFiring = false;

				LaserVFX.SetBool(VFX_IS_FIRING, IsFiring);
				LaserVFX.Stop();
			}

			//Debug.Log("끼에에에에에에에에에엑");
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