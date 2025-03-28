using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Experimental.Rendering;
using System;

[Serializable, VolumeComponentMenu("/Retro Look Pro/Artefacts")]
public sealed class Artefacts_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
    [Range(0f, 1f), Tooltip("Brightness threshold of input.")]
    public ClampedFloatParameter cutOff = new ClampedFloatParameter(0.5f, 0f, 1f);
    [Range(0f, 3f), Tooltip("Amplifies input amount after cutoff.")]
    public ClampedFloatParameter amount = new ClampedFloatParameter(1f, 0f, 3f);
    [Range(0f, 1f), Tooltip("Value represents how fast trail fades.")]
    public ClampedFloatParameter fade = new ClampedFloatParameter(0.5f, 0f, 1f);
    [Tooltip("Artefacts color.")]
    public ColorParameter color = new ColorParameter(new Color());
    [Tooltip("Render Artefacts only.")]
    public BoolParameter debugArtefacts = new BoolParameter(false);
    //
    Material m_Material;
    RTHandle texLast = null;
    RTHandle texfeedback = null;
    RTHandle texfeedback2 = null;
    RTHandle previous = null;
    public bool IsActive() => m_Material != null && intensity.value > 0f;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        if (Shader.Find("Hidden/Shader/ArtefactsEffect_RLPRO") != null)
            m_Material = new Material(Shader.Find("Hidden/Shader/ArtefactsEffect_RLPRO"));
        texLast = RTHandles.Alloc(Vector2.one, TextureXR.slices, colorFormat: GraphicsFormat.B10G11R11_UFloatPack32, dimension: TextureDimension.Tex2DArray, enableRandomWrite: true, useDynamicScale: true, name: "texLast");
        texfeedback = RTHandles.Alloc(Vector2.one, TextureXR.slices, colorFormat: GraphicsFormat.B10G11R11_UFloatPack32, dimension: TextureDimension.Tex2DArray, enableRandomWrite: true, useDynamicScale: true, name: "texfeedback");
        texfeedback2 = RTHandles.Alloc(Vector2.one, TextureXR.slices, colorFormat: GraphicsFormat.B10G11R11_UFloatPack32, dimension: TextureDimension.Tex2DArray, enableRandomWrite: true, useDynamicScale: true, name: "texfeedback2");
        previous = RTHandles.Alloc(Vector2.one, TextureXR.slices, colorFormat: GraphicsFormat.B10G11R11_UFloatPack32, dimension: TextureDimension.Tex2DArray, enableRandomWrite: true, useDynamicScale: true, name: "previous");
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;

        m_Material.SetTexture("_LastTex", camera.GetPreviousFrameRT(2));
        m_Material.SetTexture("_FeedbackTex", texfeedback);
        m_Material.SetFloat("feedbackThresh", cutOff.value);
        m_Material.SetFloat("feedbackAmount", amount.value);
        m_Material.SetFloat("feedbackFade", fade.value);
        m_Material.SetColor("feedbackColor", color.value);
        m_Material.SetFloat("_Intensity", intensity.value);
        cmd.Blit(source, texfeedback2, m_Material, 0);

        m_Material.SetTexture("_InputTexture2", texfeedback2);
        cmd.Blit(source, texfeedback, m_Material, 2);

        m_Material.SetTexture("_FeedbackTex3", texfeedback);
        m_Material.SetTexture("_InputTexture4", source);
        cmd.Blit(source, texLast, m_Material, 1);

        if (!debugArtefacts.value)
        {
            m_Material.SetTexture("_InputTexture3", texLast);
        }
        else
        {
            m_Material.SetTexture("_InputTexture3", texfeedback);
        }
        cmd.Blit(source, destination, m_Material, 3);
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
        RTHandles.Release(texLast);
        RTHandles.Release(texfeedback2);
        RTHandles.Release(texfeedback);
        RTHandles.Release(previous);
    }
}
