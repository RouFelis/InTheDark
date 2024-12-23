using System.Collections.Generic;
using UnityEngine;

namespace InTheDark.Prototypes
{
	public class SimpleGrid : EnvironmentQueryGenerator
	{
		private int _count;
		private float _space;

		public SimpleGrid() : base()
		{

		}

		public SimpleGrid(EnvironmentQueryTest[] tests, int count, float space) : base(tests)
		{
			_count = count;
			_space = space;
		}

		public override IEnumerable<EnvironmentQueryItem> Generate(Dictionary<string, Transform> args)
		{
			var querier = args["QUERIER"];
			var items = new List<EnvironmentQueryItem>();

			for (var x = -_count; x < _count + 1; x++)
			{
				for (var y = -_count; y < _count + 1; y++)
				{
					for (var z = -_count; z < _count + 1; z++)
					{
						var localPosition = querier.position + new Vector3(x * _space, y * _space, z * _space);
						var item = new EnvironmentQueryItem(localPosition, querier);

						items.Add(item);
					}
				}
			}

			Run(items, args);

			return items;
		}
	}

	[CreateAssetMenu(fileName = FILE_NAME, menuName = MENU_NAME)]
	public class ScriptableSimpleGrid : ScriptableEnvironmentQueryGenerator
	{
		private const string FILE_NAME = "SimpleGrid";
		private const string MENU_NAME = "Scriptable Objects/EnvQueryGenerator/SimpleGrid";

		[SerializeField]
		private int _count;

		[SerializeField]
		private float _space;

		public override EnvironmentQueryGenerator Create()
		{
			var tests = GetTests();

			return new SimpleGrid(tests, _count, _space);
		}
	} 
}