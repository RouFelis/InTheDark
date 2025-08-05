using UnityEngine;

public class Submit_Quest3 : InteractableObject_NonNet
{
    [SerializeField] Quest3 quest3;

    public override void Interact(ulong userId, Transform interactingObjectTransform)
    {
        quest3.SubmitInputServerRpc();
    }
}
