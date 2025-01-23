using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using System;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services;
using Unity.Services.Relay.Models;
using InTheDark.Prototypes;



public class TestRelay : MonoBehaviour
{
	public static TestRelay Instance { get; set; }

	[SerializeField]
	private string enviromnet = "production";

	[SerializeField]
	private int maxConnections = 10;

	private void Awake()
	{
		Instance = this;
	}

	public bool IsRelayEnabled => Transport != null &&
		Transport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport;

	public UnityTransport Transport => NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();

	public async Task<RelayHostData> SetupRelay()
	{
		Debug.Log($"Relay Server Starting With max connetcions {maxConnections}");
		InitializationOptions options = new InitializationOptions().SetEnvironmentName(enviromnet);

		await UnityServices.InitializeAsync(options);

		if (!AuthenticationService.Instance.IsSignedIn)
		{
			await AuthenticationService.Instance.SignInAnonymouslyAsync();
		}

		Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);

		RelayHostData relayHostData = new RelayHostData
		{
			Key = allocation.Key,
			Port = (ushort)allocation.RelayServer.Port,
			AllocationID = allocation.AllocationId,
			AllocationIDBytes = allocation.AllocationIdBytes,
			IPv4Address = allocation.RelayServer.IpV4,
			ConnectionData = allocation.ConnectionData
		};

		relayHostData.JoinCode = await RelayService.Instance.GetJoinCodeAsync(relayHostData.AllocationID);

		Transport.SetRelayServerData(relayHostData.IPv4Address, relayHostData.Port, relayHostData.AllocationIDBytes, relayHostData.Key, relayHostData.ConnectionData) ;

		Logger.Instance?.LogInfo($"Relay Server generated a join code {relayHostData.JoinCode}");

		// 250121 -> 코드 긁어오기 용
		using var command = new RelayAction()
		{
			JoinCode = relayHostData.JoinCode
		};

		command.Invoke();

		return relayHostData;
	}
		
	public async Task<RelayJoinData> JoinRelay(string joinCode)
	{
		InitializationOptions options = new InitializationOptions().SetEnvironmentName(enviromnet);

		await UnityServices.InitializeAsync(options);

		if (!AuthenticationService.Instance.IsSignedIn)
		{
			await AuthenticationService.Instance.SignInAnonymouslyAsync();
		}

		JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

		RelayJoinData relayJoinData = new RelayJoinData
		{
			Key = allocation.Key,
			Port = (ushort)allocation.RelayServer.Port,
			AllocationID = allocation.AllocationId,
			AllocationIDBytes = allocation.AllocationIdBytes,
			ConnectionData = allocation.ConnectionData,
			HostConnectionData = allocation.HostConnectionData,
			IPv4Address = allocation.RelayServer.IpV4,
			JoinCode = joinCode
		};

		Transport.SetRelayServerData(relayJoinData.IPv4Address , relayJoinData.Port, relayJoinData.AllocationIDBytes, relayJoinData.Key, relayJoinData.ConnectionData, relayJoinData.HostConnectionData);

		Logger.Instance?.LogInfo($"Relay Server generated a join code {joinCode}");

		return relayJoinData;
	}
}
