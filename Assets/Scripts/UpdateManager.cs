using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace InTheDark.Prototypes
{
	// [========================= UpdateManager =========================]

	public class UpdateManager : MonoBehaviour
	{
		// [========================= Delegate =========================]

		public delegate void UpdateDelegate();
		public delegate void FixedUpdateDelegate();
		public delegate void LateUpdateDelegate();

		// [========================= Constant =========================]

		public const string GAMEOBJECT_NAME = "Update Manager";

		// [========================= Field =========================]

		public static event UpdateDelegate OnUpdate;
		public static event FixedUpdateDelegate OnFixedUpdate;
		public static event LateUpdateDelegate OnLateUpdate;

		// [========================= Method =========================]

		static UpdateManager()
		{
			var gameObject = new GameObject(GAMEOBJECT_NAME);

			gameObject.AddComponent<UpdateManager>();

			OnUpdate = null;
			OnFixedUpdate = null;
			OnLateUpdate = null;

			DontDestroyOnLoad(gameObject);
		}

		private void Update()
		{
			OnUpdate?.Invoke();
		}

		private void FixedUpdate()
		{
			OnFixedUpdate?.Invoke();
		}

		private void LateUpdate()
		{
			OnLateUpdate?.Invoke();
		}
	}
}
