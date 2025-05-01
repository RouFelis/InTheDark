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
using Unity.Services.Core;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.Events;

namespace InTheDark.Prototypes
{
	public sealed class Main : MonoBehaviour
	{
		[SerializeField]
		private NetworkVariable<int> _dsmd;

		private void Awake()
		{
			OnAwake();

			_dsmd = new();
		}

		private void OnAwake()
		{

		}
	}
}