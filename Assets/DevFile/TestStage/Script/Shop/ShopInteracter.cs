using UnityEngine;
using NetUtil;
using Unity.Netcode;

public class ShopInteracter : InteractableObject
{
    [SerializeField] private Transform usePosTransform;
    [SerializeField] private StoreUI storeUI;
    public NetworkVariable<ulong> SelectPlayerCode = new NetworkVariable<ulong>();
    public NetworkVariable<bool> isUsed = new NetworkVariable<bool>(false);


    public override bool Interact(ulong uerID , Transform interactingObjectTransform)
    {
		if (!isUsed.Value)
		{
			if (!base.Interact(uerID, interactingObjectTransform))
				return false;

			NetworkObject netObject = interactingObjectTransform.GetComponent<NetworkObject>();
			NetUtil.TeleportUtil.SetEveryPlayerPosServerRPC(netObject.NetworkObjectId, usePosTransform.position, new Vector3(0f, 90f, 0f));

			//ÇÃ·¹ÀÌ¾î ¿òÁ÷ÀÓ ¸ØÃç!
			playerMoveController controller = netObject.gameObject.GetComponent<playerMoveController>();
			controller.EventToggle(true, this.gameObject);

			storeUI.initObject();

			ChangePowerServerRpc(interactingObjectTransform.GetComponent<NetworkObject>().NetworkObjectId, true);

			MenuManager.Instance.IsEvenet = true;

			return true;
		}
		return false;
	}

/*    ulong test;
	private void Update()
	{
		if (isUsed.Value)
		{
            NetUtil.TeleportUtil.SetEveryPlayerPosServerRPC(test, usePosTransform.position, new Vector3(0f, 90f, 0f));
        }
	}*/

	[ServerRpc(RequireOwnership = false)]
    public void ChangePowerServerRpc(ulong playerNum , bool isPower)
	{        
        isUsed.Value = isPower;
        SelectPlayerCode.Value = playerNum;
    }
}
