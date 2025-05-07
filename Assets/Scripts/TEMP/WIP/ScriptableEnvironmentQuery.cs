using UnityEngine;

namespace InTheDark.Prototypes
{
	[CreateAssetMenu(fileName = FILE_NAME, menuName = MENU_NAME)]
	public class ScriptableEnvironmentQuery : ScriptableObject
	{
		private const string FILE_NAME = "EnvQuery";
		private const string MENU_NAME = "Scriptable Objects/EnvQuery";

		[SerializeField]
		private ScriptableEnvironmentQueryOption[] _options;

		public EnvironmentQuery Create()
		{
			var length = _options.Length;
			var options = new EnvironmentQueryOption[length];

			for (var i = 0; i < length; i++)
			{
				var option = _options[i].Create();

				options[i] = option;
			}

			return new EnvironmentQuery(options);
		}
	}
}