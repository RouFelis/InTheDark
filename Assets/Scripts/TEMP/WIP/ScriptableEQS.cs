using UnityEngine;

public abstract class ScriptableEQS : ScriptableObject
{
    [SerializeField]
    private bool _isDisplayable;

    protected bool IsDisplayable => _isDisplayable;

    public abstract void OnAwake();

    public abstract void OnUpdate(EQSConsole.Item item);
}
