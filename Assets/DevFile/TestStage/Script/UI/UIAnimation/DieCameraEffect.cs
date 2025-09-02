using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class DieCameraEffect : UIAnimation
{
    [Header("Target UI")]
    [SerializeField] private RectTransform uiRoot;
    [SerializeField] private CanvasGroup uiGroup;

    [Header("Scale Settings")]
    [SerializeField] private float scaleUpSize = 1.5f;         // ó�� Ȯ�� ����
    [SerializeField] private float scaleDelay = 0.5f;          // �󸶳� ��ٸ��� (�� �پ��� ��)

    [SerializeField] private bool IsLastAnimation;

    private Vector3 originalScale;


    void Start()
	{
		if (uiRoot == null || uiGroup == null)
		{
			Debug.LogError("UI Root �Ǵ� CanvasGroup�� ������� �ʾҽ��ϴ�!");
			return;
		}

		originalScale = uiRoot.localScale;
        gameObject.SetActive(false);
	}


    protected override IEnumerator PlayRoutine()
    {
        // Step 1: Ȯ��
        uiRoot.localScale = originalScale * scaleUpSize;

        // Step 2: ���� �ð� ���
        yield return new WaitForSeconds(scaleDelay);

        // Step 3: �� ���� ���� ũ��� ���̱� (��)
        uiRoot.localScale = originalScale;

        yield return new WaitForSeconds(2f);

        if (IsLastAnimation)
        {
            FinishAnimation(true);
        }
        FinishAnimation();
    }

}
