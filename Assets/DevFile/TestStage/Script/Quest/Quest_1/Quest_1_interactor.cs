using UnityEngine;

public class Quest_1_interactor : InteractableObject
{
    [SerializeField] private Quest1 quest1;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        base.Start();
    }

    public override void Interact(ulong userId, Transform interactingObjectTransform)
    {
        base.Interact(userId, interactingObjectTransform);


        quest1.CheckQuestServerRpc();
    }

}
