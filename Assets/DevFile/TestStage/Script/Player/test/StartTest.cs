using System.Collections;
using System.Collections.Generic;
using DunGen;
using DunGen.Graph;
using UnityEngine;
using Unity.Netcode;

public class StartTest : StartRoomSetter
{
	[SerializeField] RuntimeDungeon dungeon;
	[SerializeField] private DungeonArchetype Archetype;
	[SerializeField] private DungeonFlow dungeonFlow;


	public GameObject[] itemPrefabs;     // 스폰할 아이템 프리팹들
	public int itemCountToSpawn = 10;    // 스폰할 총 아이템 수

	public List<Transform> spawnTransforms = new List<Transform>();

    void Start()
	{  
		if (SharedData.Instance.area.Value == 0)
		{
			Archetype.BranchCount.Max = 4;
			Archetype.BranchCount.Min = 2;
			dungeonFlow.Length.Max = 4;
			dungeonFlow.Length.Min = 2;

			Debug.Log("브랜치 설정 ");
		}
		else
		{
			//루트방식
			/*			int round = Mathf.Max(0, SharedData.Instance.area.Value);

						int value = 4 + (int)Mathf.Floor(Mathf.Sqrt(round * 4f)); // float값 조절해서 밀도 증가.*/

			int round = Mathf.Max(0, SharedData.Instance.area.Value);

			//정수방식
			int value = 2 + (int)Mathf.Floor(Mathf.Pow(round, 0.7f));

			Archetype.BranchCount.Max = value;
			Archetype.BranchCount.Min = value;
		
			dungeonFlow.Length.Max = value;
			dungeonFlow.Length.Min = value;

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
			//BuildIndex = 0
			BuildIndex = SharedData.Instance.area.Value
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
