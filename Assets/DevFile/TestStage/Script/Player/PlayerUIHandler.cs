using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Unity.Netcode;

[DisallowMultipleComponent]
public class PlayerUIHandler : NetworkBehaviour
{
    [Header("Vignette settings")]
    public float vignettePeak = 0.6f;
    public float vignetteAttack = 0.1f;
    public float vignetteDecay = 1f;
    public float vignetteHold = 2f;

    private Player player;
    private PlayerStats stats;
    private Image healthBar;
    private Volume postProcessingVolume;
    private Vignette vignette;
    private Coroutine activeHitEffect;
    private Vector3 originalCameraPosition;

    public void Initialize(Player player, PlayerStats stats)
    {
        this.player = player;
        this.stats = stats;

        healthBar = GameObject.Find("HealthBar")?.GetComponent<Image>();
        postProcessingVolume = GameObject.Find("Vigentte")?.GetComponent<Volume>();
        if (postProcessingVolume != null && postProcessingVolume.profile.TryGet(out Vignette v)) vignette = v;

        if (player.FirstPersonCamera != null) originalCameraPosition = player.FirstPersonCamera.transform.localPosition;
    }

    public void OnDamageTaken()
    {
        StartCameraShake();
        StartHitEffect();
        UpdateHealthBar();
    }

    private void StartCameraShake()
    {
        StartCoroutine(CameraShakeCoroutine());
    }

    private IEnumerator CameraShakeCoroutine()
    {
        float duration = 0.3f;
        float magnitude = 0.00015f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector3 offset = UnityEngine.Random.insideUnitSphere * magnitude;
            if (player.FirstPersonCamera != null)
                player.FirstPersonCamera.transform.localPosition = originalCameraPosition + offset;
            yield return null;
        }
        if (player.FirstPersonCamera != null) player.FirstPersonCamera.transform.localPosition = originalCameraPosition;
    }

    private void StartHitEffect()
    {
        if (activeHitEffect != null) StopCoroutine(activeHitEffect);
        activeHitEffect = StartCoroutine(HitEffectCoroutine());
    }

    private IEnumerator HitEffectCoroutine()
    {
        if (vignette == null) yield break;
        yield return AdjustVignette(0f, vignettePeak, vignetteAttack);
        yield return new WaitForSeconds(vignetteHold);
        yield return AdjustVignette(vignettePeak, 0f, vignetteDecay);
    }

    private IEnumerator AdjustVignette(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (vignette != null) vignette.intensity.value = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null || stats.maxHealth <= 0f) return;
        healthBar.fillAmount = Mathf.Clamp01(player.GetComponent<PlayerNetworkData>().Health.Value / stats.maxHealth) * 0.5f;
    }


    [ClientRpc]
    public void GlitchClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if(IsOwner)
        {
            PlayGlitchAndBlackoutLocal();
            Debug.Log("테스트 111111111");
		}
    }

    [ClientRpc]
    public void FadeinClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (IsOwner)
        {
            FadeInFromBlackLocal();
            Debug.Log("테스트 22222222222");
        }
    }


    // 글리치 로컬...
    private void PlayGlitchAndBlackoutLocal()
    {
        UIAnimationManager.Instance.FadeOutAnimation();
    }

    // 페이드인 로컬...
    private void FadeInFromBlackLocal()
    {
        UIAnimationManager.Instance.ReviveAnimation();
    }



}
