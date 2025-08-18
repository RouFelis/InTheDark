using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAnimationManager : MonoBehaviour
{
    public static UIAnimationManager Instance { get; private set; }

    [Header("UI�ִϸ��̼Ǹ��")]
    [SerializeField] private List<NamedUIAnimation> animations = new List<NamedUIAnimation>();

    [Header("UI ���")]
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private RectTransform compass;

    private Dictionary<string, UIAnimation> animationLookup;

    private void Awake()
    {
        // ���� �˻��� ���� Dictionary ��ȯ
        animationLookup = new Dictionary<string, UIAnimation>();
        foreach (var item in animations)
        {
            if (!animationLookup.ContainsKey(item.name))
                animationLookup.Add(item.name, item.animation);
            else
                Debug.LogWarning($"UIAnimationManager: �ߺ��� �̸� '{item.name}'�� �����Ǿ����ϴ�.");
        }
        Instance = this;
    }

    public void Play(string name)
    {
        if (animationLookup.TryGetValue(name, out UIAnimation anim))
        {
            anim.gameObject.SetActive(true);
            anim.StartEffect();
        }
        else
        {
            Debug.LogError($"UIAnimationManager: '{name}' �ִϸ��̼��� ã�� �� �����ϴ�.");
        }
    }

    #region ���� ����
    public void DieAnimation()
    {
        healthBar.localScale = Vector3.zero;
        compass.localScale = Vector3.zero;

        Play("AllDieEffect1");
        Play("AllDieEffect2");
        Play("AllDieEffect3");
    }

    public IEnumerator AllDieAnimationCo()
    {
        if (animationLookup.TryGetValue("DieAnime", out var anim))
            anim.gameObject.SetActive(true);

        yield return new WaitForSeconds(1.618f);

        Play("DieAnime");

        yield return new WaitForSeconds(2f);

        anim.gameObject.SetActive(false);
    }

    public void ReviveAnimation()
    {
        Play("ReviveAnimeFadeIn");
        StartCoroutine(HealthbarOn());
    }

    public void FadeOutAnimation()
    {
        Play("ReviveAnimeFadeOut");
    }

    private IEnumerator HealthbarOn()
    {
        yield return new WaitForSeconds(1f);
        healthBar.localScale = Vector3.one;
        compass.localScale = Vector3.one;
    }
    #endregion

    private void OnEnable()
    {
        if (animationLookup.TryGetValue("AllDieEffect3", out var anim))
        {
            anim.OnAnimationFinished += DisableDieUIAnimations;
		}
		else
		{
            Debug.LogError("AllDieEffect3�� ã�� �� ����.");
		}
    }

    private void OnDisable()
    {
        if (animationLookup.TryGetValue("AllDieEffect3", out var anim))
        {
            anim.OnAnimationFinished -= DisableDieUIAnimations;
        }
        else
        {
            Debug.LogError("AllDieEffect3�� ã�� �� ����.");
        }
    }


    public void AllDieAnimation()
    {
            StartCoroutine(AllDieAnimationCo());
    }

    private void DisableDieUIAnimations()
    {
        if (animationLookup.TryGetValue("AllDieEffect1", out var a1)) a1.gameObject.SetActive(false);
        if (animationLookup.TryGetValue("AllDieEffect2", out var a2)) a2.gameObject.SetActive(false);
        if (animationLookup.TryGetValue("AllDieEffect3", out var a3)) a3.gameObject.SetActive(false);
    }
}
