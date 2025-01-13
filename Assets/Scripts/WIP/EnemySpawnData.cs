using System;
using UnityEngine;

[CreateAssetMenu(fileName = FILE_NAME, menuName = MENU_NAME)]
public class EnemySpawnData : ScriptableObject
{
    private const string FILE_NAME = "New Enemy Spawn Data";
    private const string MENU_NAME = "Scriptable Objects/Enemy/Spawn Data";

    [SerializeField]
    private EnemyPrototypePawn _pawnPrefab;

    public void Spawn()
    {

	}
}
