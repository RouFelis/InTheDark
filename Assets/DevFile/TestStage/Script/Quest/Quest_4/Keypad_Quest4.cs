using UnityEngine;

public class Keypad_Quest4 : InteractableObject_NonNet
{
    [SerializeField] Quest4 quest4;
    public int num = 0;


    public override void Interact(ulong userId, Transform interactingObjectTransform)
    {
        quest4.AddDigit(num);
    }
}
