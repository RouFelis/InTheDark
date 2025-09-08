using System;

using UnityEngine;

namespace InTheDark.Prototypes
{
	[Serializable]
	public class EnemySpawnData
	{

	}

	[CreateAssetMenu(fileName = FILE_NAME, menuName = MENU_NAME)]
	public class ScriptableEnemySpawnData : ScriptableObject
	{
		private const string FILE_NAME = "ScriptableEnemySpawnData";
		private const string MENU_NAME = "Scriptable Objects/ScriptableEnemySpawnData";

		// positionHandler
		// prefabHandler
		// timingHandler
		// statusHandler

		public EnemySpawnData Next()
		{
			return default;
		}
	} 
}