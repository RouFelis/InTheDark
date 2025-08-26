using BehaviorDesigner.Runtime;

using Cysharp.Threading.Tasks;
using InTheDark.Example.Keywords;
using InTheDark.LoremIpsum;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Transactions;

using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Services.Core;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace InTheDark.Prototypes
{
	public sealed class Main : MonoBehaviour
	{
		private void Awake()
		{
			OnAwake();
		}

		private void OnAwake()
		{
			//VFXSpawnerCallbacks callbacks = default;
		}
	}

	#region PETIT_CODE

	public interface IPetitHealth
	{

	}

	[Serializable]
	public class PetitHealth : IPetitHealth
	{

	}

	[Serializable]
	public class PetitPlayer
	{
		
	}

	[Serializable]
	public class PetitEnemy
	{
		
	}

	#endregion
}