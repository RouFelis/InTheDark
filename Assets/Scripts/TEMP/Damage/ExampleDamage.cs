using System;

namespace InTheDark.Prototypes
{
	public class ExampleDamage : IDamage, IDisposable
	{
		private ExampleWeapon _weapon;
		private EnemyPrototypePawn _pawn;

		public ExampleDamage(ExampleWeapon weapon, EnemyPrototypePawn pawn)
		{
			_weapon = weapon;
			_pawn = pawn;
		}

		public void Update()
		{
			if (_pawn && !_pawn.IsDead)
			{
				_pawn.TakeDamage(_weapon.Damage, _weapon.HitSound);

				if (_pawn.IsDead)
				{
					// 킬수 증가...인데 아직 해당 변수가 없는 듯
					SharedData.Instance.killed.Value += 1;
				}
			}
		}

		public void Dispose()
		{
			_weapon = null;
			_pawn = null;

			GC.SuppressFinalize(this);
		}
	}
}