using UnityEngine;

namespace InTheDark.Prototypes
{
	[CreateAssetMenu]
	public class EnemyDamageNormalizer : EnemyTakeDamageTrigger
	{
		public float MaxDamage;

		public override void OnUpdate(DamageHandle handle)
		{
			handle.Damage = Mathf.Min(MaxDamage, handle.Damage);
		}
	}
}