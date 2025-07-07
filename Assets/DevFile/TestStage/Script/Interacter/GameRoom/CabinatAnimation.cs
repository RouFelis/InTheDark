using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class CabinatAnimation : BoxControllerBase
{
	[Header("Door Settings")]
	[SerializeField] private Transform leftDoorAxis;
	[SerializeField] private float doorOpenAngle = 90f;
	[SerializeField] private float doorAnimationSpeed = 2f;
	[SerializeField] private AudioSource doorSound;
	[SerializeField] private MeshCollider doorCollider;

	private Coroutine doorAnimationCoroutine;
	private NetworkVariable<bool> doorState = new NetworkVariable<bool>(false); // false: closed, true: open


	public override void StartAnimation()
	{
		DoorToggleServerRpc();
	}


	[ServerRpc(RequireOwnership = false)]
	private void DoorToggleServerRpc()
	{
		// 문 연출 처리
		//doorCollider.enabled = false;
		DoorToggleClientRpc();

		if (doorAnimationCoroutine == null)
		{
			doorAnimationCoroutine = StartCoroutine(AnimateDoorsWithSound(!doorState.Value));
			doorState.Value = !doorState.Value;
		}
	}

	[ClientRpc]
	private void DoorToggleClientRpc()
	{
		//doorCollider.enabled = false;
	}

	[ServerRpc(RequireOwnership = false)]
	private void DoorToggleDoneServerRpc()
	{
		//doorCollider.enabled = true;
		DoorToggleDoneClientRpc();
	}


	[ClientRpc]
	private void DoorToggleDoneClientRpc()
	{
		//doorCollider.enabled = true;
	}


	private IEnumerator AnimateDoorsWithSound(bool open)
	{
		doorSound.Play();
		float animationDuration = doorSound.clip.length / Mathf.Max(doorAnimationSpeed, 0.01f); // 0으로 나누는 것 방지


		Quaternion leftStart = leftDoorAxis.localRotation;

		Quaternion leftTarget = open ? Quaternion.Euler(0, doorOpenAngle, 0) : Quaternion.identity;

		float elapsedTime = 0f;
		while (elapsedTime < animationDuration)
		{
			elapsedTime += Time.deltaTime;
			float t = Mathf.Clamp01(elapsedTime / animationDuration);

			leftDoorAxis.localRotation = Quaternion.Slerp(leftStart, leftTarget, t);
			yield return null;
		}

		leftDoorAxis.localRotation = leftTarget;

		doorAnimationCoroutine = null;

		DoorToggleDoneServerRpc();
	}
}
