using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace InTheDark.Prototypes
{
    [Serializable]
    public class EnemyAttackPrototype : IEquatable<EnemyAttackPrototype>
    {
        public float Cooldown;
        public string Name;

		public bool Equals(EnemyAttackPrototype other)
		{
			var isCooldownEquals = Cooldown.Equals(other.Cooldown);
			var isNameEquals = Name.Equals(other.Name);

			return isCooldownEquals && isNameEquals;
		}
	}
}