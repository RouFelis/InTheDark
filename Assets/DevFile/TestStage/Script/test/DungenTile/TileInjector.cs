using UnityEngine;
using DunGen;
using System.Collections.Generic;

public class TileInjector : MonoBehaviour
{
    public RuntimeDungeon runtimeDungeon;      // ���� �ִ� RuntimeDungeon ������Ʈ
    public TileSet tileSetToInject;            // �������� TileSet (���⼭ ������ Ÿ�� 1�� ���õ�)

    private void Start()
    {
        if (runtimeDungeon == null)
            runtimeDungeon = FindObjectOfType<RuntimeDungeon>();

        if (runtimeDungeon != null && tileSetToInject != null)
        {
            // ���� �����⿡�� Ÿ�� ������ �޼��� ���
            runtimeDungeon.Generator.TileInjectionMethods += InjectMyTile;
        }
    }

    // Ÿ�� ������ �޼���
    private void InjectMyTile(RandomStream rng, ref List<InjectedTile> tilesToInject)
    {
        // ������ Ÿ�� ����
        bool isOnMainPath = true;      // ���� ��ο� ����
        float pathDepth = 0.5f;        // ��� �߰��� ����
        float branchDepth = 0f;        // ��� ���� (���� ��δϱ�)

        InjectedTile tile = new InjectedTile(tileSetToInject, isOnMainPath, pathDepth, branchDepth);
        tilesToInject.Add(tile);
    }
}
