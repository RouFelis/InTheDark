using UnityEngine;
using NetUtil;
using Unity.Netcode;

public class ShopInteracter : InteractableObject
{
    [SerializeField] private Transform usePosTransform;
    [SerializeField] private StoreUI storeUI;
    public NetworkVariable<ulong> SelectPlayerCode = new NetworkVariable<ulong>();
    public NetworkVariable<bool> isUsed = new NetworkVariable<bool>(false);


    public override void Interact(ulong uerID , Transform interactingObjectTransform)
    {
		if (!isUsed.Value)
		{
            NetworkObject netObject = interactingObjectTransform.GetComponent<NetworkObject>();
            NetUtil.TeleportUtil.SetEveryPlayerPosServerRPC(netObject.NetworkObjectId, usePosTransform.position, new Vector3(0f, 90f, 0f));

            //ÇÃ·¹ÀÌ¾î ¿òÁ÷ÀÓ ¸ØÃç!
            playerMoveController controller = netObject.gameObject.GetComponent<playerMoveController>();
            controller.EventToggle(true);

            storeUI.initObject();

            ChangeBoolServerRpc(interactingObjectTransform.GetComponent<NetworkObject>().NetworkObjectId);

            base.Interact(uerID, interactingObjectTransform);
        }       
    }


    [ServerRpc]
    public void ChangeBoolServerRpc(ulong playerNum)
	{
        isUsed.Value = false;
        SelectPlayerCode.Value = playerNum;
    }
}
