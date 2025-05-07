using System;
using Unity.Netcode;
using UnityEngine;

namespace InTheDark.Prototypes
{
	[CreateAssetMenu(fileName = "new resurrect", menuName = "trigger/death/resurrect")]
	public class ResurrectOnDeath : EnemyDeathTrigger
	{
		[SerializeField]
		private float _time;

		[SerializeField]
		private ResurrectOnDeathHandler _handlerPrefab;

		private ResurrectOnDeathHandler _handler;

		public override void OnUpdate(EnemyPrototypePawn pawn)
		{
			SendingResurrectableEnemyRPC(pawn);
		}

		[Rpc(SendTo.Server)]
		private void SendingResurrectableEnemyRPC(NetworkBehaviourReference reference)
		{
			GetHandler().StartEnemyResurrectRPC(reference, _time);
		}

		private ResurrectOnDeathHandler GetHandler()
		{
			if (!_handler)
			{
				_handler = FindAnyObjectByType<ResurrectOnDeathHandler>();
			}

			if (!_handler)
			{
				_handler = Instantiate(_handlerPrefab);

				_handler.NetworkObject.Spawn();

				DontDestroyOnLoad(_handler.gameObject);
			}

			return _handler;
		}
	}
}