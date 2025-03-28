Shader "Hidden/Shader/Phosphor_RLPRO"
{
    Properties
    {
        _MainTex("Main Texture", 2DArray) = "grey" {}
    }

    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

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
    TEXTURE2D_X(_MainTex);
    TEXTURE2D_X(_Tex);
    float speed = 10.00;
	half amount = 5;
	half fade;
    	TEXTURE2D(_Mask);
	SAMPLER(sampler_Mask);
	float _FadeMultiplier;
	#pragma shader_feature ALPHA_CHANNEL

	float T;
    	float fract(float x) {
		return  x - floor(x);
	}
	float2 fract(float2 x) {
		return  x - floor(x);
	}

	float random(float2 noise)
	{
		return fract(sin(dot(noise.xy, float2(0.0001, 98.233))) * 925895933.14159265359);
	}

	float random_color(float noise)
	{
		return frac(sin(noise));
	}


    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float4 sourceColor = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, input.texcoord);

        float4 color = lerp(sourceColor, Luminance(sourceColor), _Intensity);
        float4 phosphor =  SAMPLE_TEXTURE2D_X(_Tex,s_linear_clamp_sampler, input.texcoord);

        		//float fade = 1;
		if (_FadeMultiplier > 0)
		{
			#if ALPHA_CHANNEL
				float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, input.texcoord).a);
			#else
				float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, input.texcoord).r);
			#endif
				fade *= alpha_Mask;
		}

		float4 result = sourceColor + lerp(float4(0,0,0,0),phosphor,fade);

        return result;

    }
    	
    float4 CustomPostProcess011(Varyings input) : SV_Target
	{
		half2 uv = fract(input.texcoord / 12 * ((T.x * speed)));
		half4 color = float4(random(uv.xy), random(uv.xy), random(uv.xy), random(uv.xy));
				
		color.r *= random_color(sin(T.x * speed));
		color.g *= random_color(cos(T.x * speed));
		color.b *= random_color(tan(T.x * speed));

		return color;

	}

    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "mixfosfor"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
                Pass
        {
            Name "fosfor"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment CustomPostProcess011
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}