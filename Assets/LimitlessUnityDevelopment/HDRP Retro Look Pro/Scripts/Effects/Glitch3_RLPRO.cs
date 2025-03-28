using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using RetroLookPro.Enums;

[Serializable, VolumeComponentMenu(" Retro Look Pro/Glitch3")]
public sealed class Glitch3_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("glitch offset.(color shift)")]
    public ClampedFloatParameter MaxDisplace = new ClampedFloatParameter(0f, 0f, 5f, true);
    [Tooltip("block size (higher value = smaller blocks).")]
    public NoInterpClampedFloatParameter Density = new NoInterpClampedFloatParameter(1f, 0f, 5f);
    [Tooltip("Speed")]
    public NoInterpClampedFloatParameter speed = new NoInterpClampedFloatParameter(1f, 0f, 5f);

    [Space]
    [Tooltip("Mask texture")]
    public TextureParameter mask = new TextureParameter(null);
    public maskChannelModeParameter maskChannel = new maskChannelModeParameter();
    static readonly int _Mask = Shader.PropertyToID("_Mask");
    static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");

    //
    Material m_Material;
    private float T;

    public bool IsActive() => m_Material != null || Density.value > 0f || MaxDisplace.value > 0f;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        if (Shader.Find("Hidden/Shader/Glitch3Effect_RLPRO") != null)
            m_Material = new Material(Shader.Find("Hidden/Shader/Glitch3Effect_RLPRO"));
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;

		T += Time.deltaTime;
		m_Material.SetFloat("speed",  speed.value);
		m_Material.SetFloat("density",  Density.value);
		m_Material.SetFloat("maxDisplace",  MaxDisplace.value);
		m_Material.SetFloat("Time", T);
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
