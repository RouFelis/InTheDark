using System.Collections;
using System.Collections.Generic;
using DunGen;
using UnityEngine;
using Unity.Netcode;

public class StartTest : StartRoomSetter
{
    [SerializeField] RuntimeDungeon dungeon;
  
    void Start()
    {
        if (IsHost)
		{
            SharedData.Instance.SetNetSeed();

            dungeon.Generator.Seed = SharedData.Instance.networkSeed.Value;
            dungeon.Generate();
           // SetEveryPlayerPos();
        }
        else
		{
            dungeon.Generator.Seed = SharedData.Instance.networkSeed.Value;
            dungeon.Generate();
        }

		// 추가?
		// 2024.12.26 던전 입장 이벤트 재배치
		//using var command = new InTheDark.Prototypes.Enter()
		//{
		//    BuildIndex = 0
		//};

		//command.Invoke();
	}
}
