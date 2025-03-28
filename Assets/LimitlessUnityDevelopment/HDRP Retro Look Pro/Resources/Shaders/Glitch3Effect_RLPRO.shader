Shader "Hidden/Shader/Glitch3Effect_RLPRO"
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

    float _Intensity;
	float _ColorIntensity;
    TEXTURE2D_X(_MainTex);
    TEXTURE2D(_Mask);
    SAMPLER(sampler_Mask);
    float _FadeMultiplier;
    #pragma shader_feature ALPHA_CHANNEL

	float Time;
	float speed;
	float density;
	float maxDisplace;
    	float fract(float x) {
		return  x - floor(x);
	}
	float2 fract(float2 x) {
		return  x - floor(x);
	}
	float4 fract(float4 x) {
		return  x - floor(x);
	}
	float rand(float2 seed)
	{
		return fract(sin(dot(seed * floor(Time*10 * speed), float2(127.1, 311.7))) * 43758.5453123);
	}

	float rand(float seed)
	{
		return rand(float2(seed, 1.0));
	}

	float4 Frag(Varyings i) : SV_Target
	{
	    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float2 UV = i.texcoord;
        float2 pos = (UV - 0.5 * 1) / 1;
        if (_FadeMultiplier > 0)
        {
    #if ALPHA_CHANNEL
            float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
    #else
            float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
    #endif
            density *= alpha_Mask;
            maxDisplace *= alpha_Mask;
        }
		float2 rblock = rand(floor(pos * density*10));
		float displaceNoise = pow(abs(rblock.x), 8.0) * pow(abs(rblock.x), 3.0) - pow(abs(rand(7.2341)), 17.0) * maxDisplace;
		float r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, UV).r;
		float g = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, UV + half2(displaceNoise *maxDisplace* 0.01 * rand(7.0), 0.0)).g;
		float b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, UV - half2(displaceNoise *maxDisplace* 0.01 * rand(13.0), 0.0)).b;

		return half4(r, g, b, 1);
	}

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "#GlitchPass#"

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