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
            // �õ带 ���� �ð����� �����Ͽ� �Ź� �ٸ� ������ ����
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

        // �߰�?
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
