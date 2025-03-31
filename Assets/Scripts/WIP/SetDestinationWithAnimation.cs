using UnityEngine;
using UnityEngine.AI;

namespace BehaviorDesigner.Runtime.Tasks.Unity.UnityNavMeshAgent
{
	public class SetDestinationWithAnimation : Action
	{
		public SharedGameObject targetGameObject;
		[SharedRequired]
		public SharedVector3 destination;

		// cache the navmeshagent component
		private NavMeshAgent navMeshAgent;
		private GameObject prevGameObject;

		public override void OnStart()
		{
			var currentGameObject = GetDefaultGameObject(targetGameObject.Value);
			if (currentGameObject != prevGameObject)
			{
				navMeshAgent = currentGameObject.GetComponent<NavMeshAgent>();
				prevGameObject = currentGameObject;
			}
		}

		public override TaskStatus OnUpdate()
		{
			if (navMeshAgent == null)
			{
				Debug.LogWarning("NavMeshAgent is null");
				return TaskStatus.Failure;
			}

			var isEnable = navMeshAgent.SetDestination(destination.Value);
			var result = isEnable ? TaskStatus.Success : TaskStatus.Failure;

			if (isEnable)
			{
				var pawn = gameObject.GetComponent<EnemyPrototypePawn>();

				pawn?.StartMove();
			}

			return result;
		}

		public override void OnReset()
		{
			targetGameObject = null;
			destination = Vector3.zero;
		}

		//public override void OnEnd()
		//{
		//	base.OnEnd();

		//	Debug.Log($"{gameObject.name} ³ª ³¡³µ¾î!");
		//}
	}
}