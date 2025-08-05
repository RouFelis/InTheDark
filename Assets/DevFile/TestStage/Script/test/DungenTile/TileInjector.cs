using UnityEngine;
using DunGen;
using System.Collections.Generic;

public class TileInjector : MonoBehaviour
{
    public RuntimeDungeon runtimeDungeon;      // 씬에 있는 RuntimeDungeon 컴포넌트
    public TileSet tileSetToInject;            // 인젝션할 TileSet (여기서 무작위 타일 1개 선택됨)

    private void Start()
    {
        if (runtimeDungeon == null)
            runtimeDungeon = FindObjectOfType<RuntimeDungeon>();

        if (runtimeDungeon != null && tileSetToInject != null)
        {
            // 던전 생성기에서 타일 인젝션 메서드 등록
            runtimeDungeon.Generator.TileInjectionMethods += InjectMyTile;
        }
    }

    // 타일 인젝션 메서드
    private void InjectMyTile(RandomStream rng, ref List<InjectedTile> tilesToInject)
    {
        // 주입할 타일 설정
        bool isOnMainPath = true;      // 메인 경로에 삽입
        float pathDepth = 0.5f;        // 경로 중간쯤 삽입
        float branchDepth = 0f;        // 사용 안함 (메인 경로니까)

        InjectedTile tile = new InjectedTile(tileSetToInject, isOnMainPath, pathDepth, branchDepth);
        tilesToInject.Add(tile);
    }
}
