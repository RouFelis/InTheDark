using Unity.Netcode;
using UnityEngine;

namespace InTheDark.Prototypes
{
    public class AttackManager : NetworkBehaviour
	{
		private static AttackManager _instance;

		public static AttackManager Instance
		{
			get
			{
				return _instance;
			}
		}

		static AttackManager()
		{
			_instance = default;
		}

		private void Awake()
		{
			_instance = this;

			DontDestroyOnLoad(gameObject);
		}
	}
}