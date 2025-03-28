using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu(" Retro Look Pro/Pulsating Vignette")]
public sealed class PulsatingVignette_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Vignette amount.")]
    public ClampedFloatParameter Amount = new ClampedFloatParameter(0f, 0.001f, 50f, true);
    [Range(0.001f, 50f), Tooltip("Vignette shake speed.")]
    public NoInterpClampedFloatParameter speed = new NoInterpClampedFloatParameter(1f, 0.001f, 50f);
    Material m_Material;
    private float T;
    public bool IsActive() => m_Material != null && Amount.value > 0f;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        if (Shader.Find("Hidden/Shader/PulsatingVignetteEffect_RLPRO") != null)
            m_Material = new Material(Shader.Find("Hidden/Shader/PulsatingVignetteEffect_RLPRO"));
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;
		T += Time.deltaTime;
		m_Material.SetFloat("Time", T);
		m_Material.SetFloat("vignetteSpeed", speed.value);
		m_Material.SetFloat("vignetteAmount", Amount.value);
        cmd.Blit(source, destination, m_Material, 0);
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
    }
}
