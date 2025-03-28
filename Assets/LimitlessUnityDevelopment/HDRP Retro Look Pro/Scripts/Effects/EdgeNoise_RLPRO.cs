using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Retro Look Pro/Edge Noise")]
public sealed class EdgeNoise_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
	[Tooltip("Controls the intensity of the effect.")]
	public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f,true);
	public BoolParameter left = new BoolParameter(false);
	public BoolParameter right = new BoolParameter(false);
	public BoolParameter top = new BoolParameter(false);
	public BoolParameter bottom = new BoolParameter(true);
	[Range(0.01f, 0.5f), Tooltip("Noise Height.")]
	public NoInterpClampedFloatParameter height = new NoInterpClampedFloatParameter(0.2f, 0.01f, 0.5f);
	[Tooltip("Noise tiling.")]
	public NoInterpVector2Parameter tile = new NoInterpVector2Parameter(new Vector2(1, 1));


	public TextureParameter noiseTexture = new TextureParameter(null);
	Material m_Material;

	public bool IsActive() => m_Material != null && intensity.value > 0f;

	public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

	public override void Setup()
	{
		if (Shader.Find("Hidden/Shader/EdgeNoiseEffect_RLPRO") != null)
			m_Material = new Material(Shader.Find("Hidden/Shader/EdgeNoiseEffect_RLPRO"));
	}

	public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
	{
		if (m_Material == null)
			return;

		if (m_Material.HasProperty("_OffsetNoiseY"))
		{
			float offsetNoise1 = m_Material.GetFloat("_OffsetNoiseY");
			m_Material.SetFloat("_OffsetNoiseY", offsetNoise1 + UnityEngine.Random.Range(-0.05f, 0.05f));
		}
		m_Material.SetFloat("_OffsetNoiseX", UnityEngine.Random.Range(0f, 1.0f));

		m_Material.SetFloat("_NoiseBottomHeight", height.value);

		m_Material.SetFloat("_NoiseBottomIntensity", intensity.value);
		if (noiseTexture.value != null)
		{
			m_Material.SetTexture("_NoiseTexture", noiseTexture.value);
		}
		ParamSwitch(m_Material, top.value, "top_ON");
		ParamSwitch(m_Material, bottom.value, "bottom_ON");
		ParamSwitch(m_Material, left.value, "left_ON");
		ParamSwitch(m_Material, right.value, "right_ON");

		m_Material.SetFloat("tileX", tile.value.x);
		m_Material.SetFloat("tileY", tile.value.y);
		m_Material.SetFloat("_Intensity", intensity.value);
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
