using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using RetroLookPro.Enums;

[Serializable, VolumeComponentMenu(" Retro Look Pro/Negative")]
public sealed class Negative_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f,true);
	[Range(0f, 2f), Tooltip("Brightness.")]
	public NoInterpClampedFloatParameter luminosity = new NoInterpClampedFloatParameter(0f, 0f, 1.1f);
	[Range(0f, 1f), Tooltip("Vignette amount.")]
	public NoInterpClampedFloatParameter vignette = new NoInterpClampedFloatParameter(1f, 0f, 1f);
	[Range(0f, 1f), Tooltip("Contrast amount.")]
	public NoInterpClampedFloatParameter contrast = new NoInterpClampedFloatParameter(0.7f, 0f, 1f);
	[Range(0f, 1f), Tooltip("Negative amount.")]
	public NoInterpClampedFloatParameter negative = new NoInterpClampedFloatParameter(1f, 0f, 1f);
	[Space]
	[Tooltip("Mask texture")]
	public TextureParameter mask = new TextureParameter(null);
	public maskChannelModeParameter maskChannel = new maskChannelModeParameter();
	static readonly int _Mask = Shader.PropertyToID("_Mask");
	static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");

	Material m_Material;
	float T;

	public bool IsActive() => m_Material != null && intensity.value > 0f;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        if (Shader.Find("Hidden/Shader/NegativeEffect_RLPRO") != null)
            m_Material = new Material(Shader.Find("Hidden/Shader/NegativeEffect_RLPRO"));
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;

        m_Material.SetFloat("_Intensity", intensity.value);
		T += Time.deltaTime;
		if (T > 100) T = 0;
		m_Material.SetFloat("T", T);
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
		m_Material.SetFloat("Luminosity", 2 - luminosity.value);
		m_Material.SetFloat("Contrast", 1-contrast.value);
		m_Material.SetFloat("Vignette", 1 - vignette.value);
		m_Material.SetFloat("Negative", negative.value);
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
