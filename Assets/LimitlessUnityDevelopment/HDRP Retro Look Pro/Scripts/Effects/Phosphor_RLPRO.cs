using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using UnityEngine.Experimental.Rendering;
using RetroLookPro.Enums;

[Serializable, VolumeComponentMenu(" Retro Look Pro/Phosphor")]
public sealed class Phosphor_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    public ClampedFloatParameter Fade = new ClampedFloatParameter(0f, 0f, 1f, true);
    public NoInterpClampedFloatParameter width = new NoInterpClampedFloatParameter(0.4f, 0f, 20f);
    [Space]
    [Tooltip("Mask texture")]
    public TextureParameter mask = new TextureParameter(null);
    public maskChannelModeParameter maskChannel = new maskChannelModeParameter();
    static readonly int _Mask = Shader.PropertyToID("_Mask");
    static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");

    private RTHandle texTape = null;
    bool stop;
    Material m_Material;
    float T;
    public bool IsActive() => m_Material != null && Fade.value > 0f;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        if (Shader.Find("Hidden/Shader/Phosphor_RLPRO") != null)
            m_Material = new Material(Shader.Find("Hidden/Shader/Phosphor_RLPRO"));

		texTape = RTHandles.Alloc(Vector2.one, TextureXR.slices, colorFormat: GraphicsFormat.B10G11R11_UFloatPack32, dimension: TextureDimension.Tex2DArray, enableRandomWrite: true, useDynamicScale: true, name: "texLast");//RTHandles.Alloc(texWidth, texHeight, TextureXR.slices, colorFormat: GraphicsFormat.B10G11R11_UFloatPack32, dimension: TextureDimension.Tex2DArray, enableRandomWrite: true, useDynamicScale: true, name: "previous");
		stop = false;
	}

	public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;

        if (!stop)
        {
            stop = true;
            cmd.Blit(source, texTape, m_Material, 1);
        }
        cmd.Blit(source, texTape, m_Material, 1);
        m_Material.SetTexture("_Tex", texTape);
        T = Time.time;
        m_Material.SetFloat("T", T);
        m_Material.SetFloat("speed", width.value);
        m_Material.SetFloat("fade", Fade.value);
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

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
		RTHandles.Release(texTape);
	}
    private void ParamSwitch(Material mat, bool paramValue, string paramName)
    {
        if (paramValue) mat.EnableKeyword(paramName);
        else mat.DisableKeyword(paramName);
    }

}
