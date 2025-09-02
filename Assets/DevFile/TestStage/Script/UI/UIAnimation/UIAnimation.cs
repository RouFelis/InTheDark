using UnityEngine;
using System;
using System.Collections;

public class UIAnimation : MonoBehaviour, IUiAnimation
{
    public event Action OnAnimationFinished;
    protected Coroutine playingCoroutine;

    public virtual void StartEffect()
    {
        // 이미 실행 중인 애니메이션이 있으면 중지
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
            playingCoroutine = null;
        }

        // 새 애니메이션 실행
        playingCoroutine = StartCoroutine(PlayRoutine());
    }

    /// <summary>
    /// 실제 애니메이션 로직을 구현하는 메서드
    /// </summary>
    protected virtual IEnumerator PlayRoutine()
    {
        Debug.Log($"Animation Start: {this.name}");
        yield return null;

        FinishAnimation();
    }

    /// <summary>
    /// 애니메이션 종료 시 호출
    /// </summary>
    protected void FinishAnimation()
    {
        playingCoroutine = null;
    }

    protected void FinishAnimation(bool value)
    {
        if(value)
            OnAnimationFinished?.Invoke();

        playingCoroutine = null;
    }
}
