using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using RetroLookPro.Enums;

[Serializable, VolumeComponentMenu(" Retro Look Pro/Old Film")]
public sealed class OldFilm_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Effect fade.")]
    public ClampedFloatParameter Fade = new ClampedFloatParameter(0f, 0f, 1f, true);
    [Range(0f, 60f), Tooltip("Frames per second.")]
    public NoInterpClampedFloatParameter fps = new NoInterpClampedFloatParameter(1f, 0f, 60f);
    [Range(0f, 5f), Tooltip(".")]
    public NoInterpClampedFloatParameter contrast = new NoInterpClampedFloatParameter(1f, 0f, 5f);

    [Range(-2f, 4f), Tooltip("Image burn.")]
    public NoInterpClampedFloatParameter burn = new NoInterpClampedFloatParameter(0.88f, -2f, 4f);
    [Range(0f, 16f), Tooltip("Scene cut off.")]
    public NoInterpClampedFloatParameter sceneCut = new NoInterpClampedFloatParameter(0.88f, 0f, 16f);
    [Space]
    [Tooltip("Mask texture")]
    public TextureParameter mask = new TextureParameter(null);
    public maskChannelModeParameter maskChannel = new maskChannelModeParameter();
    static readonly int _Mask = Shader.PropertyToID("_Mask");
    static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");

    Material m_Material;
    private float T;

    public bool IsActive() => m_Material != null && Fade.value > 0f;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        if (Shader.Find("Hidden/Shader/OldFilmEffect_RLPRO") != null)
            m_Material = new Material(Shader.Find("Hidden/Shader/OldFilmEffect_RLPRO"));
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;
		T += Time.deltaTime;
		if (T > 100) T = 0;		
		m_Material.SetFloat("T", T);
		m_Material.SetFloat("FPS",  fps.value);
		m_Material.SetFloat("Contrast",  contrast.value);
		m_Material.SetFloat("Burn",  burn.value);
		m_Material.SetFloat("SceneCut",  sceneCut.value);
		m_Material.SetFloat("Fade",  Fade.value);
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
