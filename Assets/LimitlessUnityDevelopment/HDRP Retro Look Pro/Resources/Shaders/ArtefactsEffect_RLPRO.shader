Shader "Hidden/Shader/ArtefactsEffect_RLPRO"
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
	TEXTURE2D_X(_LastTex);
	TEXTURE2D_X(_FeedbackTex);
	TEXTURE2D_X(_FeedbackTex3);
	TEXTURE2D_X(_InputTexture4);
	TEXTURE2D_X(_InputTexture2);
	TEXTURE2D_X(_InputTexture3);
	float feedbackAmount = 0.0;
	float feedbackFade = 0.0;
	float feedbackThresh = 5.0;
	half3 feedbackColor = half3(1.0, 0.5, 0.0); //
	float feedbackAmp = 1.0;
	half3 bm_screen(half3 a, half3 b) { return 1.0 - (1.0 - a) * (1.0 - b); }

    float _Intensity;
    TEXTURE2D_X(_MainTex);

	float4 Frag0(Varyings i) : COLOR
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);		
		float one_x = 1.0 / _ScreenParams.x;
		half3 fc = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, i.texcoord  ).rgb;
		half3 fl = SAMPLE_TEXTURE2D_X(_LastTex,s_linear_clamp_sampler, i.texcoord  ).rgb;
		float diff = abs(fl.x - fc.x + fl.y - fc.y + fl.z - fc.z) / 3.0;
		if (diff < feedbackThresh) diff = 0.0;
		half3 fbn = fc * diff * feedbackAmount;
		half3 fbb = half3(0.0, 0.0, 0.0);
		fbb = (
			SAMPLE_TEXTURE2D_X(_FeedbackTex,s_linear_clamp_sampler, i.texcoord  ).rgb +
			SAMPLE_TEXTURE2D_X(_FeedbackTex,s_linear_clamp_sampler, i.texcoord   + float2(one_x, 0.0)).rgb +
			SAMPLE_TEXTURE2D_X(_FeedbackTex,s_linear_clamp_sampler, i.texcoord   - float2(one_x, 0.0)).rgb
			) / 3.0;
		fbb *= feedbackFade;
		fbn = bm_screen(fbn, fbb);
		return half4(fbn * feedbackColor, 1.0);
	}

		float4 Frag(Varyings i) : COLOR
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		half3 col = SAMPLE_TEXTURE2D_X(_InputTexture4,s_linear_clamp_sampler, i.texcoord  ).rgb;
		half4 fbb = SAMPLE_TEXTURE2D_X(_FeedbackTex3,s_linear_clamp_sampler, i.texcoord  );
		col.rgb = bm_screen(col.rgb, fbb.xyz);
		return half4(col, fbb.a);
	}

		float4 Frag1(Varyings i) : COLOR
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		float4 col = SAMPLE_TEXTURE2D_X(_InputTexture2,s_linear_clamp_sampler,i.texcoord  );
		float4 col1 = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler,i.texcoord  );
		return lerp(col1, col, _Intensity);
	}

		float4 Frag2(Varyings i) : COLOR
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		float4 col = SAMPLE_TEXTURE2D_X(_InputTexture3,s_linear_clamp_sampler,i.texcoord  );
		return col;
	}

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "#first#"

            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
                #pragma fragment Frag0
                #pragma vertex Vert
            ENDHLSL
        }
			Pass
		{
			Name "#second#"

			ZWrite Off
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off

			HLSLPROGRAM
				#pragma fragment Frag
				#pragma vertex Vert
			ENDHLSL
		}
			Pass
		{
			Name "#CopyBlit#"

			ZWrite Off
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off

			HLSLPROGRAM
				#pragma fragment Frag1
				#pragma vertex Vert
			ENDHLSL
		}
			Pass
		{
			Name "#CopyBlit2#"

			ZWrite Off
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off

			HLSLPROGRAM
				#pragma fragment Frag2
				#pragma vertex Vert
			ENDHLSL
		}
    }
    Fallback Off
}