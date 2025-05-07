using UnityEngine;

namespace InTheDark.Prototypes
{
	public abstract class ScriptableFactoryComponent<T> : ScriptableObject
	{
		public abstract void OnBuild(T instance);
	} 
}