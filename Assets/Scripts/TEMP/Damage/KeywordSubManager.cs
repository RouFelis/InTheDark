using Unity.Netcode;

using UnityEngine;

namespace InTheDark.Prototypes
{
    public abstract class KeywordSubManager<TKeyword> : NetworkBehaviour where TKeyword : IKeyword
	{

    } 
}