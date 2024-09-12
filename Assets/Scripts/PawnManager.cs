using InTheDark;

using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;

namespace InTheDark.Prototypes
{
	public class PawnManager : NetworkBehaviour
	{
		private void OnEnable()
		{
			UpdateManager.OnUpdate += OnUpdate;
		}

		private void OnDisable()
		{
			UpdateManager.OnUpdate -= OnUpdate;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
		}

		private void OnUpdate()
		{

		}

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
		}

		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();
		}
	}
}