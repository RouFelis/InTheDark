using UnityEngine;
using System;
using System.Collections;

public class UIAnimation : MonoBehaviour, IUiAnimation
{
    public event Action OnAnimationFinished;
    protected Coroutine playingCoroutine;

    public virtual void StartEffect()
    {
        // �̹� ���� ���� �ִϸ��̼��� ������ ����
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
            playingCoroutine = null;
        }

        // �� �ִϸ��̼� ����
        playingCoroutine = StartCoroutine(PlayRoutine());
    }

    /// <summary>
    /// ���� �ִϸ��̼� ������ �����ϴ� �޼���
    /// </summary>
    protected virtual IEnumerator PlayRoutine()
    {
        Debug.Log($"Animation Start: {this.name}");
        yield return null;

        FinishAnimation();
    }

    /// <summary>
    /// �ִϸ��̼� ���� �� ȣ��
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
