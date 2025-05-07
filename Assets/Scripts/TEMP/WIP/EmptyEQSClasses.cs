using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.Rendering.GPUSort;

namespace InTheDark.Prototypes
{
	public ref struct EnvironmentQueryResult
	{
		public bool HasValue;

		public Vector3 Value;

		public EnvironmentQueryResult(bool hasValue, Vector3 value)
		{
			HasValue = hasValue;
			Value = value;
		}

		public static implicit operator bool(EnvironmentQueryResult result)
		{
			var hasValue = result.HasValue;

			return hasValue;
		}

		public static implicit operator Vector3(EnvironmentQueryResult result)
		{
			var value = result.Value;

			return value;
		}
	}

	public class EnvironmentQueryItem : IDisposable
	{
		public bool HasValue;
		public float Value;

		public Vector3 LocalPosition;
		public Vector3 NavMeshPosition;

		public Transform Querier;

		public EnvironmentQueryItem()
		{

		}

		public EnvironmentQueryItem(Vector3 position, Transform querier)
		{
			LocalPosition = position;
			NavMeshPosition = position;

			Querier = querier;

			OnReset();
		}

		public void Dispose()
		{
			Dispose(this, EventArgs.Empty);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(object sender, EventArgs e)
		{
			Querier = default;
		}

		public Vector3 GetWorldPosition()
		{
			var worldPostion = NavMeshPosition + Querier.position;

			return worldPostion;
		}

		public void OnReset()
		{
			var worldPosition = GetWorldPosition();
			var isPathFound = NavMesh.SamplePosition(worldPosition, out var hit, 1F, NavMesh.AllAreas);

			HasValue = isPathFound;
			Value = 0F;

			NavMeshPosition = isPathFound ? hit.position : LocalPosition;
		}
	}

	public class EnvironmentQuery
	{
		private EnvironmentQueryOption[] _options;

		public EnvironmentQuery()
		{

		}

		public EnvironmentQuery(EnvironmentQueryOption[] options)
		{
			_options = options;
		}

		public EnvironmentQueryResult Run(Dictionary<string, Transform> args)
		{
			var result = new EnvironmentQueryResult();

			for (var i = 0; i < _options.Length && !result; i++)
			{
				result = _options[i].Run(args);
			}

			return result;
		}
	}

	public class EnvironmentQueryOption
	{
		private EnvironmentQueryGenerator _generator;

		public EnvironmentQueryOption()
		{

		}

		public EnvironmentQueryOption(EnvironmentQueryGenerator generator)
		{
			_generator = generator;
		}

		public EnvironmentQueryResult Run(Dictionary<string, Transform> args)
		{
			var items = _generator.Generate(args);
			var best = items
				.Where(item => item.HasValue && item.Value < 0.0F)
				.OrderByDescending(item => item.Value)
				.FirstOrDefault();

			var hasValue = best is not null;
			var value = hasValue ? best.GetWorldPosition() : Vector3.zero;

			var result = new EnvironmentQueryResult(hasValue, value);

			return result;
		}
	}

	public abstract class EnvironmentQueryGenerator
	{
		private EnvironmentQueryTest[] _tests;

		protected EnvironmentQueryGenerator()
		{

		}

		protected EnvironmentQueryGenerator(EnvironmentQueryTest[] tests)
		{
			_tests = tests;
		}

		public abstract IEnumerable<EnvironmentQueryItem> Generate(Dictionary<string, Transform> args);
		
		protected void Run(IEnumerable<EnvironmentQueryItem> items, Dictionary<string, Transform> args)
		{
			foreach (var item in items)
			{
				for (var i = 0; i < _tests.Length && item.HasValue; i++)
				{
					_tests[i].OnGenerate(item, args);
				}
			}
		}
	}

	public abstract class EnvironmentQueryTest
	{
		public abstract void OnGenerate(EnvironmentQueryItem item, Dictionary<string, Transform> args);
	}

	public abstract class ScriptableEnvironmentQueryGenerator : ScriptableObject
	{
		[SerializeField]
		private ScriptableEnvironmentQueryTest[] _tests;

		public abstract EnvironmentQueryGenerator Create();

		protected EnvironmentQueryTest[] GetTests()
		{
			var length = _tests.Length;
			var tests = new EnvironmentQueryTest[length];

			for (var i = 0; i < length; i++)
			{
				var test = _tests[i].Create();

				tests[i] = test;
			}

			return tests;
		}
	}

	public abstract class ScriptableEnvironmentQueryTest : ScriptableObject
	{
		public abstract EnvironmentQueryTest Create();
	}
}