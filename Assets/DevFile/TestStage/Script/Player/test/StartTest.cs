using System.Collections;
using System.Collections.Generic;
using DunGen;
using UnityEngine;
using Unity.Netcode;

public class StartTest : StartRoomSetter
{
    [SerializeField] RuntimeDungeon dungeon;
    private NetworkVariable<int> networkSeed = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    public override void OnDestroy()
    {
        networkSeed.Dispose();
        base.OnDestroy();
    } 

    void Start()
    {
        if (IsHost)
		{
            // 시드를 현재 시간으로 설정하여 매번 다른 난수를 생성
            int seed = (int)System.DateTime.Now.Ticks;
            Random.InitState(seed);

            networkSeed.Value = seed;

            dungeon.Generator.Seed = seed;
            dungeon.Generate();
            SetEveryPlayerPos();
        }
        else
		{
            dungeon.Generator.Seed = networkSeed.Value;
            dungeon.Generate();
        }

        // 추가?
        using var command = new InTheDark.Prototypes.Enter()
        {
            BuildIndex = 0
        };

        command.Invoke();
    }

    public void Init()
	{
        networkSeed.Dispose();
    }    

}
