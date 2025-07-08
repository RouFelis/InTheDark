using System.Collections;
using System.Collections.Generic;
using DunGen;
using UnityEngine;
using Unity.Netcode;

public class StartTest : StartRoomSetter
{
	[SerializeField] RuntimeDungeon dungeon;

	public GameObject[] itemPrefabs;     // ������ ������ �����յ�
	public int itemCountToSpawn = 10;    // ������ �� ������ ��

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

		// �߰�?
		// 2024.12.26 ���� ���� �̺�Ʈ ���ġ
		// 2025.02.10 �ѹ�?
		using var command = new InTheDark.Prototypes.Enter()
		{
			BuildIndex = 0
		};

		command.Invoke();
	}
}
