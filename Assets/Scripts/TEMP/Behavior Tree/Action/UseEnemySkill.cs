using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace InTheDark.Prototypes
{
	public class UseEnemySkill : BehaviorDesigner.Runtime.Tasks.Action
	{
		//public float Time;

		//public bool isActivated = false;

		private TaskStatus _executionStatus = TaskStatus.Failure;

		private EnemyPrototypePawn _pawn;
		private ChargeSkillManager _skillHandler;

		//private CancellationTokenSource _onSkillStop;

		public override void OnAwake()
		{
			_pawn = GetComponent<EnemyPrototypePawn>();
			_skillHandler = GetComponent<ChargeSkillManager>();
		}

		public override TaskStatus OnUpdate()
		{
			//if (!isActivated && _executionStatus == TaskStatus.Inactive && _pawn.Target)
			//{
			//	Debug.Log($"{_pawn.Target} 있는거...맞지?");

			//	isActivated = true;
			//	_executionStatus = TaskStatus.Running;

			//	//OnSkillActive().Forget();
			//	StartCoroutine(OnSkillStart());
			//}

			//Debug.Log($"{_pawn.Target} 있는거...맞지?");

			//isActivated = true;
			//_executionStatus = TaskStatus.Running;

			if (_executionStatus != TaskStatus.Running)
			{
				_executionStatus = TaskStatus.Running;

				StartCoroutine(OnSkillStart());
			}
			else
			{
				_executionStatus = TaskStatus.Failure;
			}

			//StartCoroutine(OnSkillStart());

			//Time += UnityEngine.Time.deltaTime;

			return _executionStatus;
		}

		//public override void OnPause(bool paused)
		//{
		//	//Debug.Log("sfhfhhhhghghhhhhhh");

		//	//isActivated = false;

		//	//OnCompleted(true);
		//}

		//public override void OnEnd()
		//{
		//	//Debug.Log("sfhfhhhhghghhhhhhh?????");

		//	//_executionStatus = TaskStatus.Inactive;
		//}

		//private async UniTaskVoid OnSkillActive()
		//{
		//	using var onSkillStop = new CancellationTokenSource();

		//	var skill = GetComponent<ChargeSkillManager>();

		//	_onSkillStop = onSkillStop;

		//	if (skill && _pawn.Target)
		//	{
		//		//await skill.Active(_onSkillStop.Token);
		//		await skill.Active(_onSkillStop.Token);
		//	}

		//	//await UniTask.Delay(TimeSpan.FromSeconds(12.0F), false, PlayerLoopTiming.Update, _onSkillStop.Token, false);

		//	OnCompleted();

		//	_executionStatus = TaskStatus.Failure;
		//}

		private IEnumerator OnSkillStart()
		{
			//var skill = GetComponent<ChargeSkillManager>();

			if (_skillHandler && _pawn.Target)
			{
				yield return StartCoroutine(_skillHandler.Active());
			}

			_executionStatus = TaskStatus.Success;
		}

		//private void OnCompleted(bool isPause = false)
		//{
		//	if (_onSkillStop != null)
		//	{
		//		if (isPause)
		//		{
		//			_onSkillStop?.Cancel();
		//		}

		//		_onSkillStop = null;
		//	}
		//}
	} 
}