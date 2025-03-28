using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using UnityEngine.Experimental.Rendering;
using RetroLookPro.Enums;


[Serializable, VolumeComponentMenu(" Retro Look Pro/Noise")]
public sealed class Noise_RLPRO : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f, true);
    [Tooltip("stretch Resolution")]
    public NoInterpFloatParameter stretchResolution = new NoInterpFloatParameter(480f);
    [Tooltip("Vertical Resolution")]
    public NoInterpFloatParameter VerticalResolution = new NoInterpFloatParameter(480f);
    [Space]
    [Space]
    [Tooltip("Granularity")]
    public BoolParameter Granularity = new BoolParameter(false);
    [Tooltip("Granularity Amount")]
    public NoInterpClampedFloatParameter GranularityAmount = new NoInterpClampedFloatParameter(0.5f, 0f, 0.5f);
    [Space]
    [Tooltip("Tape Noise")]
    public BoolParameter TapeNoise = new BoolParameter(false);
    [Tooltip("Tape Noise Signal Processing")]
    public NoInterpClampedFloatParameter TapeNoiseSignalProcessing = new NoInterpClampedFloatParameter(1f, 0f, 15f);
    [Tooltip("Tape Noise Fade")]
    public NoInterpClampedFloatParameter TapeNoiseFade = new NoInterpClampedFloatParameter(1f, 0f, 1.5f);
    [Tooltip("Tape Noise Amount(lower value = more noise)")]
    public NoInterpClampedFloatParameter TapeNoiseAmount = new NoInterpClampedFloatParameter(1f, 0f, 1.5f);
    [Tooltip("tape Lines Amount")]
    public NoInterpClampedFloatParameter tapeLinesAmount = new NoInterpClampedFloatParameter(0.8f, 0f, 1f);
    [Tooltip("Tape Noise Speed")]
    public NoInterpClampedFloatParameter TapeNoiseSpeed = new NoInterpClampedFloatParameter(0.5f, -1.5f, 1.5f);
    [Space]
    [Tooltip("Line Noise")]
    public BoolParameter LineNoise = new BoolParameter(false);
    [Tooltip("Line Noise Amount")]
    public NoInterpClampedFloatParameter LineNoiseAmount = new NoInterpClampedFloatParameter(1f, 0f, 15f);
    [Tooltip("Line Noise Speed")]
    public NoInterpClampedFloatParameter LineNoiseSpeed = new NoInterpClampedFloatParameter(1f, 0f, 10f);
    [Space]
    [Tooltip("Signal Noise")]
    public BoolParameter SignalNoise = new BoolParameter(false);
    [Tooltip("Signal Noise Power")]
    public NoInterpClampedFloatParameter SignalNoisePower = new NoInterpClampedFloatParameter(0.9f, 0.5f, 0.97f);
    [Tooltip("Signal Noise Amount")]
    public NoInterpClampedFloatParameter SignalNoiseAmount = new NoInterpClampedFloatParameter(1f, 0f, 2f);

    [Tooltip("Time.unscaledTime.")]
    public BoolParameter unscaledTime = new BoolParameter(false);
    [Space]
    [Tooltip("Mask texture")]
    public TextureParameter mask = new TextureParameter(null);
    public maskChannelModeParameter maskChannel = new maskChannelModeParameter();

    Material m_Material;
    private float _time;
    private RTHandle texTape = null;
    bool stop;
    static readonly int _Mask = Shader.PropertyToID("_Mask");
    static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");

    public bool IsActive() => m_Material != null && intensity.value > 0f;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

	public override void Setup()
	{
		if (Shader.Find("Hidden/Shader/NoiseEffects_RLPRO") != null)
			m_Material = new Material(Shader.Find("Hidden/Shader/NoiseEffects_RLPRO"));
		texTape = RTHandles.Alloc(Vector2.one, TextureXR.slices, colorFormat: GraphicsFormat.B10G11R11_UFloatPack32, dimension: TextureDimension.Tex2DArray, enableRandomWrite: true, useDynamicScale: true, name: "texLast");
		stop = false;
	}

	public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
	{
		if (m_Material == null)
			return;

		m_Material.SetFloat("_Intensity", intensity.value);

		m_Material.SetTexture("_InputTexture", source);

		if (unscaledTime.value) { _time = Time.unscaledTime; }
		else _time = Time.time;


		float screenLinesNum_ = stretchResolution.value;
		if (screenLinesNum_ <= 0) screenLinesNum_ = camera.actualHeight;

		if (!stop && (texTape.rt.height != Mathf.Min(VerticalResolution.value, screenLinesNum_)))
		{
			stop = true;
            cmd.Blit(source, texTape, m_Material,0);

            //HDUtils.DrawFullScreen(cmd, m_Material, texTape, shaderPassId: 0);
		}

		m_Material.SetFloat("time_", _time);
		m_Material.SetFloat("screenLinesNum", screenLinesNum_);
		m_Material.SetFloat("noiseLinesNum", VerticalResolution.value);
		m_Material.SetFloat("noiseQuantizeX", TapeNoiseSignalProcessing.value);
		ParamSwitch(m_Material, Granularity.value, "VHS_FILMGRAIN_ON");
		ParamSwitch(m_Material, TapeNoise.value, "VHS_TAPENOISE_ON");
		ParamSwitch(m_Material, LineNoise.value, "VHS_LINENOISE_ON");
		ParamSwitch(m_Material, SignalNoise.value, "VHS_YIQNOISE_ON");

		m_Material.SetFloat("signalNoisePower", SignalNoisePower.value);
		m_Material.SetFloat("signalNoiseAmount", SignalNoiseAmount.value);
		m_Material.SetFloat("tapeLinesAmount", 1 - tapeLinesAmount.value);
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

		m_Material.SetFloat("filmGrainAmount", GranularityAmount.value);

		ParamSwitch(m_Material, TapeNoise.value, "VHS_TAPENOISE_ON");
		m_Material.SetFloat("tapeNoiseTH", TapeNoiseAmount.value);
		m_Material.SetFloat("tapeNoiseAmount", TapeNoiseFade.value);
		m_Material.SetFloat("tapeNoiseSpeed", TapeNoiseSpeed.value);
		m_Material.SetFloat("lineNoiseAmount", LineNoiseAmount.value);
		m_Material.SetFloat("lineNoiseSpeed", LineNoiseSpeed.value);

		//HDUtils.DrawFullScreen(cmd, m_Material, texTape, shaderPassId: 1);
        cmd.Blit(source, texTape, m_Material, 1);


        m_Material.SetTexture("_TapeTex", texTape);

        cmd.Blit(source, destination, m_Material, 0);
		//texTape.rt.Release();
	}
	private void ParamSwitch(Material mat, bool paramValue, string paramName)
	{
		if (paramValue) mat.EnableKeyword(paramName);
		else mat.DisableKeyword(paramName);
	}
	public override void Cleanup()
	{
		CoreUtils.Destroy(m_Material);
		 RTHandles.Release(texTape);

	}
}
