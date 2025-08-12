using BehaviorDesigner.Runtime;

using Cysharp.Threading.Tasks;

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
			Stun.Run();
		}
	}

	public class Stun
	{
		public static void Run()
		{

		}
	}
}