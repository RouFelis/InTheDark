using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using QuickCmd;
using Unity.Netcode;

public class TestRelay : MonoBehaviour
{
    private async void Start()
	{
		await UnityServices.InitializeAsync();

		AuthenticationService.Instance.SignedIn += () =>
		{
			Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
		};
		await AuthenticationService.Instance.SignInAnonymouslyAsync();
	}
    
	[Command]
	private async void CreateRelay()
	{
		try
		{
			Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

			string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

			Debug.Log(joinCode);

			//NetworkManager.Instantiate.GetComponent<unitytansport>
		}
		catch (RelayServiceException e)
		{
			Debug.Log(e);
		}
	}

	[Command]
	private async void JoinRelay(string joinCode)
	{
		try
		{
			Debug.Log("joining Relay with " + joinCode);
			await RelayService.Instance.JoinAllocationAsync(joinCode);
		} 
		catch (RelayServiceException e)
		{
			Debug.Log(e);
		}

	}

}
