using System.Collections;
using System.Collections.Generic;
using DunGen;
using UnityEngine;
using Unity.Netcode;

public class StartTest : StartRoomSetter
{
	[SerializeField] RuntimeDungeon dungeon;

	public GameObject[] itemPrefabs;     // 스폰할 아이템 프리팹들
	public int itemCountToSpawn = 10;    // 스폰할 총 아이템 수

	public List<Transform> spawnTransforms = new List<Transform>();

    void Start()
	{
		if (IsHost)
		{
			SharedData.Instance.SetNetSeed();

			dungeon.Generator.Seed = SharedData.Instance.networkSeed.Value;
			dungeon.Generate();
            //SetEveryPlayerPos();
        }
		else
		{
			dungeon.Generator.Seed = SharedData.Instance.networkSeed.Value;
			dungeon.Generate();
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
}
