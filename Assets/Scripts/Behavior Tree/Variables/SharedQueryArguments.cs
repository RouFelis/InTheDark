using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SharedQueryArguments : SharedVariable<Dictionary<string, Transform>>
{
	public static implicit operator SharedQueryArguments(Dictionary<string, Transform> value) { return new SharedQueryArguments { mValue = value }; }
}