using UnityEngine;

public abstract class EnemyRandomBoxTrigger : ScriptableObject
{
    public abstract void OnUpdate(EnemyPrototypePawn pawn);
}