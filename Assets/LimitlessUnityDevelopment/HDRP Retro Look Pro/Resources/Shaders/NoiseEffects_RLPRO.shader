Shader "Hidden/Shader/NoiseEffects_RLPRO"
{
	Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }

	HLSLINCLUDE

	#pragma target 4.5
	#pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
 

	struct Attributes
	{
		uint vertexID : SV_VertexID;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct Varyings
	{
		float4 positionCS : SV_POSITION;
		float2 texcoord   : TEXCOORD0;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	Varyings Vert(Attributes input)
	{
		Varyings output;
		UNITY_SETUP_INSTANCE_ID(input);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
		output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
		output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
		return output;
	}

	#pragma shader_feature VHS_FILMGRAIN_ON
	#pragma shader_feature VHS_LINENOISE_ON
	#pragma shader_feature VHS_TAPENOISE_ON
	#pragma shader_feature VHS_YIQNOISE_ON

	TEXTURE2D_X(_MainTex);
	TEXTURE2D_X(_TapeTex);
	TEXTURE2D(_Mask);
	SAMPLER(sampler_Mask);
	float _FadeMultiplier;
	#pragma shader_feature ALPHA_CHANNEL

	float _Intensity;
	
	float screenLinesNum = 240.0;
	half tapeLinesAmount;
	float noiseLinesNum = 240.0;
	float noiseQuantizeX = 1.0;
	float tapeNoiseAmount = 1.0;
	float signalNoisePower = 1.0f;
	float signalNoiseAmount = 1.0f;
	float time_ = 0.0;
	float SLN = 0.0;
	float SLN_Noise = 0.0;
	float ONE_X = 0.0;
	float ONE_Y = 0.0;
	half bms(half c1, half c2) { return 1.0 - (1.0 - c1) * (1.0 - c2); }
	float onOff(float a, float b, float c, float t) {return step(c, sin(t + a * cos(t * b)));}

	#if VHS_YIQNOISE_ON

		#define MOD3 float3(443.8975,397.2973, 491.1871)

		float2 hash22(float2 p) 
		{
			float3 p3 = frac(float3(p.xyx) * MOD3);
			p3 += dot(p3.zxy, p3.yzx + 19.19);
			return frac(float2((p3.x + p3.y) * p3.z, (p3.x + p3.z) * p3.y));
		}
		float2 n4rand_bw(float2 p, float t, float c) 
		{
			t = frac(t);
			float2 nrnd0 = hash22(p + 0.07 * t);
			c = 1.0 / (10.0 * c);
			nrnd0 = pow(nrnd0, c);
			return nrnd0;
		}
	#endif

	half3 rgb2yiq(half3 c) 
	{
		return half3(
			(0.2989 * c.x + 0.5959 * c.y + 0.2115 * c.z),
			(0.5870 * c.x - 0.2744 * c.y - 0.5229 * c.z),
			(0.1140 * c.x - 0.3216 * c.y + 0.3114 * c.z)
			);
	};

	half3 yiq2rgb(half3 c) 
	{
		return half3(
			(1.0 * c.x + 1.0 * c.y + 1.0 * c.z),
			(0.956 * c.x - 0.2720 * c.y - 1.1060 * c.z),
			(0.6210 * c.x - 0.6474 * c.y + 1.7046 * c.z)
			);
	};

	//TapePass
	float tapeNoiseTH = 0.7;
	float tapeNoiseSpeed = 1.0;
	float filmGrainAmount = 16.0;
	float filmGrainPower = 10.0;
	float lineNoiseAmount = 1.0;
	float lineNoiseSpeed = 5.0;
	#define MOD3 float3(443.8975,397.2973, 491.1871)

	float hash12(float2 p) 
	{
		float3 p3 = frac(float3(p.xyx) * MOD3);
		p3 += dot(p3, p3.yzx + 19.19);
		return frac(p3.x * p3.z * p3.y);
	}
	float hash(float n) { return frac(sin(n) * 43758.5453123); }

	float niq(in float3 x) 
	{
		float3 p = floor(x);
		float3 f = frac(x);
		f = f * f * (3.0 - 2.0 * f);
		float n = p.x + p.y * 57.0 + 113.0 * p.z;
		float res = lerp(lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x),
			lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y),
			lerp(lerp(hash(n + 113.0), hash(n + 114.0), f.x),
				lerp(hash(n + 170.0), hash(n + 171.0), f.x), f.y), f.z);
		return res;
	}

	float tapeNoiseLines(float2 p, float t) 
	{
		float y = p.y * _ScreenSize.y;
		float s = t * 2.0;
		return  	(niq(float3(y * 0.01 + s, 1.0, 1.0)) + 0.0)
			* (niq(float3(y * 0.011 + 1000.0 + s, 1.0, 1.0)) + 0.0)
			* (niq(float3(y * 0.51 + 421.0 + s, 1.0, 1.0)) + 0.0)
			;
	}

	float tapeNoise(float nl, float2 p, float t) 
	{
		float nm = hash12(frac(p + t * float2(0.234, 0.637)));
		nm = nm * nm * nm * nm + 0.3;
		nl *= nm;
		if (nl < tapeNoiseTH) nl = 0.0; else nl = 1.0;
		return nl;
	}

	#if VHS_LINENOISE_ON
		float rnd_rd(float2 co) 
		{
			float a = 12.9898;
			float b = 78.233;
			float c = 43758.5453;
			float dt = dot(co.xy, float2(a, b));
			float sn = fmod(dt, 3.14);
			return frac(sin(sn) * c);
		}
	
		float rndln(float2 p, float t) 
		{
			float sample = rnd_rd(float2(1.0, 2.0 * cos(t)) * t * 8.0 + p * 1.0).x;
			sample *= sample;
			return sample;
		}
		float lineNoise(float2 p, float t) 
		{
			float n = rndln(p * float2(0.5, 1.0) + float2(1.0, 3.0), t);
			float freq = abs(sin(t));
			float c = n * smoothstep(fmod(p.y * 4.0 + t / 2.0 + sin(t + sin(t * 0.3)), freq), 0, 0.95);
			return c;
		}
	#endif

	#if VHS_FILMGRAIN_ON
		float filmGrain(float2 uv, float t, float c) 
		{
			float nr = hash12(uv + 0.07 * frac(t));
			return nr * nr * nr;
		}
	#endif
	
	float4 NoiseFrag(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		float t = time_;
		
		float2 UV = i.texcoord;
        float2 positionSS = UV ;

		float2 p = UV;
		if (screenLinesNum == 0.0) screenLinesNum = _ScreenSize.y;
		SLN = screenLinesNum;
		SLN_Noise = noiseLinesNum;
		if (SLN_Noise == 0 || SLN_Noise > SLN) SLN_Noise = SLN;
		ONE_X = 1.0 / _ScreenSize.x;
		ONE_Y = 1.0 / _ScreenSize.y;
		half3 col = half3(0.0,0.0,0.0);
		half3 signal = half3(0.0,0.0,0.0);
		float2 pn = p;
		if (SLN != SLN_Noise)
		{
			#if VHS_LINESFLOAT_ON
			float sh = frac(t);
			pn.y = floor(pn.y * SLN_Noise + sh) / SLN_Noise - sh / SLN_Noise;
			#else 
			pn.y = floor(pn.y * SLN_Noise) / SLN_Noise;
			#endif				 
		}
		float ScreenLinesNumX = SLN_Noise * _ScreenSize.x / _ScreenSize.y;
		float SLN_X = noiseQuantizeX * (_ScreenSize.x - ScreenLinesNumX) + ScreenLinesNumX;
		pn.x = floor(pn.x * SLN_X) / SLN_X;
		float2 pn_ = pn;
		float ONEXN = 1.0 / SLN_X;

		#if VHS_TAPENOISE_ON
			uint distWidth = 20;
			float distAmount = 4.0;
			float distThreshold = tapeLinesAmount;
			float distShift = 0;
			for (uint ii = 0; ii < distWidth % 1023; ii++)
			{
				float tnl = SAMPLE_TEXTURE2D_X_LOD(_TapeTex,s_linear_clamp_sampler, float2(0.0, pn.y - ONEXN * ii),0).y;
				if (tnl > distThreshold) 
				{
					float sh = sin(1.0 * PI * (float(ii) / float(distWidth)));
					p.x -= float(int(sh) * distAmount * ONEXN);
					distShift += sh;
				}
			}
		#endif	

		col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, p).rgb;
		signal = rgb2yiq(col);

		#if VHS_LINENOISE_ON || VHS_FILMGRAIN_ON
			signal.x += SAMPLE_TEXTURE2D_X_LOD(_TapeTex,s_linear_clamp_sampler, pn,0).z;
		#endif

		#if VHS_YIQNOISE_ON
			float2 noise = n4rand_bw(pn_,t, 1.0 - signalNoisePower);
			signal.y += (noise.x * 2.0 - 1.0) * signalNoiseAmount * signal.x;
			signal.z += (noise.y * 2.0 - 1.0) * signalNoiseAmount * signal.x;
		#endif

		#if VHS_TAPENOISE_ON
			half tn = SAMPLE_TEXTURE2D_X_LOD(_TapeTex,s_linear_clamp_sampler, pn,0 ).x;
			signal.x = bms(signal.x, tn * tapeNoiseAmount);
			uint tailLength = 10;

			for (uint j = 0; j < tailLength % 1023; j++) 
			{
				float jj = float(j);
				float2 d = float2(pn.x - ONEXN * jj,pn.y);
				tn = SAMPLE_TEXTURE2D_X_LOD(_TapeTex, s_linear_clamp_sampler, ClampAndScaleUVForPoint(d), 0).x;
				float fadediff = SAMPLE_TEXTURE2D_X_LOD(_TapeTex,s_linear_clamp_sampler, d,0).a;

				if (tn > 0.8) 
				{
					float nsx = 0.0;
					float newlength = float(tailLength) * (1 - fadediff);
					if (jj <= newlength) nsx = 1.0 - (jj / newlength);
					signal.x = bms(signal.x, nsx * tapeNoiseAmount).x;
				}
			}

			if (distShift > 0.4) 
			{
				signal.y *= 1.0 / distShift;
				signal.z *= 1.0 / distShift;
			}
		#endif

		col = yiq2rgb(signal);

		half4 col1 = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, i.texcoord);


		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, i.texcoord).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, i.texcoord).r);
