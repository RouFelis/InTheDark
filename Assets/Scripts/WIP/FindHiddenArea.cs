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
					Debug.Log($"제발 {navMeshHitInfo.position}가 된다면 한번만이라도 메세지를...ㅠㅠㅠ");
				}

				Debug.Log($"원본이 {point}인데 어떻게... 아니 그냥 {navMeshHitInfo.position}일단 뜨,ㅣ워");

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

					Debug.Log($"{result} 로 합의...");
				}

				//Debug.Log($"{result} jbfhsdbfjdsbdfjb");
			}

			return result;
		}
	}
}