using UnityEngine;

namespace InTheDark.Prototypes
{
	[CreateAssetMenu(fileName = FILE_NAME, menuName = MENU_NAME)]
	public class ScriptableEnvironmentQueryOption : ScriptableObject
	{
		private const string FILE_NAME = "EnvQueryOption";
		private const string MENU_NAME = "Scriptable Objects/EnvQueryOption";

		[SerializeField]
		private ScriptableEnvironmentQueryGenerator _generator;

		public EnvironmentQueryOption Create()
		{
			var generator = _generator.Create();

			return new EnvironmentQueryOption(generator);
		}
	}
}