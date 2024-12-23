using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

public class HeardSound : Conditional
{
	public float distance;
	// The LayerMask of the targets
	public LayerMask targetLayer;

	// Set the target variable when a target has been found so the subsequent tasks know which object is the target
	public SharedVector3 target;

	public SharedNetworkBehaviour pawn;

	private int size;

	private Collider[] colliders = new Collider[16];

	public override TaskStatus OnUpdate()
	{
		for (var i = 0; i < size; i++)
		{
			colliders[i] = default;
		}

		size = Physics.OverlapSphereNonAlloc(transform.position, distance, colliders, targetLayer);

		for (var i = 0; i < size; i++)
		{
			var element = colliders[i];
			var ddd = element.GetComponent<AudioSource>();

			if (ddd && ddd.isPlaying)
			{
				NavMesh.SamplePosition(element.transform.position, out var destination, 5.0f, NavMesh.AllAreas);

				// Set the target so other tasks will know which transform is within sight
				//target.Value = element.transform.position;
				target.Value = destination.position;
				pawn.Value = element.GetComponent<NetworkBehaviour>();

				Debug.Log($"{pawn}... I Heard You...");

				return TaskStatus.Success;
			}
		}

		pawn.Value = default;

		return TaskStatus.Failure;
	}
}
