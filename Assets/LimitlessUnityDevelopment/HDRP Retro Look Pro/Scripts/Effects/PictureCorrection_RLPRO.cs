using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using RetroLookPro.Enums;

[Serializable, VolumeComponentMenu(" Retro Look Pro/Picture Correction")]
public sealed class PictureCorrection_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
    [Range(-0.25f, 0.25f), Tooltip(" Y permanent adjustment..")]
    public NoInterpClampedFloatParameter signalAdjustY = new NoInterpClampedFloatParameter(0f, -0.25f, 0.25f);
    [Range(-0.25f, 0.25f), Tooltip("I permanent adjustment..")]
    public NoInterpClampedFloatParameter signalAdjustI = new NoInterpClampedFloatParameter(0f, -0.25f, 0.25f);
    [Range(-0.25f, 0.25f), Tooltip("Q permanent adjustment..")]
    public NoInterpClampedFloatParameter signalAdjustQ = new NoInterpClampedFloatParameter(0f, -0.25f, 0.25f);
    [Range(-2f, 2f), Tooltip("tweak/shift Y values..")]
    public NoInterpClampedFloatParameter signalShiftY = new NoInterpClampedFloatParameter(1f, -2f, 2f);
    [Range(-2f, 2f), Tooltip("tweak/shift I values..")]
    public NoInterpClampedFloatParameter signalShiftI = new NoInterpClampedFloatParameter(1f, -2f, 2f);
    [Range(-2f, 2f), Tooltip("tweak/shift Q values..")]
    public NoInterpClampedFloatParameter signalShiftQ = new NoInterpClampedFloatParameter(1f, -2f, 2f);
    [Range(0f, 2f), Tooltip("use this to balance the gamma(brightness) of the signal.")]
    public NoInterpClampedFloatParameter gammaCorection = new NoInterpClampedFloatParameter(1f, -0f, 2f);
    [Space]
    [Tooltip("Mask texture")]
    public TextureParameter mask = new TextureParameter(null);
    public maskChannelModeParameter maskChannel = new maskChannelModeParameter();
    static readonly int _Mask = Shader.PropertyToID("_Mask");
    static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");

    Material m_Material;

    public bool IsActive() => m_Material != null && intensity.value > 0f;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        if (Shader.Find("Hidden/Shader/PictureCorrectionEffect_RLPRO") != null)
            m_Material = new Material(Shader.Find("Hidden/Shader/PictureCorrectionEffect_RLPRO"));
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;
		m_Material.SetFloat("signalAdjustY",  signalAdjustY.value);
		m_Material.SetFloat("signalAdjustI",  signalAdjustI.value);
		m_Material.SetFloat("signalAdjustQ",  signalAdjustQ.value);
		m_Material.SetFloat("signalShiftY",  signalShiftY.value);
		m_Material.SetFloat("signalShiftI",  signalShiftI.value);
		m_Material.SetFloat("signalShiftQ",  signalShiftQ.value);
		m_Material.SetFloat("gammaCorection",  gammaCorection.value);
		m_Material.SetFloat("_Intensity", intensity.value);
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
