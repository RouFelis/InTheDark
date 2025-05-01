using UnityEngine;
using System;

public class UIAnimation : MonoBehaviour, IUiAnimation
{
    public event Action OnAnimationFinished;

    protected void ActionInvoke()
	{
        OnAnimationFinished.Invoke();
    }

    public virtual void StartEffect()
    {
        Debug.Log($"Aniamtion + {this.name}");
    }    
}
