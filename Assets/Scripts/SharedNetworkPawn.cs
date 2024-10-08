using BehaviorDesigner.Runtime;
using InTheDark.Prototypes;
using System;

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