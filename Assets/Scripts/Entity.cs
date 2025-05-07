using Unity.Netcode;
using UnityEngine;

namespace InTheDark
{
    public abstract class Entity : NetworkBehaviour
    {
        [SerializeField]
        private NetworkVariable<bool> _isActive = new();

        [SerializeField]
        private NetworkVariable<bool> _isAlive = new();

        public bool IsActive
        {
            get
            {
                return _isActive.Value;
            }

            set
            {
                _isActive.Value = value;
            }
        }

        public bool IsAlive
        {
            get
            {
                return _isAlive.Value;
            }

            set
            {
                _isAlive.Value = value;
            }
        }
    } 
}