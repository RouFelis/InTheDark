using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using RetroLookPro.Enums;

[Serializable]
public sealed class maskChannelModeParameter : VolumeParameter<maskChannelMode> { };

[Serializable, VolumeComponentMenu("/Retro Look Pro/Analog TV Noise")]
public sealed class AnalogTVNoise_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Effect Fade.")]
    public ClampedFloatParameter fade = new ClampedFloatParameter(0f, 0f, 1f,true);
    [Tooltip("Option enables static noise (without movement).")]
    public BoolParameter staticNoise = new BoolParameter(false);
    [Tooltip("Horizontal/Vertical Noise lines.")]
    public BoolParameter Horizontal = new BoolParameter(true);
    [Range(0f, 60f), Tooltip("Noise bar width.")]
    public NoInterpClampedFloatParameter barWidth = new NoInterpClampedFloatParameter(21f, 0f, 60f);
    [Range(0f, 60f), Tooltip("Noise tiling.")]
    public NoInterpVector2Parameter tile = new NoInterpVector2Parameter(new Vector2(1, 1));
    [Range(0f, 1f), Tooltip("Noise texture angle.")]
    public NoInterpClampedFloatParameter textureAngle = new NoInterpClampedFloatParameter(1f, 0f, 1f);
    [Range(0f, 100f), Tooltip("Noise bar edges cutoff.")]
    public NoInterpClampedFloatParameter edgeCutOff = new NoInterpClampedFloatParameter(0f, 0f, 100f);
    [Range(-1f, 1f), Tooltip("Noise cutoff.")]
    public NoInterpClampedFloatParameter CutOff = new NoInterpClampedFloatParameter(1f, -1f, 1f);
    [Range(-10f, 10f), Tooltip("Noise bars speed.")]
    public NoInterpClampedFloatParameter barSpeed = new NoInterpClampedFloatParameter(1f, -60f, 60f);
    [Tooltip("Noise texture.")]
    public TextureParameter texture = new TextureParameter(null);
    [Space]
    [Tooltip("Mask texture")]
    public TextureParameter mask = new TextureParameter(null);
    public maskChannelModeParameter maskChannel = new maskChannelModeParameter();

    static readonly int _Mask = Shader.PropertyToID("_Mask");
    static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");


    //
    Material m_Material;
    float TimeX;
    public bool IsActive() => m_Material != null && fade.value > 0f;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        if (Shader.Find("Hidden/Shader/AnalogTVNoiseEffect_RLPRO") != null)
            m_Material = new Material(Shader.Find("Hidden/Shader/AnalogTVNoiseEffect_RLPRO"));
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;
        TimeX += Time.deltaTime;
        if (TimeX > 100) TimeX = 0;

        m_Material.SetFloat("TimeX", TimeX);
        m_Material.SetFloat("_Fade", fade.value);
        if (texture.value != null)
            m_Material.SetTexture("_Pattern", texture.value);
        m_Material.SetFloat("barHeight", barWidth.value);
        m_Material.SetFloat("barSpeed", barSpeed.value);
        m_Material.SetFloat("cut", CutOff.value);
        m_Material.SetFloat("edgeCutOff", edgeCutOff.value);
        m_Material.SetFloat("angle", textureAngle.value);
        m_Material.SetFloat("tileX", tile.value.x);
        m_Material.SetFloat("tileY", tile.value.y);
        m_Material.SetFloat("horizontal", Horizontal.value ? 1 : 0);
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
        if (!staticNoise.value)
        {
            m_Material.SetFloat("_OffsetNoiseX", UnityEngine.Random.Range(0f, 0.6f));
            m_Material.SetFloat("_OffsetNoiseY", UnityEngine.Random.Range(0f, 0.6f));
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
