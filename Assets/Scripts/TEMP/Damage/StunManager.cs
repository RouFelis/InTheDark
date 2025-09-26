using Unity.Netcode;

using UnityEngine;

namespace InTheDark.Prototypes
{
	public class StunManager : KeywordSubManager<IStun>
	{
		public EnemyStunHandler GetHandler(EnemyPrototypePawn pawn)
		{
			return default;
		}

		public PlayerStunHandler GetHandler(Player player)
		{
			return default;
		}
	}

	public class PlayerStunHandler
	{

	}

	public class EnemyStunHandler
	{

	}
}