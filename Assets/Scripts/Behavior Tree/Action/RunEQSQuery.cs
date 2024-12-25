using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using InTheDark.Prototypes;
using System.Collections.Generic;
using UnityEngine;

public class RunEQSQuery : BehaviorDesigner.Runtime.Tasks.Action
{
    public SharedVector3 target;
	public SharedQueryArguments _args;

	public EnvironmentQuery query;

	public ScriptableEnvironmentQuery scriptableQuery;

	public override void OnAwake()
	{
		if (scriptableQuery)
		{
			query = scriptableQuery.Create();
		}

		if (_args.Value is null)
		{
			Debug.Log("argument is null!!!");

			_args.Value = new();
		}

		_args.Value.Add("QUERIER", transform);
	}

	public override TaskStatus OnUpdate()
	{
		var queryResult = query.Run(_args.Value);
		var taskResult = queryResult ? TaskStatus.Success : TaskStatus.Failure;

		if (queryResult)
		{
			target.SetValue(queryResult.Value);
		}

		return taskResult;
	}
}
