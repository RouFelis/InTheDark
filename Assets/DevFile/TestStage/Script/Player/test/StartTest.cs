using DunGen;
using UnityEngine;
using Unity.Netcode;

public class StartTest : NetworkBehaviour
{
    [SerializeField] RuntimeDungeon dungeon;
    private NetworkVariable<int> networkSeed = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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
        }
		else
		{
            dungeon.Generator.Seed = networkSeed.Value;
            dungeon.Generate();
        }
    }

    public void Init()
	{
        networkSeed.Dispose();
    }    

}
