using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using RetroLookPro.Enums;

[Serializable, VolumeComponentMenu(" Retro Look Pro/VHS Scanlines")]
public sealed class VHSScanlines_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Effect fade.")]
    public ClampedFloatParameter Fade = new ClampedFloatParameter(0f, 0f, 1f, true);
    [Tooltip("Lines color.")]
    public ColorParameter scanLinesColor = new ColorParameter(new Color());
    [Tooltip("Amount of scanlines.")]
    public FloatParameter scanLines = new FloatParameter(1.5f);
    [Tooltip("Lines speed.")]
    public FloatParameter speed = new FloatParameter(0);
    [Tooltip("Enable horizontal lines.")]
    public BoolParameter horizontal = new BoolParameter(true);
    [Tooltip("distortion.")]
    public ClampedFloatParameter distortion = new ClampedFloatParameter(0.2f, 0f, 0.5f);
    [Tooltip("distortion1.")]
    public FloatParameter distortion1 = new FloatParameter(0);
    [Tooltip("distortion2.")]
    public FloatParameter distortion2 = new FloatParameter(0);
    [Tooltip("Scale lines size.")]
    public FloatParameter scale = new FloatParameter(1);
    [Space]
    [Tooltip("Mask texture")]
    public TextureParameter mask = new TextureParameter(null);
    public maskChannelModeParameter maskChannel = new maskChannelModeParameter();
    static readonly int _Mask = Shader.PropertyToID("_Mask");
    static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");

    Material m_Material;
    private int pass;
    private float T;
    public bool IsActive() => m_Material != null && Fade.value > 0f;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        if (Shader.Find("Hidden/Shader/VHSScanlinesEffect_RLPRO") != null)
            m_Material = new Material(Shader.Find("Hidden/Shader/VHSScanlinesEffect_RLPRO"));
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;
        T += Time.deltaTime;

        m_Material.SetFloat("Time", T);
        m_Material.SetFloat("_ScanLines", scanLines.value);
        m_Material.SetFloat("speed", speed.value);
        m_Material.SetFloat("_OffsetDistortion", distortion.value);
        m_Material.SetFloat("fade", Fade.value);
        m_Material.SetFloat("sferical", distortion1.value);
        m_Material.SetFloat("barrel", distortion2.value);
        m_Material.SetFloat("scale", scale.value);
        m_Material.SetColor("_ScanLinesColor", scanLinesColor.value);
        if (horizontal.value)
        {
            if (distortion.value != 0)
                pass = 1;
            else
                pass = 0;
        }
        else
        {
            if (distortion.value != 0)
                pass = 3;
            else
                pass = 2;
        }
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

        cmd.Blit(source, destination, m_Material, pass);
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
