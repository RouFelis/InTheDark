using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace InTheDark.Prototypes
{
	public abstract class Singleton<TSingleton> where TSingleton : Singleton<TSingleton>, new()
    {
		private static TSingleton _instance;

		public static TSingleton Instance
		{
			get
			{
				return _instance;
			}
		}

		static Singleton()
		{
			var instance = new TSingleton();

			_instance = instance;
		}
	} 
}
