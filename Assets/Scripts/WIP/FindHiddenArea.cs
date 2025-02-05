using UnityEngine;
using UnityEngine.AI;

namespace InTheDark.Prototypes
{
	[CreateAssetMenu(fileName = "FindHiddenArea", menuName = "Scriptable Objects/FindHiddenArea")]
	public class FindHiddenArea : PositionGenerator
	{
		[SerializeField]
		private PositionGenerator _target;

		[SerializeField]
		private float _radius;

		[SerializeField]
		private int _maxCount;

		public override Vector3 Generate()
		{
			var players = FindObjectsByType<Player>(FindObjectsSortMode.None);

			var result = _target.Generate();
			var isRunning = true;

			for (var i = 0; i < _maxCount && isRunning; i++)
			{
				var point = Random.insideUnitSphere * _radius + result;
				var isEnable = NavMesh.SamplePosition(point, out var navMeshHitInfo, _radius, NavMesh.AllAreas);

				if (isEnable)
				{
					Debug.Log($"���� {navMeshHitInfo.position}�� �ȴٸ� �ѹ����̶� �޼�����...�ФФ�");
				}

				Debug.Log($"������ {point}�ε� ���... �ƴ� �׳� {navMeshHitInfo.position}�ϴ� ��,�ӿ�");

				for (var j = 0; isEnable && j < players.Length; j++)
				{
					var player = players[j];

					var direction = player.transform.position - navMeshHitInfo.position;
					var isOccultation = Physics.Raycast(navMeshHitInfo.position, direction, out var raycastHitInfo, _radius);
					var isSight = Vector3.Angle(direction, player.transform.forward) < 80.0F;

					if (raycastHitInfo.collider == player && isOccultation && isSight)
					{
						isEnable = false;
					}
				}

				if (isEnable)
				{
					result = navMeshHitInfo.position;
					isRunning = false;

					Debug.Log($"{result} �� ����...");
				}

				//Debug.Log($"{result} jbfhsdbfjdsbdfjb");
			}

			return result;
		}
	}
}