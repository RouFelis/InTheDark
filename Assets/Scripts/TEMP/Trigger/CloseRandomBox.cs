using UnityEngine;

namespace InTheDark.Prototypes
{
	[CreateAssetMenu]
	public class CloseRandomBox : EnemyTakeDamageTrigger
	{
		public override void OnUpdate(DamageHandle handle)
		{
			var randomBox = handle.Target.GetComponent<EnemyRandomBox>();

			if (randomBox && handle.Damage > 0.0F)
			{
				randomBox.ChangeEntityIdentityServerRPC(true);
			}
		}
	}
}