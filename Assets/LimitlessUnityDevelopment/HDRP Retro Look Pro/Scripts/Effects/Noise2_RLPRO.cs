using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using RetroLookPro.Enums;
public enum noiseParam
{
    blackWhite, color
}
[Serializable]
public sealed class NoiseParameter : VolumeParameter<noiseParam> { };
[Serializable, VolumeComponentMenu(" Retro Look Pro/Noise 2")]
public sealed class Noise2_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);
    [Range(0f, 1f), Tooltip("Dark areas adjustment.")]
    public ClampedFloatParameter fade = new ClampedFloatParameter(0f, 0f, 1f, true);
    [Header("Tape Noise Settings")]
    public NoInterpClampedFloatParameter waveAmount = new NoInterpClampedFloatParameter(1f, 0f, 10f);
    public NoInterpClampedFloatParameter tapeIntensity = new NoInterpClampedFloatParameter(1f, 0f, 1f);
    public NoInterpClampedFloatParameter tapeLinesAmount = new NoInterpClampedFloatParameter(1f, 0f, 10f);
    public NoInterpClampedFloatParameter tapeSpeed = new NoInterpClampedFloatParameter(1f, 0f, 1f);
    [Header("Noise Settings")]
    public NoiseParameter Noise = new NoiseParameter();
    [Tooltip("threshold.")]
    public NoInterpClampedFloatParameter threshold = new NoInterpClampedFloatParameter(1f, 0f, 1f);
    public BoolParameter Smoother = new BoolParameter(false);
    [Space]
    [Tooltip("Mask texture")]
    public TextureParameter mask = new TextureParameter(null);
    public maskChannelModeParameter maskChannel = new maskChannelModeParameter();
    static readonly int _Mask = Shader.PropertyToID("_Mask");
    static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");

    Material m_Material;

    public bool IsActive() => (bool)enable;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        if (Shader.Find("Hidden/Shader/Noise2Effect_RLPRO") != null)
            m_Material = new Material(Shader.Find("Hidden/Shader/Noise2Effect_RLPRO"));
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;
        m_Material.SetFloat("threshold", 1 - threshold.value);
        m_Material.SetFloat("Smoother", Smoother.value ? 1 : 0);
        m_Material.SetFloat("Fade", fade.value);
        m_Material.SetFloat("waveAmount", waveAmount.value);
        m_Material.SetFloat("tapeLinesAmount", tapeLinesAmount.value);
        m_Material.SetFloat("tapeIntensity", tapeIntensity.value);
        m_Material.SetFloat("tapeSpeed", tapeSpeed.value);
        m_Material.SetInt("NoiseVal", Noise == noiseParam.blackWhite ? 0 : 1);
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
