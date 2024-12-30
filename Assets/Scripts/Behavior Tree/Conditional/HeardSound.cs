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
/*
	public SharedGameObject targetObject;  // 타겟 객체
	public LayerMask soundLayer = 3;          // 소리가 감지될 레이어
	public float hearingRadius = 10.0f;   // 청취 반경

	public override TaskStatus OnUpdate()
	{
		if (IsObjectWithinHearingRange())
		{
			return TaskStatus.Success;
		}
		return TaskStatus.Failure;
	}

	private bool IsObjectWithinHearingRange()
	{
		Debug.Log("1");
		if (targetObject.Value == null)
		{
			Debug.Log("3");
			return false;
		}
		Debug.Log("2");
		float distance = Vector3.Distance(transform.position, targetObject.Value.transform.position);
		if (distance <= hearingRadius)
		{
			// 소리가 들리는 객체가 반경 내에 있는지 확인
			RaycastHit hit;
			Vector3 direction = (targetObject.Value.transform.position - transform.position).normalized;

			

			if (Physics.Raycast(transform.position, direction, out hit, hearingRadius))
			{
				Debug.Log("소리 테스트 " + hit.rigidbody.gameObject.layer);

				return hit.collider.gameObject == targetObject.Value;
			}
		}
		return false;
	}*/
}
