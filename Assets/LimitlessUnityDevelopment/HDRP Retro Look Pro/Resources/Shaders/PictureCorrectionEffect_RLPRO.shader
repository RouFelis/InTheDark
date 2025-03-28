Shader "Hidden/Shader/PictureCorrectionEffect_RLPRO"
{
	Properties
    {
        _MainTex("Main Texture", 2DArray) = "grey" {}
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
	float signalAdjustY = 0.0;
	float signalAdjustI = 0.0;
	float signalAdjustQ = 0.0;

	float signalShiftY = 0.0;
	float signalShiftI = 0.0;
	float signalShiftQ = 0.0;
	float gammaCorection = 1.0;
	half3 rgb2yiq(half3 c) {
		return half3(
			(0.2989 * c.x + 0.5959 * c.y + 0.2115 * c.z),
			(0.5870 * c.x - 0.2744 * c.y - 0.5229 * c.z),
			(0.1140 * c.x - 0.3216 * c.y + 0.3114 * c.z)
			);
	};

	half3 yiq2rgb(half3 c) {
		return half3(
			(1.0 * c.x + 1.0 * c.y + 1.0 * c.z),
			(0.956 * c.x - 0.2720 * c.y - 1.1060 * c.z),
			(0.6210 * c.x - 0.6474 * c.y + 1.7046 * c.z)
			);
	};

    float _Intensity;
    TEXTURE2D_X(_MainTex);
	TEXTURE2D(_Mask);
	SAMPLER(sampler_Mask);
	float _FadeMultiplier;
	#pragma shader_feature ALPHA_CHANNEL

	float4 Frag(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		float2 UV = i.texcoord;
        float2 positionSS = UV;

		half3 signal = half3(0.0, 0.0, 0.0);
		float2 c = i.texcoord.xy;
		signal = rgb2yiq(SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS).rgb);
		signal.x += signalAdjustY;
		signal.y += signalAdjustI;
		signal.z += signalAdjustQ;
		signal.x *= signalShiftY;
		signal.y *= signalShiftI;
		signal.z *= signalShiftQ;

		float3 rgb = yiq2rgb(signal);
		if (gammaCorection != 1.0) rgb = pow(abs(rgb), gammaCorection);
		half4 colIn = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		if (_FadeMultiplier > 0)
		{
			#if ALPHA_CHANNEL
				float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, c).a);
			#else
				float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, c).r);
			#endif
				_Intensity *= alpha_Mask;
		}

		return lerp(colIn,half4(rgb, colIn.a), _Intensity);
	}

    ENDHLSL

    SubShader
    {
			Pass
		{
			Name "#PictureCorrection#"

			ZWrite Off
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off

			HLSLPROGRAM
				#pragma fragment Frag
				#pragma vertex Vert
			ENDHLSL
		}
    }
    Fallback Off
}