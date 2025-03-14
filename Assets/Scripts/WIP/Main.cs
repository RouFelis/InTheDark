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


		private void Awake()
		{
			OnAwake();

			//SpotLight light = default;

			//light.SetCauser(FindAnyObjectByType<Player>())
			//	.SetAngle(25.0F)
			//	.SetRange(16.0F)
			//	.SetDamage(9.0F)
			//	.Tick();
		}

		private void OnAwake()
		{
			//// 1
			//var observer = NC0566.GetBuilder<int>()
			//	.OnCompleted(() => Debug.Log("Completed."))
			//	.OnNext(x => Debug.Log(x))
			//	.Build();

			//var observable = new NC3066<int>();

			//observable.Subscribe(observer);

			//new List<int>()
			//	.Select(x => x.ToString());

			//// 2
			//observable
			//	.Where(x => x > 10)
			//	.Select(x => x.ToString())
			//	.Repeat(2)
			//	.Distinct()
			//	.Subscribe(NC0566.GetBuilder<string>()
			//	.OnCompleted(() => Debug.Log("Completed."))
			//	.OnNext(x => Debug.Log(x))
			//	.Build());

			//// 3
			//observable
			//	.Where(x => x > 10)
			//	.Select(x => x.ToString())
			//	.Repeat(2)
			//	.Distinct()
			//	.Subscribe(
			//	(x) => Debug.Log(x),
			//	() => Debug.Log("Completed."));
		}
	}
}