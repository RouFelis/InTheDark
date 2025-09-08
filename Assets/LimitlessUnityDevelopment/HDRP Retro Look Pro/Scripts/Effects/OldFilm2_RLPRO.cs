using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using RetroLookPro.Enums;

[Serializable, VolumeComponentMenu(" Retro Look Pro/Old Film 2")]
public sealed class OldFilm2_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Effect fade.")]
    public ClampedFloatParameter Fade = new ClampedFloatParameter(0f, 0f, 1f, true);
    public NoInterpClampedFloatParameter SepiaAmount = new NoInterpClampedFloatParameter(0f, 0f, 1f, true);
    [ Tooltip("Frames per second.")]
    public NoInterpClampedFloatParameter NoiseAmount = new NoInterpClampedFloatParameter(1f, 0f, 1);
    [Space]
    [ Tooltip(".")]
    public NoInterpClampedFloatParameter ScratchAmount = new NoInterpClampedFloatParameter(1f, 0f, 1f);
    public NoInterpClampedFloatParameter speed = new NoInterpClampedFloatParameter(1f, 0f, 1f);
    [Tooltip("Image burn.")]
    public NoInterpClampedFloatParameter ScratchSize = new NoInterpClampedFloatParameter(0.88f, 0.0001f, 1f);
    public NoInterpClampedFloatParameter ScratchResolution = new NoInterpClampedFloatParameter(0.88f, 0.0001f, 1f);
    [Space]
    [Range(0f, 16f), Tooltip("Scene cut off.")]
    public NoInterpClampedFloatParameter Grain = new NoInterpClampedFloatParameter(1f, 0f, 1f);
    [Space]
    [Tooltip("Mask texture")]
    public TextureParameter mask = new TextureParameter(null);
    public maskChannelModeParameter maskChannel = new maskChannelModeParameter();
    static readonly int _Mask = Shader.PropertyToID("_Mask");
    static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");

    Material m_Material;
    private float T;

    public bool IsActive() => m_Material != null && Fade.value > 0f;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        if (Shader.Find("Hidden/Shader/OldFilm2Effect_RLPRO") != null)
            m_Material = new Material(Shader.Find("Hidden/Shader/OldFilm2Effect_RLPRO"));
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;
		T += Time.deltaTime;
		if (T > 100) T = 0;		
		m_Material.SetFloat("TComponent", T);
		m_Material.SetFloat("SepiaValue", SepiaAmount.value);
		m_Material.SetFloat("NoiseValue", NoiseAmount.value);
		m_Material.SetFloat("ScratchValue", ScratchAmount.value);
		m_Material.SetFloat("ScratchSize", ScratchSize.value);
		m_Material.SetFloat("ScratchResolution", ScratchResolution.value);
		m_Material.SetFloat("_Grain", Grain.value);
		m_Material.SetFloat("Fade", Fade.value);
		m_Material.SetFloat("speed", speed.value);
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
