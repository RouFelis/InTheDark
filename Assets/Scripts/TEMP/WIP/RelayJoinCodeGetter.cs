using UnityEngine;
using UnityEngine.Rendering;

namespace InTheDark.Prototypes
{
    public class RelayJoinCodeGetter : MonoBehaviour
    {
        [SerializeField]
        private string _joinCode;

		private void Awake()
		{
			Game.OnRelay += OnRelayJoin;

			DontDestroyOnLoad(gameObject);
		}

		private void OnDestroy()
		{
			Game.OnRelay -= OnRelayJoin;
		}

		private void OnRelayJoin(RelayEvent received)
		{
			_joinCode = received.JoinCode;

			GUIUtility.systemCopyBuffer = received.JoinCode;
		}
	} 
}