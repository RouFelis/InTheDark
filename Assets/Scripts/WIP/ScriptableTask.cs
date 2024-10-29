using Cysharp.Threading.Tasks;

using UnityEngine;

namespace InTheDark
{
    public abstract class ScriptableTask : ScriptableObject
    {
        public abstract UniTask<bool> OnNext(int index);
    }
}