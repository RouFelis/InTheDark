using BehaviorDesigner.Runtime;
using InTheDark.Prototypes;
using System;
using Unity.Netcode;

[Serializable]
public class SharedNetworkPawn : SharedVariable<NetworkPawn>
{
	public static implicit operator SharedNetworkPawn(NetworkPawn value)
	{
		var sharedVariable = new SharedNetworkPawn()
		{
			Value = value
		};

		return sharedVariable;
	}
}

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