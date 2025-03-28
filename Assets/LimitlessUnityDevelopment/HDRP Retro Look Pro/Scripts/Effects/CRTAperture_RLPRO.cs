using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using RetroLookPro.Enums;

[Serializable, VolumeComponentMenu(" Retro Look Pro/CRT Aperture")]
public sealed class CRTAperture_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the fade of the effect.")]
    public ClampedFloatParameter Fade = new ClampedFloatParameter(0f, 0f, 1f);

    [Tooltip("Glow Halation.")]
    public NoInterpClampedFloatParameter GlowHalation = new NoInterpClampedFloatParameter(4.27f, 0f, 5f);
    [Tooltip("Glow Difusion.")]
    public NoInterpClampedFloatParameter GlowDifusion = new NoInterpClampedFloatParameter(0.83f, 0f, 2f);
    [Tooltip("Mask Colors.")]
    public NoInterpClampedFloatParameter MaskColors = new NoInterpClampedFloatParameter(0.57f, 0f, 5f);
    [Tooltip("Mask Strength.")]
    public NoInterpClampedFloatParameter MaskStrength = new NoInterpClampedFloatParameter(0.318f, 0f, 1f);
    [Tooltip("Gamma Input.")]
    public NoInterpClampedFloatParameter GammaInput = new NoInterpClampedFloatParameter(1.12f, 0f, 5f);
    [Tooltip("Gamma Output.")]
    public NoInterpClampedFloatParameter GammaOutput = new NoInterpClampedFloatParameter(0.89f, 0f, 5f);
    [Tooltip("Brightness.")]
    public NoInterpClampedFloatParameter Brightness = new NoInterpClampedFloatParameter(0.85f, 0f, 2.5f);
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
		if (Shader.Find("Hidden/Shader/CRTAperture_RLPRO") != null)
			m_Material = new Material(Shader.Find("Hidden/Shader/CRTAperture_RLPRO"));
	}

	public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
	{
		if (m_Material == null)
			return;
		m_Material.SetFloat("GLOW_HALATION", GlowHalation.value);
		m_Material.SetFloat("GLOW_DIFFUSION", GlowDifusion.value);
		m_Material.SetFloat("MASK_COLORS", MaskColors.value);
		m_Material.SetFloat("MASK_STRENGTH", MaskStrength.value);
		m_Material.SetFloat("GAMMA_INPUT", GammaInput.value);
		m_Material.SetFloat("GAMMA_OUTPUT", GammaOutput.value);
		m_Material.SetFloat("BRIGHTNESS", Brightness.value);
		m_Material.SetFloat("fade", Fade.value);
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
        cmd.Blit(source, destination, m_Material, 0);
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
