using Unity.Netcode;
using UnityEngine;

namespace InTheDark.Prototypes
{
    // 이게 플레이어 찾기 알고리즘 담당 스크립트가 될 듯?
    // 아
    public class EnemyAggroHandler : NetworkBehaviour
    {
		private NetworkVariable<NetworkBehaviourReference> _target = new NetworkVariable<NetworkBehaviourReference>();

        public Player Target
        {
            get
            {
                var isEnable = _target.Value.TryGet(out Player player);

				return player;
            }

            set
            {
				_target.Value = value;
			}
        }
	} 
}