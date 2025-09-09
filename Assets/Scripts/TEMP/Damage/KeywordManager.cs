using Unity.Netcode;

using UnityEngine;

namespace InTheDark.Prototypes
{
    public sealed class KeywordManager : NetworkBehaviour
    {
        public static KeywordSubManager<TKeyword> GetKeywordSubManager<TKeyword>() where TKeyword : IKeyword
        {
            var instance = FindFirstObjectByType<KeywordManager>();
            var manager = instance?.GetComponentInChildren<KeywordSubManager<TKeyword>>();

            return manager;
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}