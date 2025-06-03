using BehaviorDesigner.Runtime.Tasks;

using UnityEngine;

public class OnSpawn : Action
{
	public TaskStatus status = TaskStatus.Inactive;

	public string targetStateName;

	public Animator animator;
	public EnemyPrototypePawn pawn;

	public override void OnAwake()
	{
		pawn = GetComponent<EnemyPrototypePawn>();
		animator = pawn.animator;
	}

	public override TaskStatus OnUpdate()
	{
		if (status.Equals(TaskStatus.Inactive))
		{
			status = TaskStatus.Running;

			animator?.SetTrigger("OnSpawn");
		}
		else if (status.Equals(TaskStatus.Running))
		{
			var isRunning = animator.GetCurrentAnimatorStateInfo(0) is var info && info.IsName(targetStateName) && info.normalizedTime <= 1.0F;

			status = isRunning ? TaskStatus.Running : TaskStatus.Success;
		}

		return status;
	}
}
