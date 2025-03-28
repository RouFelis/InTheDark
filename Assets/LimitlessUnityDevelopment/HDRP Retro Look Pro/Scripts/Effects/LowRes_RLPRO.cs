using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Experimental.Rendering;
using System;
using RetroLookPro.Enums;

[Serializable, VolumeComponentMenu(" Retro Look Pro/LowRes")]
public sealed class LowRes_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

    [Tooltip("Lower value = more downscale. (0.5 means half width at the reference resolution.)")]
    public ClampedFloatParameter downscale = new ClampedFloatParameter(0.5f, 0.01f, 1f);

    [Tooltip("Width at which 'downscale' is exactly as user-specified. Larger or smaller screens auto-adjust.")]
    public int referenceWidth = 1920;

    [Tooltip("Mask texture")]
    public TextureParameter mask = new TextureParameter(null);
    public maskChannelModeParameter maskChannel = new maskChannelModeParameter();

    static readonly int _Mask = Shader.PropertyToID("_Mask");
    static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");

    Material m_Material;

    // We store these RTHandles as "lowresTexture" and "highresTexture"
    RTHandle lowresTexture;
    RTHandle highresTexture;

    // We track the actual allocated scale so we can detect changes
    float m_CurrentDownscale;
    float m_CurrentWidth;

    public bool IsActive() => m_Material != null && intensity.value > 0f;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        if (Shader.Find("Hidden/Shader/LowResolution_RLPRO") != null)
            m_Material = new Material(Shader.Find("Hidden/Shader/LowResolution_RLPRO"));

        // Initialize RTHandles based on referenceWidth for now,
        // but we’ll re-check in Render if the user’s screen or param changes
        AllocateRTs(ComputeAdjustedDownscale(referenceWidth), referenceWidth);
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;

        // 1) Compute the user’s adjusted downscale for the *current* resolution
        float actualWidth = camera.camera.pixelWidth;
        float adjustedDownscale = ComputeAdjustedDownscale(actualWidth);

        // 2) If downscale changed or actual width changed, reallocate
        if (!Mathf.Approximately(m_CurrentDownscale, adjustedDownscale)
            || !Mathf.Approximately(m_CurrentWidth, actualWidth))
        {
            // Release old RTs
            RTHandles.Release(highresTexture);
            RTHandles.Release(lowresTexture);

            // Allocate new RTs with the updated scale
            AllocateRTs(adjustedDownscale, actualWidth);
        }

        // 3) Mask logic as before
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

        // 4) Pass intensity
        m_Material.SetFloat("_Intensity", intensity.value);

        // 5) Downsample pass
        cmd.Blit(source, lowresTexture, m_Material, 0);

        // 6) Upsample pass
        m_Material.SetTexture("_InputTexture2", lowresTexture);
        m_Material.SetFloat("downsample", adjustedDownscale);
        cmd.Blit(lowresTexture, highresTexture, m_Material, 1);

        // 7) Final composite pass
        m_Material.SetTexture("_InputTexture3", highresTexture);
        m_Material.SetFloat("downsample", adjustedDownscale);
        cmd.Blit(source, destination, m_Material, 2);
    }

    // Called when the effect is destroyed or disabled
    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
        RTHandles.Release(highresTexture);
        RTHandles.Release(lowresTexture);
    }

    // Allocates RTHandles for the given adjustedDownscale 
    private void AllocateRTs(float adjustedDownscale, float actualWidth)
    {
        // Save them so we can detect changes next frame
        m_CurrentDownscale = adjustedDownscale;
        m_CurrentWidth = actualWidth;

        // We create a "lowresTexture" with 'adjustedDownscale' of the *full screen*
        // (i.e. if adjustedDownscale=0.5 at 1920 wide => we do half-size in each dimension)
        lowresTexture = RTHandles.Alloc(
            scaleFactor: new Vector2(adjustedDownscale, adjustedDownscale),
            slices: TextureXR.slices,
            colorFormat: GraphicsFormat.B10G11R11_UFloatPack32,
            dimension: TextureDimension.Tex2DArray,
            enableRandomWrite: true,
            useDynamicScale: true,
            name: "lowresTexture"
        );

        // The "highresTexture" is a full-size buffer (scaleFactor=1,1)
        highresTexture = RTHandles.Alloc(
            scaleFactor: Vector2.one,
            slices: TextureXR.slices,
            colorFormat: GraphicsFormat.B10G11R11_UFloatPack32,
            dimension: TextureDimension.Tex2DArray,
            enableRandomWrite: true,
            useDynamicScale: true,
            name: "highresTexture"
        );
    }

    // Takes the user-specified 'downscale' (e.g., 0.5) as correct for 'referenceWidth'.
    // If actualWidth is bigger, we reduce the fraction so final pixel count is the same.
    private float ComputeAdjustedDownscale(float actualWidth)
    {
        // For example, if downscale=0.5 at referenceWidth=1920 => 960 px wide
        // At 3840 wide, ratio=2 => adjustedDownscale=0.5 / 2=0.25 => 960 px wide
        float ratio = actualWidth / referenceWidth;
        float rawAdjusted = downscale.value / ratio;
        // clamp to something >0
        return Mathf.Clamp(rawAdjusted, 0.01f, 1f);
    }

    private void ParamSwitch(Material mat, bool paramValue, string paramName)
    {
        if (paramValue) mat.EnableKeyword(paramName);
        else mat.DisableKeyword(paramName);
    }
}
