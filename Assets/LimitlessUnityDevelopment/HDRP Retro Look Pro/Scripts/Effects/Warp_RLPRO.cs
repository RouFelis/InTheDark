using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using RetroLookPro.Enums;

[Serializable, VolumeComponentMenu(" Retro Look Pro/Warp")]
public sealed class Warp_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Fade adjustment.")]
    public ClampedFloatParameter Fade = new ClampedFloatParameter(0f, 0f, 1f, true);
    [Tooltip("Warp mode.")]
    public WarpModeParameter warpMode = new WarpModeParameter();
    [Tooltip("Warp image corners on x/y axes.")]
    public NoInterpVector2Parameter warp = new NoInterpVector2Parameter(new Vector2(0.03125f, 0.04166f));
    [Tooltip("Warp picture center.")]
    public NoInterpFloatParameter scale = new NoInterpFloatParameter(1f);
    [Tooltip("Enables Clamp sampler state.")]
    public BoolParameter clampSampler = new BoolParameter(true);
    [Space]
    [Tooltip("Mask texture")]
    public TextureParameter mask = new TextureParameter(null);
    public maskChannelModeParameter maskChannel = new maskChannelModeParameter();
    static readonly int _Mask = Shader.PropertyToID("_Mask");
    static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");

    Material m_Material;

    public bool IsActive() => m_Material != null && Fade.value > 0f;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

	public override void Setup()
	{
		if (Shader.Find("Hidden/Shader/WarpEffect_RLPRO") != null)
			m_Material = new Material(Shader.Find("Hidden/Shader/WarpEffect_RLPRO"));
	}

	public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
	{
		if (m_Material == null)
			return;
		m_Material.SetFloat("fade", Fade.value);
		m_Material.SetFloat("scale", scale.value);
		m_Material.SetVector("warp", warp.value);
		m_Material.SetFloat("clamp", clampSampler.value ? 1 : 0);
		if (mask.value != null)
		{
			m_Material.SetTexture(_Mask, mask.value);
			m_Material.SetFloat(_FadeMultiplier, 1);
			ParamSwitch(m_Material, maskChannel.value == maskChannelMode.alphaChannel ? true : false, "ALPHA_CHANNEL");
		}
		else
		{
			m_Material.SetFloat(_FadeMultiplier, 0);
		}

		cmd.Blit(source, destination, m_Material, warpMode == WarpMode.SimpleWarp ? 0 : 1);
	}
	private void ParamSwitch(Material mat, bool paramValue, string paramName)
	{
		if (paramValue) mat.EnableKeyword(paramName);
		else mat.DisableKeyword(paramName);
	}

	public override void Cleanup()
	{
		CoreUtils.Destroy(m_Material);
	}
}
