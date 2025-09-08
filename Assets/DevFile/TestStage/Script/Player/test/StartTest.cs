using System.Collections;
using System.Collections.Generic;
using DunGen;
using UnityEngine;
using Unity.Netcode;

public class StartTest : StartRoomSetter
{
	[SerializeField] RuntimeDungeon dungeon;
	[SerializeField] private DungeonArchetype Archetype;


	public GameObject[] itemPrefabs;     // 스폰할 아이템 프리팹들
	public int itemCountToSpawn = 10;    // 스폰할 총 아이템 수

	public List<Transform> spawnTransforms = new List<Transform>();

    void Start()
	{  
		if (SharedData.Instance.area.Value == 0)
		{
			Archetype.BranchCount.Max = 4;
			Archetype.BranchCount.Min = 2;

			Debug.Log("브랜치 설정 ");
		}
		else
		{
			int round = SharedData.Instance.area.Value;
			int baseCount = (int)Mathf.Floor(2 + Mathf.Log(round + 1, 2)); // log₂(라운드+1)

			Archetype.BranchCount.Min = baseCount;
			Archetype.BranchCount.Max = baseCount + 2;

			Debug.Log("브랜치 설정 ");
		}
		
		if (IsClient&&!IsServer)
		{
			SharedData.Instance.networkSeed.OnValueChanged += ClientDungenGenerate;
			//기본 1회 생성.
			dungeon.Generator.Seed = SharedData.Instance.networkSeed.Value;
			dungeon.Generate();
		}

		if (IsHost)
		{
			SharedData.Instance.SetNetSeedServerRpc();

			dungeon.Generator.Seed = SharedData.Instance.networkSeed.Value;
			dungeon.Generator.OnGenerationStatusChanged += GenerateFail;
			dungeon.Generate();
            //SetEveryPlayerPos();
        }

		// 추가?
		// 2024.12.26 던전 입장 이벤트 재배치
		// 2025.02.10 롤백?
		using var command = new InTheDark.Prototypes.Enter()
		{
			BuildIndex = 0
		};

		command.Invoke();
	}

	private void ClientDungenGenerate(int oldValue, int newValue)
	{
		dungeon.Generator.Seed = newValue;
		dungeon.Generate();
	}

	private void GenerateFail(DungeonGenerator generator, GenerationStatus status)
	{
		if (status == GenerationStatus.Failed)
		{
			SharedData.Instance.SetNetSeedServerRpc();

			dungeon.Generator.Seed = SharedData.Instance.networkSeed.Value;
			Debug.Log($"던전 생성 실패. 새 시드 {SharedData.Instance.networkSeed.Value}");

			dungeon.Generate();
		}
	}

	private void OnDisable()
	{
		if (!IsHost)
		{
			SharedData.Instance.networkSeed.OnValueChanged -= ClientDungenGenerate;
		}
	}
}
