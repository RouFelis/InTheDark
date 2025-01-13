using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = FILE_NAME, menuName = MENU_NAME)]
public class StageData : ScriptableObject
{
	private const string FILE_NAME = "New Stage Data";
	private const string MENU_NAME = "Scriptable Objects/Stage";

	[Rpc(SendTo.Server)]
	private void OnSpawnServerRPC()
	{

	}

	[Rpc(SendTo.Everyone)]
	private void OnSpawnClientRPC()
	{

	}
}
