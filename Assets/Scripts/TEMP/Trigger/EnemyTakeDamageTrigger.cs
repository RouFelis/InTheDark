using System;

using UnityEngine;

namespace InTheDark.Prototypes
{
	[Serializable]
	public class DamageHandle
	{
		public EnemyPrototypePawn Target;

		public float Damage;
	}

	public abstract class EnemyTakeDamageTrigger : ScriptableObject
	{
		public abstract void OnUpdate(DamageHandle handle);
	}
}