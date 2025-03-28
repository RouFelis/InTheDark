using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using RetroLookPro.Enums;

[Serializable]
public sealed class WarpModeParameter : VolumeParameter<WarpMode> { };

[Serializable, VolumeComponentMenu(" Retro Look Pro/TV Effect")]
public sealed class TV_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Effect fade.")]
    public ClampedFloatParameter Fade = new ClampedFloatParameter(0, 0, 1, true);

    [Range(0f, 2f), Tooltip("Dark areas adjustment.")]
    public NoInterpClampedFloatParameter maskDark = new NoInterpClampedFloatParameter(0.5f, 0, 2f);

    [Range(0f, 2f), Tooltip("Light areas adjustment.")]
    public NoInterpClampedFloatParameter maskLight = new NoInterpClampedFloatParameter(1.5f, 0, 2f);

    [Range(-8f, -16f), Tooltip("Dark areas fine tune.")]
    public NoInterpClampedFloatParameter hardScan = new NoInterpClampedFloatParameter(-8f, -8f, 16f);

    [Space, Tooltip("Correct effect resolution, depending on screen resolution")]
    public BoolParameter ScaleWithActualScreenSize = new BoolParameter(false);

    [Range(1f, 16f), Tooltip("Effect resolution scale.")]
    public NoInterpClampedFloatParameter resScale = new NoInterpClampedFloatParameter(4f, 1f, 16f);

    [Tooltip("Screen width at which resScale is used literally. Larger or smaller widths auto-adjust.")]
    public int referenceWidth = 1920;

    [Space, Range(-3f, 1f), Tooltip("pixels sharpness.")]
    public NoInterpClampedFloatParameter hardPix = new NoInterpClampedFloatParameter(-3f, -3f, 1f);

    [Tooltip("Warp mode.")]
    public WarpModeParameter warpMode = new WarpModeParameter { };

    [Tooltip("Warp picture (barrel distortion).")]
    public Vector2Parameter warp = new Vector2Parameter(new Vector2(0f, 0f));

    public NoInterpFloatParameter scale = new NoInterpFloatParameter(0.5f);

    [Space, Tooltip("Mask texture")]
    public TextureParameter mask = new TextureParameter(null);
    public maskChannelModeParameter maskChannel = new maskChannelModeParameter();

    static readonly int _Mask = Shader.PropertyToID("_Mask");
    static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");

    // The Material that uses Hidden/Shader/TV_RLPRO_HDRP
    Material m_Material;

    // We'll store the final scaled value in 'scaler'
    float scaler;

    public bool IsActive() => m_Material != null && Fade.value > 0f;

    // We run after post-processing in HDRP
    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        if (Shader.Find("Hidden/Shader/TV_RLPRO_HDRP") != null)
            m_Material = new Material(Shader.Find("Hidden/Shader/TV_RLPRO_HDRP"));
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;

        // Pass all your usual properties
        m_Material.SetFloat("fade", Fade.value);
        m_Material.SetFloat("scale", scale.value);
        m_Material.SetFloat("hardScan", hardScan.value);
        m_Material.SetFloat("hardPix", hardPix.value);

        // If we do not scale with actual screen size, just use resScale.value
        // Otherwise we compute a ratio referencing 'referenceWidth'
        if (ScaleWithActualScreenSize.value)
        {
            float actualWidth = camera.camera.pixelWidth; // or Screen.width
            float ratio = (float)referenceWidth / actualWidth;
            // So if actualWidth=1920 => ratio=1 => scaler=resScale.value
            // if actualWidth=3840 => ratio=0.5 => scaler=resScale.value * 0.5
            scaler = resScale.value * ratio;
        }
        else
        {
            scaler = resScale.value;
        }

        if (mask.value != null)
        {
            m_Material.SetTexture(_Mask, mask.value);
            m_Material.SetFloat(_FadeMultiplier, 1);
            ParamSwitch(m_Material, maskChannel.value == maskChannelMode.alphaChannel, "ALPHA_CHANNEL");
        }
        else
        {
            m_Material.SetFloat(_FadeMultiplier, 0);
        }

        // Pass the final scale to your shader
        m_Material.SetFloat("resScale", scaler);

        // Also pass your other properties
        m_Material.SetFloat("maskDark", maskDark.value);
        m_Material.SetFloat("maskLight", maskLight.value);
        m_Material.SetVector("warp", warp.value);

        // Blit using pass 0 or 1 depending on warpMode
        cmd.Blit(
            source,
            destination,
            m_Material,
            warpMode == WarpMode.SimpleWarp ? 0 : 1
        );
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
    }

    private void ParamSwitch(Material mat, bool paramValue, string paramName)
    {
        if (paramValue) mat.EnableKeyword(paramName);
        else mat.DisableKeyword(paramName);
    }
}
