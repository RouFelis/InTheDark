using Unity.Netcode;
using UnityEngine;

namespace InTheDark.Prototypes
{
    // �̰� �÷��̾� ã�� �˰��� ��� ��ũ��Ʈ�� �� ��?
    // ��
    public class EnemyAggroHandler : NetworkBehaviour
    {
		[SerializeField]
		private EnemyPrototypePawn _pawn;

		//private NetworkVariable<NetworkBehaviourReference> _target = new NetworkVariable<NetworkBehaviourReference>();

   //     public Player Target
   //     {
   //         get
   //         {
   //             var isEnable = _target.Value.TryGet(out Player player);

			//	return player;
   //         }

   //         set
   //         {
   //             if (IsServer)
   //             {
			//		_target.Value = value;
			//	}
			//}
   //     }
	} 
}