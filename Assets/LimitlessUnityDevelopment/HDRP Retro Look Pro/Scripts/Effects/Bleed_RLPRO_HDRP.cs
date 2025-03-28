using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using RetroLookPro.Enums;

// Define possible Bleed Modes for the effect
public enum BleedMode
{
    NTSCOld3Phase,
    NTSC3Phase,
    NTSC2Phase,
    customBleeding
}

// Create a VolumeParameter wrapper for our BleedMode enum
[Serializable]
public sealed class bleedModeParameter : VolumeParameter<BleedMode> { };

// Mark this class as a custom volume component under "Retro Look Pro/Bleed_RLPRO_HDRP"
[Serializable, VolumeComponentMenu(" Retro Look Pro/Bleed_RLPRO_HDRP")]
public sealed class Bleed_RLPRO_HDRP : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("NTSC Bleed modes.")]
    public bleedModeParameter bleedMode = new bleedModeParameter();

    [Tooltip("Bleed Stretch amount.")]
    public FloatParameter bleedAmount = new ClampedFloatParameter(0, 0, 15f);

    [Range(0, 50), Tooltip("Bleed Length.")]
    public IntParameter bleedLength = new IntParameter(0);

    [Tooltip("Debug bleed curve.")]
    public BoolParameter bleedDebug = new BoolParameter(false);

    [Tooltip("Enable this to edit curves and see result instantly (otherwise result will be applied after you enter playmode).")]
    public BoolParameter editCurves = new BoolParameter(false);

    [Tooltip("Synchronize Y and Q chanels.")]
    public BoolParameter syncYQ = new BoolParameter(false);

    [Tooltip("Curve Y chanel.")]
    public TextureCurveParameter curveY = new TextureCurveParameter(
        new TextureCurve(new AnimationCurve(), 1f, false, new Vector2(0.5f, 1f))
    );
    [Tooltip("Curve I chanel.")]
    public TextureCurveParameter curveI = new TextureCurveParameter(
        new TextureCurve(new AnimationCurve(), 1f, false, new Vector2(0.5f, 1f))
    );
    [Tooltip("Curve Q chanel.")]
    public TextureCurveParameter curveQ = new TextureCurveParameter(
        new TextureCurve(new AnimationCurve(), 1f, false, new Vector2(0.5f, 1f))
    );

    [Space]
    [Tooltip("Mask texture")]
    public TextureParameter mask = new TextureParameter(null);

    // Select which channel of the mask texture is used (R or Alpha).
    public maskChannelModeParameter maskChannel = new maskChannelModeParameter();

    [Tooltip("Manually control how strongly bleed scales with resolution.")]
    public FloatParameter resolutionFactor = new ClampedFloatParameter(1, 0, 4f);

    // Shader property IDs
    static readonly int _Mask = Shader.PropertyToID("_Mask");
    static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");

    // Utility index for referencing bleedMode if needed in the script
    public int bleedModeIndex;

    // Material that will use our Bleed_RLPRO_HDRP shader
    Material m_Material;

    // Maximum length for our color bleed curves
    int max_curve_length = 50;

    // Texture that stores precomputed curves for custom bleeding
    Texture2D texCurves = null;

    // Offset values for the Y, I, Q curves
    Vector4 curvesOffest = new Vector4(0, 0, 0, 0);

    // Internal array to hold curve data
    float[,] curvesData = new float[50, 3];

    // Determines if this effect should be active or not
    public bool IsActive()
    {
        // Currently always returns true, so the effect is always considered active
        return true;
    }

    // Defines at which point in the HDRP pipeline this effect is injected
    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    // Called once when the effect is created or reloaded
    public override void Setup()
    {
        // Try to find the specified shader, and create a material from it
        if (Shader.Find("Hidden/Shader/Bleed_RLPRO_HDRP") != null)
            m_Material = new Material(Shader.Find("Hidden/Shader/Bleed_RLPRO_HDRP"));

        // If we are in the 'customBleeding' mode, initialize our curves
        if (bleedMode.value == BleedMode.customBleeding)
        {
            Curves();
        }
    }

    // Called every frame to apply the effect
    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        // If our material isn't set up properly, exit
        if (m_Material == null)
            return;

        // If the bleed mode is 'customBleeding' (index == 3), and 'editCurves' is on, regenerate curves in real-time
        if ((int)bleedMode.value == 3)
        {
            if (editCurves.value)
                Curves();
        }

        // If we are in 'customBleeding' mode but the texture is null, create it
        if ((int)bleedMode.value == 3)
        {
            if (texCurves == null)
                Curves();
            m_Material.SetTexture("_CurvesTex", texCurves);
        }

        // If a mask texture is assigned, pass it to the shader and set the fade multiplier
        if (mask.value != null)
        {
            m_Material.SetTexture(_Mask, mask.value);
            m_Material.SetFloat(_FadeMultiplier, 1);
            // Switch the shader keyword based on which channel we use (R or Alpha)
            ParamSwitch(m_Material, maskChannel.value == maskChannelMode.alphaChannel, "ALPHA_CHANNEL");
        }
        else
        {
            // Otherwise, disable the mask effect by setting the fade multiplier to 0
            m_Material.SetFloat(_FadeMultiplier, 0);
        }

        // Get the camera's actual rendering width
        float actualWidth = camera.camera.pixelWidth;

        // Compute a width ratio relative to 1920 (our baseline resolution)
        float widthRatio = actualWidth / 1920;

        // Raise that ratio to 'resolutionFactor', letting you manually control how strongly bleed scales with resolution
        float scaleFactor = Mathf.Pow(widthRatio, resolutionFactor.value);

        // Multiply the base bleedAmount by this factor
        float finalBleed = bleedAmount.value * scaleFactor;

        // Pass final bleed value to the shader
        m_Material.SetFloat("bleedAmount", finalBleed);

        // Pass curve offsets, used in the custom bleeding logic
        m_Material.SetVector("curvesOffest", curvesOffest);

        // Set the number of taps / bleed length
        m_Material.SetFloat("bleedLength", bleedLength.value);

        // Enable or disable debug mode (VHS_DEBUG_BLEEDING_ON) in the shader
        ParamSwitch(m_Material, bleedDebug.value, "VHS_DEBUG_BLEEDING_ON");

        // A quick way to toggle the effect off if IsActive() == false
        m_Material.SetFloat("_Intensity", IsActive() ? 0 : 1);

        // Finally, execute a Blit:  
        // - Take 'source' (the camera's rendered image),
        // - Apply our material (with the selected pass from bleedMode),
        // - Output to 'destination' (the final image)
        cmd.Blit(source, destination, m_Material, (int)bleedMode.value);
    }

    // Called when the effect is destroyed or disabled
    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
    }

    // Utility to enable/disable a shader keyword
    private void ParamSwitch(Material mat, bool paramValue, string paramName)
    {
        if (paramValue)
            mat.EnableKeyword(paramName);
        else
            mat.DisableKeyword(paramName);
    }

    // Generates a 1D texture (texCurves) storing Y, I, Q curve data for custom bleeding
    private void Curves()
    {
        // Create the texture if it's null, 1 pixel tall and 'max_curve_length' wide
        if (texCurves == null)
            texCurves = new Texture2D(max_curve_length, 1, TextureFormat.RGBA32, false);

        // Reset curve offsets
        curvesOffest[0] = 0.0f;
        curvesOffest[1] = 0.0f;
        curvesOffest[2] = 0.0f;

        float t = 0.0f;

        // Populate curve data by sampling curveY, curveI, curveQ
        for (int i = 0; i < bleedLength.value; i++)
        {
            // 't' is an index in [0..bleedLength], scaled to 100 for the curve evaluation
            t = ((float)i) / ((float)bleedLength.value);
            t = (int)(t * 100);

            // Read the three channels from our custom curves
            curvesData[i, 0] = curveY.value.Evaluate(t);
            curvesData[i, 1] = curveI.value.Evaluate(t);
            curvesData[i, 2] = curveQ.value.Evaluate(t);

            // Optionally keep Q in sync with I if 'syncYQ' is enabled
            if (syncYQ.value)
                curvesData[i, 2] = curvesData[i, 1];

            // Track the minimum negative offset among Y, I, Q
            // (We'll offset them so they remain positive)
            if (curvesOffest[0] > curvesData[i, 0])
                curvesOffest[0] = curvesData[i, 0];
            if (curvesOffest[1] > curvesData[i, 1])
                curvesOffest[1] = curvesData[i, 1];
            if (curvesOffest[2] > curvesData[i, 2])
                curvesOffest[2] = curvesData[i, 2];
        }

        // Convert negative offsets to positive
        curvesOffest[0] = Mathf.Abs(curvesOffest[0]);
        curvesOffest[1] = Mathf.Abs(curvesOffest[1]);
        curvesOffest[2] = Mathf.Abs(curvesOffest[2]);

        // Shift the curve data by 'curvesOffest' so everything is in a safe range
        for (int i = 0; i < bleedLength.value; i++)
        {
            curvesData[i, 0] += curvesOffest[0];
            curvesData[i, 1] += curvesOffest[1];
            curvesData[i, 2] += curvesOffest[2];

            // We write each sample into texCurves at a specific pixel (x, y=0)
            // flipping i so that the data is reversed
            texCurves.SetPixel(
                -2 + bleedLength.value - i, // a custom mapping of i to x
                0,
                new Color(curvesData[i, 0], curvesData[i, 1], curvesData[i, 2])
            );
        }

        // Finally apply the changes to the texture
        texCurves.Apply();
    }
}
