using BehaviorDesigner.Runtime.Tasks;

using UnityEngine;

public class OnSpawn : Action
{
	public TaskStatus status = TaskStatus.Inactive;

	public string targetStateName;

	public Animator animator;
	public EnemyPrototypePawn pawn;

	private float time;

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
			// �� �κ��� ������µ���
			var isRunning = animator.GetCurrentAnimatorStateInfo(0) is var info && info.IsName(targetStateName) && info.normalizedTime <= 1.0F;
			//var isRunning = time < 1.0F;

			//time += Time.deltaTime;

			//Debug.Log($"{time}�� ���: ����: {isRunning}/{status}");

			status = isRunning ? TaskStatus.Running : TaskStatus.Failure;
		}

		return status;
	}
}
