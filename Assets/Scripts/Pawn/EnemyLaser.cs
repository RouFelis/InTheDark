using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace InTheDark.Prototypes
{
	[RequireComponent(typeof(LineRenderer))]
	public class EnemyLaser : EnemyWeapon
	{
		public float duration;

		[Header("Laser Settings")]
		public float maxDistance = 100f;
		public float width = 0.1f;
		public Color laserColor = Color.red;

		[Header("Effects...�ε� �̰� �߿����� ����")]
		public float scrollSpeed = 1f;
		public float brightness = 2f;
		public Material laserMaterial;

		public LineRenderer lineRenderer;
		private Vector3[] positions = new Vector3[2];

		private void Awake()
		{
			lineRenderer.startWidth = 0.0F;
			lineRenderer.endWidth = 0.0F;
		}

		protected override async UniTask OnAttack(IHealth target)
		{
			var time = 0.0F;
			var direction = _pawn.Target.transform.position - transform.position;

			using var source = new CancellationTokenSource();

			//Debug.Log("�̰� Ȥ�� ȣ���� �ȴٸ� ������ �˷���");

			//if (_animator)
			//{
			//	_animator.SetTrigger(EnemyPrototypePawn.ATTACK_TRIGGER);
			//	//_onAttack = source;
			//}

			if (IsTargetNearby)
			{
				target.TakeDamage(_damage, _pawn.attackSound);
			}

			transform.LookAt(_pawn.Target.transform);

			positions[0] = transform.position + new Vector3(0.0F, 1.75F, 0.0F) + transform.forward * 0.5F; // �̰� ������ �ϵ��ڵ����� �ٲ�� �ϴµ�
			positions[1] = Physics.Raycast(positions[0], direction, out RaycastHit hit, maxDistance)
				? hit.point // (O)
				: transform.position + transform.forward * maxDistance; // (X)

			// �浹 üũ
			//if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxDistance))
			//{
			//	positions[1] = hit.point;
			//}

			lineRenderer.material = laserMaterial;

			laserMaterial.SetColor("_Color", laserColor);
			laserMaterial.SetFloat("_ScrollSpeed", scrollSpeed);
			laserMaterial.SetFloat("_Brightness", brightness);

			lineRenderer.SetPositions(positions);

			while (duration >= time)
			{
				var timeTemp = Mathf.Min(time + Time.deltaTime, duration);
				//var widthTemp = width * (1.0F - timeTemp / duration);
				var widthTemp = Mathf.Lerp(width, 0.0F, timeTemp / duration);

				time = timeTemp;

				lineRenderer.startWidth = widthTemp;
				lineRenderer.endWidth = widthTemp;

				await UniTask.NextFrame();
			}

			//Debug.Log("����������������������");
		}
	} 
}