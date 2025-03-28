Shader "Hidden/Shader/VHSTapeRewind_RLPRO"
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

    half _Fade;

    float _Intensity;
    float _amount;
    TEXTURE2D_X(_MainTex);


		float4 Frag0(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = i.texcoord;
        float2 positionSS = uv ;

        float2 displacementSampleUV = float2(uv.x + (_Time.x + 20.) * 75.0, uv.y) ;

        float da = _amount;

        float displacement = SAMPLE_TEXTURE2D_X(_MainTex,s_point_clamp_sampler,  displacementSampleUV).x * da;

        float2 displacementDirection = float2(cos(displacement * 6.28318530718), sin(displacement * 6.28318530718));
        float2 displacedUV = (uv  + displacementDirection * displacement);
        float4 shade = SAMPLE_TEXTURE2D_X(_MainTex,s_point_clamp_sampler, displacedUV);
        float4 main = SAMPLE_TEXTURE2D_X(_MainTex,s_point_clamp_sampler,  positionSS);
        return  float4(lerp(main, shade, _Fade));
	}

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "#VHS Tape Rewind#"

            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
                #pragma fragment Frag0
                #pragma vertex Vert
            ENDHLSL
        }

    }
    Fallback Off
}