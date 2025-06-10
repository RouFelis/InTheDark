using Cysharp.Threading.Tasks;

using Unity.Netcode;

using UnityEngine;

namespace InTheDark.Prototypes
{
    public abstract class EnemyPawnAttackHandler : NetworkBehaviour
    {
        public abstract UniTaskVoid Attack();

        protected abstract UniTask OnAttack(IHealth target);
	}
}