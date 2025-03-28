using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using RetroLookPro.Enums;
using UnityEngine.Experimental.Rendering;
using LimitlessDev.RetroLookPro;

[Serializable]
public sealed class resModeParameter : VolumeParameter<ResolutionMode> { };
[Serializable]
public sealed class Vector2IntParameter : VolumeParameter<Vector2Int> { };
[Serializable]
public sealed class preLParameter : VolumeParameter<effectPresets> { };

[Serializable, VolumeComponentMenu(" Retro Look Pro/Colormap Palette")]
public sealed class ColormapPalette_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Range(0f, 1f), Tooltip("Opacity.")]
    public ClampedFloatParameter Opacity = new ClampedFloatParameter(0f, 0f, 1f);

    [Tooltip("Pixelation factor at the reference width.")]
    public ClampedIntParameter pixelSize = new ClampedIntParameter(1, 1, 10);

    [Range(0f, 1f), Tooltip("Dithering effect.")]
    public ClampedFloatParameter dither = new ClampedFloatParameter(1f, 0f, 1f);

    public preLParameter presetsList = new preLParameter { };
    public IntParameter presetIndex = new IntParameter(0);

    [Tooltip("Dither texture.")]
    public TextureParameter bluenoise = new TextureParameter(null);

    [Space, Tooltip("Mask texture")]
    public TextureParameter mask = new TextureParameter(null);
    public maskChannelModeParameter maskChannel = new maskChannelModeParameter();

    static readonly int _Mask = Shader.PropertyToID("_Mask");
    static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");

    Material m_Material;

    public int tempPresetIndex = 0;
    private bool m_Init;

    Texture2D colormapPalette;
    Texture3D colormapTexture;

    private Vector2 m_Res;

    // =========================
    // NEW: referenceWidth
    // =========================
    [Tooltip("Screen width at which pixelSize is interpreted literally. Higher or lower widths auto-adjust pixelSize.")]
    public int referenceWidth = 1920;

    // We'll track the final adjusted pixel size and width so we know when to re-alloc RTs
    private float m_CurrentPixelSize;
    private float m_CurrentWidth;

    // The 'lowresTexture' we downsample into
    RTHandle lowresTexture;

    public bool IsActive() => m_Material != null && Opacity.value > 0f;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        if (Shader.Find("Hidden/Shader/ColormapPaletteEffect_RLPRO") != null)
            m_Material = new Material(Shader.Find("Hidden/Shader/ColormapPaletteEffect_RLPRO"));

        m_Init = true;

        // Initialize once. We'll also do a re-alloc check in Render() in case resolution changes.
        // For now, we guess some default "width" = referenceWidth.
        AllocateLowresRT(ComputeAdjustedPixelSize(referenceWidth));
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;

        // 1) Update the material with the new screen-based variables
        ApplyMaterialVariables(m_Material, out m_Res);

        // 2) Check if the user changed the preset, or pixelSize, etc.
        if (m_Init || intHasChanged(tempPresetIndex, presetIndex.value))
        {
            tempPresetIndex = presetIndex.value;
            ApplyColormapToMaterial(m_Material);
            m_Init = false;
        }

        // 3) Now see if the user changed pixelSize, or the screen resolution changed 
        //    => we re-allocate the RT with the correct scale.
        float actualWidth = camera.camera.pixelWidth;
        float adjustedPxSize = ComputeAdjustedPixelSize(actualWidth);

        // If pixelSize or actualWidth changed => re-alloc
        if (!Mathf.Approximately(m_CurrentPixelSize, adjustedPxSize))
        {
            RTHandles.Release(lowresTexture);
            AllocateLowresRT(adjustedPxSize);
        }

        // 4) If there's a mask
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

        // 5) PASS 0: downsample from 'source' into 'lowresTexture'
        cmd.Blit(source, lowresTexture, m_Material, 0);

        // 6) PASS 2: final composite pass (index 2 in your shader)
        //    (Pass 1 is presumably a dithering pass, but your code calls pass 2 after pass 0. 
        //    We'll keep it the same.)
        m_Material.SetVector("Resolution", new Vector4(m_Res.x, m_Res.y, 0, 0));
        m_Material.SetTexture("_InputTexture3", lowresTexture);

        cmd.Blit(source, destination, m_Material, 2);
    }

    // Creates or re-creates the 'lowresTexture' with the appropriate scale factor
    void AllocateLowresRT(float adjustedPxSize)
    {
        // We'll keep track of the final "adjusted pixel size" so we don't re-alloc unnecessarily
        m_CurrentPixelSize = adjustedPxSize;

        // The scale factor is just 1 / adjustedPxSize in both X & Y. 
        // If adjustedPxSize=2 => half resolution. If =4 => quarter resolution, etc.
        float scale = 1f / adjustedPxSize;

        lowresTexture = RTHandles.Alloc(
            scaleFactor: new Vector2(scale, scale),
            slices: TextureXR.slices,
            colorFormat: GraphicsFormat.B10G11R11_UFloatPack32,
            dimension: TextureDimension.Tex2DArray,
            enableRandomWrite: true,
            useDynamicScale: true,
            name: "lowresTexture"
        );
    }

    // Adjust the user-specified pixelSize to maintain the same final “pixel count” across resolutions
    float ComputeAdjustedPixelSize(float actualWidth)
    {
        // If the user sets pixelSize=2 at referenceWidth=1920 => final = 1920/2=960 px wide
        // At 3840 wide, ratio=2 => we want the final px count still ~960 => so we do 3840/4=960 => pixelSize=4
        // => adjustedPixelSize = (pixelSize) * ratio
        float ratio = actualWidth / referenceWidth;
        float adjusted = pixelSize.value * ratio;

        // clamp if you want to avoid extremes
        float clamped = Mathf.Clamp(adjusted, 1f, 9999f);
        return clamped;
    }

    // Called when the effect is destroyed or disabled
    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
        RTHandles.Release(lowresTexture);
    }

    private void ParamSwitch(Material mat, bool paramValue, string paramName)
    {
        if (paramValue) mat.EnableKeyword(paramName);
        else mat.DisableKeyword(paramName);
    }

    public void ApplyMaterialVariables(Material bl, out Vector2 res)
    {
        // Original logic: res.x= Screen.width / pixelSize.value; but now we have an "adjusted" pixel size
        // We'll compute the final "virtual" resolution after adjusting
        float actualWidth = Screen.width;
        float adjustedPxSize = ComputeAdjustedPixelSize(actualWidth);

        // Final resolution in X & Y
        res.x = (Screen.width / adjustedPxSize);
        res.y = (Screen.height / adjustedPxSize);

        Opacity.value = Mathf.Clamp01(Opacity.value);
        dither.value = Mathf.Clamp01(dither.value);

        bl.SetFloat("_Dither", dither.value);
        bl.SetFloat("_Opacity", Opacity.value);
    }

    public void ApplyColormapToMaterial(Material bl)
    {
        if (presetsList.value != null)
        {
            if (bluenoise.value != null)
            {
                bl.SetTexture("_BlueNoise", bluenoise.value);
            }
            ApplyPalette(bl);
            ApplyMap(bl);
        }
    }

    void ApplyPalette(Material bl)
    {
        colormapPalette = new Texture2D(256, 1, TextureFormat.RGB24, false);
        colormapPalette.filterMode = FilterMode.Point;
        colormapPalette.wrapMode = TextureWrapMode.Clamp;

        for (int i = 0; i < presetsList.value.presetsList[presetIndex.value].preset.numberOfColors; ++i)
        {
            colormapPalette.SetPixel(i, 0, presetsList.value.presetsList[presetIndex.value].preset.palette[i]);
        }

        colormapPalette.Apply();
        bl.SetTexture("_Palette", colormapPalette);
    }

    public void ApplyMap(Material bl)
    {
        int colorsteps = 64;
        colormapTexture = new Texture3D(colorsteps, colorsteps, colorsteps, TextureFormat.RGB24, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        colormapTexture.SetPixels32(presetsList.value.presetsList[presetIndex.value].preset.pixels);
        colormapTexture.Apply();
        bl.SetTexture("_Colormap", colormapTexture);
    }

    public bool intHasChanged(int A, int B)
    {
        bool result = false;
        if (B != A)
        {
            A = B;
            result = true;
        }
        return result;
    }
}
