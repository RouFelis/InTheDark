using UnityEngine;

public class Submit_Quest4 : InteractableObject_NonNet
{
    [SerializeField] Quest4 quest4;

    public override void Interact(ulong userId, Transform interactingObjectTransform)
    {
        quest4.SubmitInput();
    }
}
