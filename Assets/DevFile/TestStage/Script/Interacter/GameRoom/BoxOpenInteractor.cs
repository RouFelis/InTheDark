using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class BoxOpenInteractor : InteractableObject_NonNet
{
	[SerializeField] private BoxControllerBase BoxControllers;


	public override void Interact(ulong userId, Transform interactingObjectTransform)
	{
		base.Interact(userId, interactingObjectTransform);

		BoxControllers.StartAnimation();
	}
}
