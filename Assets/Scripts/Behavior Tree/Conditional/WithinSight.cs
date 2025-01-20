using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using InTheDark.Prototypes;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

public class WithinSight : Conditional
{
    // public int maxPlayerCount;
    
    //public float distance;;

	public float sightDistance;
	public float aroundDistance;

    public float fieldOfViewAngle;
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

		size = Physics.OverlapSphereNonAlloc(transform.position, sightDistance, colliders, targetLayer);

		for (var i = 0; i < size; i++)
		{
			var element = colliders[i];

			var direction = element.transform.position - transform.position;
			var isSight = Vector3.Angle(direction, transform.forward) < fieldOfViewAngle;
			var distance = isSight ? sightDistance : aroundDistance;
			var isOccultation = Physics.Raycast(transform.position, direction, out var hit, distance);

			OnDrawRaycastGizmo(element, hit, direction/*, isSight*/);

			if (hit.collider == element && isOccultation/* && isSight*/)
			{
				NavMesh.SamplePosition(element.transform.position, out var destination, 5.0f, NavMesh.AllAreas);

				// Set the target so other tasks will know which transform is within sight
				//target.Value = element.transform.position;
				target.Value = destination.position;
				pawn.Value = element.GetComponent<NetworkBehaviour>();

				return TaskStatus.Success;
			}
		}

		pawn.Value = default;

		return TaskStatus.Failure;
    }

	//public override void OnDrawGizmos()
	//{
	//	if (pawn.Value)
	//	{

	//	}
	//}

	[System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnDrawRaycastGizmo(Collider collider, RaycastHit hit, Vector3 direction/*, bool isSight*/)
    {
        var color = hit.collider == collider/* && isSight*/ ? Color.blue : Color.red;
            
        Debug.DrawRay(transform.position, direction, color);
    }
}