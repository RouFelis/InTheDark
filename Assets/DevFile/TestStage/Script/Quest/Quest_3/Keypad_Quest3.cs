using UnityEngine;

public class Keypad_Quest3 : InteractableObject_NonNet
{
    [SerializeField] Quest3 quest3;
    public int num = 0;


    public override void Interact(ulong userId, Transform interactingObjectTransform)
    {
        quest3.AddDigitServerRpc(num);
    }
}
