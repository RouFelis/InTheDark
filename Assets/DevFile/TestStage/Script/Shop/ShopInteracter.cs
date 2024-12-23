using UnityEngine;
using NetUtil;
using Unity.Netcode;

public class ShopInteracter : InteractableObject
{
    [SerializeField] Transform usePosTransform;
    [SerializeField] StoreUI storeUI;

    public override void Interact(ulong uerID , Transform interactingObjectTransform)
    {
        NetworkObject netObject = interactingObjectTransform.GetComponent<NetworkObject>();
        NetUtil.TeleportUtil.SetEveryPlayerPosServerRPC(netObject.NetworkObjectId , usePosTransform.position , new Vector3(0f,90f,0f));

        //ÇÃ·¹ÀÌ¾î ¿òÁ÷ÀÓ ¸ØÃç!
        playerMoveController controller = netObject.gameObject.GetComponent<playerMoveController>();
        controller.EventToggle(true);

        storeUI.initObject();

        base.Interact(uerID , interactingObjectTransform);
    }


}
