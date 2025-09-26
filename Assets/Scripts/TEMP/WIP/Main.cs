using BehaviorDesigner.Runtime;

using Cysharp.Threading.Tasks;
using InTheDark.Example.Keywords;
using InTheDark.LoremIpsum;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
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

			//IObserver<IA> observerA = null;
			//IObserver<IB> observerB = null;
			//IObserver<IC> observerC = null;

			//IObservable<IA> observableA = null;
			//IObservable<IB> observableB = null;
			//IObservable<IC> observableC = null;

			//observableA.Subscribe(observerA);
			//observableA.Subscribe(observerB);
			//observableA.Subscribe(observerC);

			//observableB.Subscribe(observerA);
			//observableB.Subscribe(observerB);
			//observableB.Subscribe(observerC);

			//observableC.Subscribe(observerA);
			//observableC.Subscribe(observerB);
			//observableC.Subscribe(observerC);

			//Debug.Log(Math.Log(243, 3));
		}
	}

	public interface IA
	{

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