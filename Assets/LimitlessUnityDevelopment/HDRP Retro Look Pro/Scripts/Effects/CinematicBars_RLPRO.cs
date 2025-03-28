using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu(" Retro Look Pro/Cinematic Bars")]
public sealed class CinematicBars_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Black bars amount (width)")]
    public ClampedFloatParameter Amount = new ClampedFloatParameter(0f, 0.01f, 0.51f, true);
    [Tooltip("Fade black bars.")]
    public NoInterpClampedFloatParameter fade = new NoInterpClampedFloatParameter(1f, 0f, 1f);
    Material m_Material;

    public bool IsActive() => m_Material != null && Amount.value > 0f;

	public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

	public override void Setup()
	{
		if (Shader.Find("Hidden/Shader/CinematicBarsEffect_RLPRO") != null)
			m_Material = new Material(Shader.Find("Hidden/Shader/CinematicBarsEffect_RLPRO"));
	}

	public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
	{
		if (m_Material == null)
			return;
		m_Material.SetFloat("_Stripes", 0.51f - Amount.value);
		m_Material.SetFloat("_Fade", fade.value);
        cmd.Blit(source, destination, m_Material, 0);
    }

    public override void Cleanup()
	{
		CoreUtils.Destroy(m_Material);
	}
}
