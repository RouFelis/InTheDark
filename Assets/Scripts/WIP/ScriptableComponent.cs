using UnityEngine;

namespace InTheDark.Prototypes
{
	public abstract class ScriptableComponent<T> : ScriptableObject
	{
		public abstract void OnCreate(T instance);
	}
}