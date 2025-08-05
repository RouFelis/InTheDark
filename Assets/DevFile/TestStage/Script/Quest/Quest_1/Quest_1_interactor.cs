using UnityEngine;

public class Quest_1_interactor : InteractableObject
{
    [SerializeField] private Quest1 quest1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        base.Start();
    }

    public override bool Interact(ulong userId, Transform interactingObjectTransform)
    {
		if (!base.Interact(userId, interactingObjectTransform))
			return false;

        quest1.CheckQuestServerRpc();
        
        return true;
    }

}
