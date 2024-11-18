using UnityEngine;
using NetUtil;
using Unity.Netcode;

public class ShopInteracter : InteractableObject
{
    [SerializeField] Transform usePosTransform;
    [SerializeField] StoreUI storeUI;

    public override void Interact(Transform interactingObjectTransform)
    {
        NetworkObject netObject = interactingObjectTransform.GetComponent<NetworkObject>();
        NetUtil.TeleportUtil.SetEveryPlayerPosServerRPC(netObject.NetworkObjectId , usePosTransform.position , new Vector3(0f,90f,0f));

        //�÷��̾� ������ ����!
        playerMoveController controller = netObject.gameObject.GetComponent<playerMoveController>();
        controller.enabled = false;
        Cursor.lockState = CursorLockMode.Confined;// ���콺 ����

        storeUI.initObject();

        base.Interact(interactingObjectTransform);
    }


}
