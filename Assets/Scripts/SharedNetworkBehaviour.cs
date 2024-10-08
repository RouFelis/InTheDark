using BehaviorDesigner.Runtime;
using System;
using Unity.Netcode;

[Serializable]
public class SharedNetworkBehaviour : SharedVariable<NetworkBehaviour>
{
	public static implicit operator SharedNetworkBehaviour(NetworkBehaviour value)
	{
		var sharedVariable = new SharedNetworkBehaviour()
		{
			Value = value
		};

		return sharedVariable;
	}
}