#endif
			_Intensity *= alpha_Mask;
		}

		return lerp(col1, half4(col, 1), _Intensity);
	}

	float4 NoiseFrag1(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float t = time_;
		float2 UV = i.texcoord;
        float2 positionSS = UV;

		float2 p = UV;
		float2 p_ = p ;
		float ns = 0.0;
		float nt = 0.0;
		float nl = 0.0;
		float	ntail = hash12(p + float2(0.01,0.02));

		#if VHS_TAPENOISE_ON
			nl = tapeNoiseLines(p, t * tapeNoiseSpeed) * 1.0;
			nt = tapeNoise(nl, p, t * tapeNoiseSpeed) * 1.0;
		#endif

		#if VHS_LINENOISE_ON
			ns += lineNoise(p_, t * lineNoiseSpeed) * lineNoiseAmount * 20;
		#endif

		#if VHS_FILMGRAIN_ON	
			float bg = filmGrain((p_ - 0.5 ) * 0.5, t, filmGrainPower);
			ns += bg * filmGrainAmount;
		#endif

		return half4(nt,nl,ns,ntail);
	}

	ENDHLSL

	SubShader
	{
		Pass
		{
			Name "#NOISE#"
			ZWrite Off
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off

			HLSLPROGRAM
				#pragma fragment NoiseFrag
				#pragma vertex Vert
			ENDHLSL
		}
			Pass
		{
			Name "#NOISE2#"

			ZWrite Off
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off

			HLSLPROGRAM
				#pragma fragment NoiseFrag1
				#pragma vertex Vert
			ENDHLSL
		}

	}
	Fallback Off
}