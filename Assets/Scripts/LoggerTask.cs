using Cysharp.Threading.Tasks;

using System;

using UnityEngine;

namespace InTheDark
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = MENU_NAME)]
    public class LoggerTask : ScriptableTask
    {
        private const string FILE_NAME = "Logger";
        private const string MENU_NAME = "Scriptable Tasks/Example/Logger";
        
        [Serializable]
        private struct __Model
        {
            public string Message;
        }

        [SerializeField]
        private __Model[] _models;
        
        public override async UniTask<bool> OnNext(int index)
        {
            await UniTask.Yield();
                
            Debug.Log(_models[index].Message);

            return true;
        }
    }
}