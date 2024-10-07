using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SaveObjectChecker : NetworkBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (IsHost)
		{

		}
		Debug.Log("Enter");
	}
}
