using System;

using UnityEngine;

namespace InTheDark
{
    public abstract class BootstrapConfigs : ScriptableObject
    {

    }

    public sealed class BootstrapManager : MonoBehaviour
    {
        public static Action OnBootstrapCompleted;
	} 
}