Shader "Hidden/Shader/VHSScanlinesEffect_RLPRO"
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
	float4 _ScanLinesColor;
	float _ScanLines;
	float speed;
	float fade;
	float _OffsetDistortion;
	float sferical;
	float barrel;
	float scale;
	float _OffsetColor;
	float2 _OffsetColorAngle;
	float Time;
    float _Intensity;
    TEXTURE2D_X(_MainTex);
	TEXTURE2D(_Mask);
	SAMPLER(sampler_Mask);
	float _FadeMultiplier;
	#pragma shader_feature ALPHA_CHANNEL

	float2 FisheyeDistortion(float2 coord, float spherical, float barrel, float scale)
	{
		float2 h = coord.xy - float2(0.5, 0.5);
		float r2 = dot(h, h);
		float f = 1.0 + r2 * (spherical + barrel * sqrt(r2));
		return f * scale * h + 0.5;
	}

	float4 FragH(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		float2 UV = i.texcoord;
        uint2 positionSS = UV;

		float2 coord = FisheyeDistortion(UV, sferical, barrel, scale);
		half4 color = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		float lineSize = _ScreenParams.y * 0.005;
		float displacement = ((_Time.x / 4 * 1000) * speed) % _ScreenParams.y;
		float ps;
		ps = displacement + (coord.y * _ScreenParams.y / i.positionCS.w);
		float sc = UV.y;
		float4 result;
		result = ((uint)(ps / floor(_ScanLines * lineSize)) % 2 == 0) ? color : _ScanLinesColor;
		result += color * sc;
		if (_FadeMultiplier > 0)
		{
			#if ALPHA_CHANNEL
				float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV ).a);
			#else
				float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV ).r);
			#endif
				fade *= alpha_Mask;
		}

		return lerp(color,result,fade);
	}

	float4 FragHD(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		
		float2 UV = i.texcoord;
        float2 positionSS = UV;

		float2 coord = FisheyeDistortion(UV, sferical, barrel, scale);
		half4 color = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		float lineSize = _ScreenParams.y * 0.005;
		float displacement = ((_Time.x / 4 * 1000) * speed) % _ScreenParams.y;
		float ps;
		UV.y = frac(UV.y + cos((coord.x + _Time.x / 4) * 100)  * _OffsetDistortion*0.1);
		ps = displacement + (UV.y * _ScreenParams.y / i.positionCS.w);
		float sc = UV.y;
		float4 result;
		result = ((uint)(ps / floor(_ScanLines * lineSize)) % 2 == 0) ? color : _ScanLinesColor;
		result += color * sc;
		if (_FadeMultiplier > 0)
		{
			#if ALPHA_CHANNEL
				float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV ).a);
			#else
				float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV ).r);
			#endif
				fade *= alpha_Mask;
		}
		return lerp(color,result,fade);
	}

	float4 FragV(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		float2 UV = i.texcoord;
        float2 positionSS = UV;

		float2 coord = FisheyeDistortion(UV, sferical, barrel, scale);
		half4 color = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		float lineSize = _ScreenParams.y * 0.005;
		float displacement = ((_Time.x / 4 * 1000) * speed) % _ScreenParams.y;
		float ps;
		ps = displacement + (coord.x * _ScreenParams.x / i.positionCS.w);
		float sc = UV.y;
		float4 result;
		result = ((uint)(ps / floor(_ScanLines * lineSize)) % 2 == 0) ? color : _ScanLinesColor;
		result += color * sc;
		if (_FadeMultiplier > 0)
		{
			#if ALPHA_CHANNEL
				float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV ).a);
			#else
				float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV ).r);
			#endif
				fade *= alpha_Mask;
		}
		return lerp(color,result,fade);
	}

	float4 FragVD(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		float2 UV = i.texcoord;
        float2 positionSS = UV;

		float2 coord = FisheyeDistortion(UV, sferical, barrel, scale);
		half4 color = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		float lineSize = _ScreenParams.y * 0.005;
		float displacement = ((_Time.x / 4 * 1000) * speed) % _ScreenParams.y;
		float ps;
		UV.x = frac(UV.x + cos((coord.y + (_Time.x / 4)) * 100)  * _OffsetDistortion*0.1);
		ps = displacement + (UV.x * _ScreenParams.x / i.positionCS.w);
		float sc = UV.y;
		float4 result;
		result = ((uint)(ps / floor(_ScanLines * lineSize)) % 2 == 0) ? color : _ScanLinesColor;
		result += color * sc;
		if (_FadeMultiplier > 0)
		{
			#if ALPHA_CHANNEL
				float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV ).a);
			#else
				float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV ).r);
			#endif
				fade *= alpha_Mask;
		}
		return lerp(color,result,fade);
	}
    ENDHLSL

    SubShader
    {
		Cull Off ZWrite Off ZTest Always

			Pass
		{
			HLSLPROGRAM

				#pragma vertex Vert
				#pragma fragment FragH

			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM

				#pragma vertex Vert
				#pragma fragment FragHD

			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM

				#pragma vertex Vert
				#pragma fragment FragV

			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM

				#pragma vertex Vert
				#pragma fragment FragVD

			ENDHLSL
		}
    }
    Fallback Off
}