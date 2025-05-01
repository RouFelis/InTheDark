using System.Collections.Generic;
using UnityEngine;
using DunGen;

[System.Serializable]
public class InjectTileData
{
	public TileSet tileSet;
	public bool isOnMainPath = false;
	public bool isRequired = false;

	public float pathDepth = 0.5f;
	public float branchDepth = 1f;
}

[System.Serializable]
public class TileSetPair
{
    [Header("Tile A (필수)")]
    public InjectTileData tileA; // 예: 입구

    [Header("Tile B (선택)")]
    public InjectTileData tileB; // 예: 본 방
}


public class TileInjectionManager : MonoBehaviour
{
    [Header("타일셋 세트 리스트")]
    public List<TileSetPair> tileSetPairs = new List<TileSetPair>();

    [Header("몇 개의 세트를 인젝션할지")]
    public int numberOfSetsToInject = 1;

    private void Start()
    {
        var runtimeDungeon = FindAnyObjectByType<RuntimeDungeon>();
        if (runtimeDungeon == null)
        {
            Debug.LogError("RuntimeDungeon이 씬에 존재하지 않음!");
            return;
        }

        runtimeDungeon.Generator.TileInjectionMethods += InjectTiles;
    }

    private void InjectTiles(RandomStream randomStream, ref List<InjectedTile> tiles)
    {
        if (tileSetPairs.Count == 0 || numberOfSetsToInject <= 0)
            return;


        var injectionSeed = SharedData.Instance.networkSeed.Value;
        var rng = new System.Random(injectionSeed);

        List<TileSetPair> shuffled = new List<TileSetPair>(tileSetPairs);
        Shuffle(shuffled, rng);

        int injectCount = Mathf.Min(numberOfSetsToInject, shuffled.Count);
        int actualTileCount = 0;

        for (int i = 0; i < injectCount; i++)
        {
            var pair = shuffled[i];

            if (TryAddTile(pair.tileA, tiles)) actualTileCount++;
            if (TryAddTile(pair.tileB, tiles)) actualTileCount++;
        }

        Debug.Log($"[Seed {injectionSeed}] 총 {actualTileCount}개의 타일이 인젝션되었습니다.");
    }

    private bool TryAddTile(InjectTileData config, List<InjectedTile> tiles)
    {
        if (config == null || config.tileSet == null)
        {
            return false;
        }

        tiles.Add(new InjectedTile(
            config.tileSet,
            config.isOnMainPath,
            config.pathDepth,
            config.branchDepth,
            config.isRequired
        ));

        return true;
    }

    private void Shuffle<T>(List<T> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
