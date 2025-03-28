Shader "Hidden/Shader/EdgeStretchEffect_RLPRO"
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
	#pragma shader_feature top_ON
	#pragma shader_feature bottom_ON
	#pragma shader_feature left_ON
	#pragma shader_feature right_ON

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
	float amplitude;
	float frequency;
	half _NoiseBottomHeight;
	float Time;
	half speed;

		float onOff(float a, float b, float c, float t)
		{
			return step(c, sin(t + a * cos(t * b)));
		}
		float2 wobble(float2 uv, float amplitude, float frequence, float speed)
		{
			float offset = amplitude * sin(uv.y * frequence * 20.0 + Time * speed);
			return float2((uv.x + (20 * _NoiseBottomHeight)) + offset, uv.y);
		}
		float2 wobbleR(float2 uv, float amplitude, float frequence, float speed)
		{
			float offset = amplitude * onOff(2.1, 4.0, 0.3, Time * speed) * sin(uv.y * frequence * 20.0 + Time * speed);
			return float2((uv.x + (20 * _NoiseBottomHeight)) + offset, uv.y);
		}
		float2 wobbleV(float2 uv, float amplitude, float frequence, float speed)
		{
			float offset = amplitude * sin(uv.x * frequence * 20.0 + Time * speed);
			return float2((uv.y + (20 * _NoiseBottomHeight)) + offset, uv.x);
		}
		float2 wobbleVR(float2 uv, float amplitude, float frequence, float speed)
		{
			float offset = amplitude * onOff(2.1, 4.0, 0.3, Time * speed) * sin(uv.x * frequence * 20.0 + Time * speed);
			return float2((uv.y + (20 * _NoiseBottomHeight)) + offset, uv.x);
		}

    TEXTURE2D_X(_MainTex);
    float _Intensity;

	float4 FragDist(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		half2 uv = i.texcoord;

#if top_ON
	uv.y = min(uv.y, 1 - (wobble(uv, amplitude, frequency, speed).x * (_NoiseBottomHeight / 20)));

#endif
#if bottom_ON
	uv.y = max(uv.y, wobble(uv, amplitude, frequency, speed).x * (_NoiseBottomHeight / 20));
#endif
#if left_ON
	uv.x = max(uv.x, wobbleV(uv, amplitude, frequency, speed).x * (_NoiseBottomHeight / 20));
#endif
#if right_ON
	uv.x = min(uv.x, 1 - (wobbleV(uv, amplitude, frequency, speed).x * (_NoiseBottomHeight / 20)));
#endif
        half2 positionSS = i.texcoord;
	half4 color = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, uv);
	half4 color1 = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
	float exp = 1.0;
	return lerp(color1, float4(pow(color.xyz, float3(exp, exp, exp)), color.a), _Intensity);

	}
	float4 FragDistRand(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);	
		half2 uv = i.texcoord;
#if top_ON
	uv.y = min(uv.y, 1 - (wobbleR(uv, amplitude, frequency, speed).x * (_NoiseBottomHeight / 20)));
#endif
#if bottom_ON
	uv.y = max(uv.y, wobbleR(uv, amplitude, frequency, speed).x * (_NoiseBottomHeight / 20));
#endif
#if left_ON
	uv.x = max(uv.x, wobbleVR(uv, amplitude, frequency, speed).x * (_NoiseBottomHeight / 20));
#endif
#if right_ON
	uv.x = min(uv.x, 1 - (wobbleVR(uv, amplitude, frequency, speed).x * (_NoiseBottomHeight / 20)));
#endif
	half2 positionSS = i.texcoord;
	half4 color = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, uv);
	half4 color1 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, positionSS);
	float exp = 1.0;
	return lerp(color1, float4(pow(color.xyz, float3(exp, exp, exp)), color.a), _Intensity);

	}
	float4 Frag(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		half2 uv = i.texcoord;
	
#if top_ON
	uv.y = min(uv.y, 1 - (_NoiseBottomHeight / 2) - 0.01);
#endif
#if bottom_ON
	uv.y = max(uv.y, (_NoiseBottomHeight / 2) - 0.01);
#endif
#if left_ON
	uv.x = max(uv.x, (_NoiseBottomHeight / 2) - 0.01);
#endif
#if right_ON
	uv.x = min(uv.x, 1 - (_NoiseBottomHeight / 2) - 0.01);
#endif
	half2 positionSS = i.texcoord;
	half4 color = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, uv);
	half4 color1 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, positionSS);
	float exp = 1.0;
	return lerp(color1, float4(pow(color.xyz, float3(exp, exp, exp)), color.a), _Intensity);

	}
    ENDHLSL

    SubShader
    {
		Cull Off ZWrite Off ZTest Always

			Pass
		{
			HLSLPROGRAM

				#pragma vertex Vert
				#pragma fragment FragDist

			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM

				#pragma vertex Vert
				#pragma fragment FragDistRand

			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM

				#pragma vertex Vert
				#pragma fragment Frag

			ENDHLSL
		}
    }
    Fallback Off
}