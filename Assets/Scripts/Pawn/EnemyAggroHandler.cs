using Unity.Netcode;
using UnityEngine;

namespace InTheDark.Prototypes
{
    // �̰� �÷��̾� ã�� �˰��� ��� ��ũ��Ʈ�� �� ��?
    // ��
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