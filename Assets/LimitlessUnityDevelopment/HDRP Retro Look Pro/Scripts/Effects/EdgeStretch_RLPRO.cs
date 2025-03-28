using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Retro Look Pro/Edge Stretch")]
public sealed class EdgeStretch_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
    public BoolParameter left = new BoolParameter(false);
    public BoolParameter right = new BoolParameter(false);
    public BoolParameter top = new BoolParameter(false);
    public BoolParameter bottom = new BoolParameter(true);

    [Tooltip("Height of Noise.")]
    public NoInterpClampedFloatParameter height = new NoInterpClampedFloatParameter(0.2f, 0.01f, 0.5f);
    [Space]
    [Tooltip("Stretch noise distortion.")]
    public BoolParameter distort = new BoolParameter(true);
    [Tooltip("Noise distortion frequency.")]
    public NoInterpClampedFloatParameter frequency = new NoInterpClampedFloatParameter(0.2f, 0.1f, 100f);
    [Tooltip("Noise distortion amplitude.")]
    public NoInterpClampedFloatParameter amplitude = new NoInterpClampedFloatParameter(0.2f, 0.0f, 0.5f);
    [Tooltip("Noise distortion speed.")]
    public NoInterpClampedFloatParameter speed = new NoInterpClampedFloatParameter(0.2f, 0.0f, 50f);
    [Tooltip("Enable noise distortion random frequency.")]
    public BoolParameter distortRandomly = new BoolParameter(true);
    //
    Material m_Material;
    private float T;
    public bool IsActive() => m_Material != null && intensity.value > 0f;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

	public override void Setup()
	{
		if (Shader.Find("Hidden/Shader/EdgeStretchEffect_RLPRO") != null)
			m_Material = new Material(Shader.Find("Hidden/Shader/EdgeStretchEffect_RLPRO"));
	}

	public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
	{
		if (m_Material == null)
			return;
		T += Time.deltaTime;
		m_Material.SetFloat("Time", T);
		m_Material.SetFloat("_NoiseBottomHeight", height.value);
		m_Material.SetFloat("frequency", frequency.value);
		m_Material.SetFloat("amplitude", amplitude.value);
		m_Material.SetFloat("_Intensity", intensity.value);
		m_Material.SetFloat("speed", speed.value);

		m_Material.SetTexture("_InputTexture", source);
		ParamSwitch(m_Material, top.value, "top_ON");
		ParamSwitch(m_Material, bottom.value, "bottom_ON");
		ParamSwitch(m_Material, left.value, "left_ON");
		ParamSwitch(m_Material, right.value, "right_ON");
		if (distort.value)
            cmd.Blit(source, destination, m_Material, distortRandomly.value ? 1 : 0);
        else
		cmd.Blit(source, destination, m_Material, 2);

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
