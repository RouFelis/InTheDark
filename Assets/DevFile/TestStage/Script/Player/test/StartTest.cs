using System.Collections;
using System.Collections.Generic;
using DunGen;
using UnityEngine;
using Unity.Netcode;

public class StartTest : StartRoomSetter
{
	[SerializeField] RuntimeDungeon dungeon;
	[SerializeField] private DungeonArchetype Archetype;


	public GameObject[] itemPrefabs;     // ������ ������ �����յ�
	public int itemCountToSpawn = 10;    // ������ �� ������ ��

	public List<Transform> spawnTransforms = new List<Transform>();

    void Start()
	{  
		if (SharedData.Instance.area.Value == 0)
		{
			Archetype.BranchCount.Max = 4;
			Archetype.BranchCount.Min = 2;

			Debug.Log("�귣ġ ���� ");
		}
		else
		{
			int round = SharedData.Instance.area.Value;
			int baseCount = (int)Mathf.Floor(2 + Mathf.Log(round + 1, 2)); // log��(����+1)

			Archetype.BranchCount.Min = baseCount;
			Archetype.BranchCount.Max = baseCount + 2;

			Debug.Log("�귣ġ ���� ");
		}
		
		if (IsClient&&!IsServer)
		{
			SharedData.Instance.networkSeed.OnValueChanged += ClientDungenGenerate;
			//�⺻ 1ȸ ����.
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

		// �߰�?
		// 2024.12.26 ���� ���� �̺�Ʈ ���ġ
		// 2025.02.10 �ѹ�?
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
			Debug.Log($"���� ���� ����. �� �õ� {SharedData.Instance.networkSeed.Value}");

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
