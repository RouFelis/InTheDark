Shader "Hidden/Shader/VHSEffect_RLPRO"
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
	#define half4_one half4(1.0, 1.0, 1.0, 1.0)
	half iterations;
	half smoothSize;
	float _StandardDeviation;
	half _OffsetNoiseX;
	half _OffsetNoiseY;
	TEXTURE2D_X(_MainTex);
	SAMPLER(_SecondaryTex);
	TEXTURE2D(_Mask);
	SAMPLER(sampler_Mask);
	float _FadeMultiplier;

	half _Stripes;
	float4 _MainTex_ST;
	float4 _SecondaryTex_ST;
	#define E 2.71828182846
	float _Intensity;
	float _TexIntensity;
	float _TexCut;
	float _OffsetColor;
	float2 _OffsetColorAngle;
	half _OffsetPosY;
	half _OffsetDistortion;
	half tileX = 0;
	half tileY = 0;
	half smooth1 = 0;
	float Time;
#pragma shader_feature ALPHA_CHANNEL

	float MaskCut(float2 uv)
	{
		if (_FadeMultiplier > 0)
		{
			#if ALPHA_CHANNEL
						float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv).a);
			#else
						float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv).r);
			#endif
						return alpha_Mask;
		}
		else return 1;
	}

	#define unity_ColorSpaceLuminance half4(0.0396819152, 0.458021790, 0.00609653955, 1.0) 
	float smoothCut(float colR) {
		if (smooth1)
			return saturate(colR - _TexCut);
		else
			return ceil(colR - _TexCut);
	}
	inline half luminance(half3 rgb)
	{
		return dot(rgb, unity_ColorSpaceLuminance.rgb);
	}
    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
		float2 texcoordStereo   : TEXCOORD1;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
		output.texcoordStereo = ClampAndScaleUV(output.texcoord + float2(_OffsetNoiseX - 0.2f, _OffsetNoiseY), _ScreenParams.zw * float2(tileY, tileX), 1.0);
		output.texcoordStereo *= float2(tileY, tileX);
        return output;
    }
	float4 box(sampler2D tex, float2 uv, float4 size)
	{
		float4 col = 0;
		float sum = 0;
		//iterate over blur samples
		for (float index = 0; index < iterations; index++) 
		{
			//get the offset of the sample
			float offset = (index / (10 - 1) - 0.5) * size.x;
			//get uv coordinate of sample
			float2 uv1 = uv + float2(0, offset);

			//calculate the result of the gaussian function
			float stDevSquared = _StandardDeviation * _StandardDeviation;
			float gauss = (1 / sqrt(2 * PI * stDevSquared)) * pow(E, -((offset * offset) / (2 * stDevSquared)));
			//add result to sum
			sum += gauss;
			//multiply color with influence from gaussian function and add it to sum color
			col += tex2D(tex, uv1) * gauss;
		}
		//divide the sum of values by the amount of samples
		col = col / sum;
		return col;
	}
	//
	float4 FragDivide(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
		float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		col2 = col2 / col;
		col2 = lerp(col,col2, _TexIntensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(UV.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}
		return lerp(col3, last, _Intensity);
	}
	//
	float4 FragSubtract(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		col2 = saturate(col - col2);
		col2 = lerp(col,col2, _TexIntensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(UV.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}
		return lerp(col3, last, _Intensity);
	}
	//
	float4 FragMultiply(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		col2 = saturate(col * col2);
		col2 = lerp(col,col2,_TexIntensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(UV.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}
		return lerp(col3, last, _Intensity);

	}
	//
	float4 FragColorBurn(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		col2 = half4_one - (half4_one - col) / col2;
		col2 = lerp(col,col2,_TexIntensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(UV.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}
		return lerp(col3, last, _Intensity);

	}
	//
	float4 FragLinearBurn(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		col2 = col2 + col - half4_one;
		col2 = lerp(col,col2,_TexIntensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(UV.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}
		return lerp(col3, last, _Intensity);

	}
	//
		float4 FragDarkerColor(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		col2 = luminance(col.rgb) < luminance(col2.rgb) ? col : col2;
		col2 = lerp(col,col2,_TexIntensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(UV.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}
		return lerp(col3, last, _Intensity);

	}
	//
	float4 FragLighten(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		col2 = max(col, col2);
		col2 = lerp(col,col2,_TexIntensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(UV.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}
		return lerp(col3, last, _Intensity);

	}
	//
	float4 FragScreen(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		col2 = half4_one - ((half4_one - col2) * (half4_one - col));
		col2 = lerp(col,col2,_TexIntensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(UV.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}
		return lerp(col3, last, _Intensity);

	}
	//
	float4 FragColorDodge(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		col2 = col / (half4_one - col2);
		col2 = lerp(col,col2,_TexIntensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(UV.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}
		return lerp(col3, last, _Intensity);

	}
	//
	float4 FragLinearDodge(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		col2 = col + col2;
		col2 = lerp(col,col2,_TexIntensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(UV.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}
		return lerp(col3, last, _Intensity);

	}
	//
	float4 FragLighterColor(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		col2 = luminance(col.rgb) > luminance(col2.rgb) ? col : col2;
		col2 = lerp(col,col2,_TexIntensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(UV.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}
		return lerp(col3, last, _Intensity);

	}
	//
	float4 FragOverlay(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		float4 check = step(0.5, col2);
		float4 ress = check * (half4_one - ((half4_one - 2.0 * (col - 0.5)) * (half4_one - col2)));
		ress += (half4_one - check) * (2.0 * col * col2);
		col2 = ress;
		col2 = lerp(col,col2,_TexIntensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(UV.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}
		return lerp(col3, last, _Intensity);

	}
	//
	float4 FragSoftLight(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		float4 check = step(0.5, col2);
		float4 result = check * (2.0 * col * col2 + col * col - 2.0 * col * col * col2);
		result += (half4_one - check) * (2.0 * sqrt(col) * col2 - sqrt(col) + 2.0 * col - 2.0 * col * col2);
		col2 = result;
		col2 = lerp(col,col2,_TexIntensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(UV.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}
		return lerp(col3, last, _Intensity);

	}
	//
	float4 FragHardLight(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		float4 check = step(0.5, col2);
		float4 result = check * (half4_one - ((half4_one - 2.0 * (col - 0.5)) * (half4_one - col2)));
		result += (half4_one - check) * (2.0 * col * col2);
		col2 = result;
		col2 = lerp(col,col2,_TexIntensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(UV.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}
		return lerp(col3, last, _Intensity);

	}
	//
	float4 FragVividLight(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		float4 check = step(0.5, col2);
		float4 result = check * (col / (half4_one - 2.0 * (col2 - 0.5)));
		result += (half4_one - check) * (half4_one - (half4_one - col) / (2.0 * col2));
		col2 = result;
		col2 = lerp(col,col2,_TexIntensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(UV.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}
		return lerp(col3, last, _Intensity);

	}
	//
		float4 FragLinearLight(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		float4 check = step(0.5, col2);
		float4 result = check * (col + (2.0 * (col2 - 0.5)));
		result += (half4_one - check) * (col + 2.0 * col2 - half4_one);
		col2 = result;
		col2 = lerp(col,col2,_TexIntensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(UV.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}
		return lerp(col3, last, _Intensity);

	}
	//
	float4 FragPinLight(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		float4 check = step(0.5, col2);
		float4 result = check * max(2.0 * (col2 - 0.5), col);
		result += (half4_one - check) * min(2 * col2, col);
		col2 = result;
		col2 = lerp(col,col2,_TexIntensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(UV.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}
		return lerp(col3, last, _Intensity);

	}
	//
	float4 FragHardMix(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		float4 result = float4(0.0, 0.0, 0.0, 0.0);
		result.r = col2.r > 1.0 - col.r ? 1.0 : 0.0;
		result.g = col2.g > 1.0 - col.g ? 1.0 : 0.0;
		result.b = col2.b > 1.0 - col.b ? 1.0 : 0.0;
		result.a = col2.a > 1.0 - col.a ? 1.0 : 0.0;
		col2 = result;
		col2 = lerp(col,col2,_Intensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(UV.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}
		return lerp(col3, last, _Intensity);

	}
	//
	float4 FragDifference(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		col2 = abs(col - col2);
		col2 = lerp(col,col2,_Intensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(UV.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}
		return lerp(col3, last, _Intensity);

	}
	//
	float4 FragExclusion(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		col2 = col + col2 - (2.0 * col * col2);
		col2 = lerp(col,col2,_Intensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(UV.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}
		return lerp(col3, last, _Intensity);

	}
	//
	float4 FragDarken(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		col2 = min(col, col2);
		col2 = lerp(col,col2,_Intensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(input.texcoord.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}
		return lerp(col3, last, _Intensity);

	}
	//
	float4 Frag(Varyings input) : SV_Target
	{
		float2 UV = input.texcoord;
		half alpha_Mask = MaskCut(UV);
		_OffsetColor *= alpha_Mask;
		_TexIntensity *= alpha_Mask;
		_OffsetPosY *= alpha_Mask;
		_OffsetDistortion *= alpha_Mask;
		UV = float2(frac(UV.x), frac(UV.y + _OffsetPosY));
		UV.x = _OffsetDistortion > 0 ? frac(UV.x + cos((UV.y + Time) * 100) * _OffsetDistortion*0.1) : frac(UV.x);
		float2 positionSS = UV;
				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS);
		half  amount = _OffsetColor * (distance(positionSS, half2(0.5, 0.5))) * 2;
		col.r = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS + (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).r;
		col.b = SAMPLE_TEXTURE2D_X(_MainTex,s_linear_clamp_sampler, positionSS - (amount * half2(_OffsetColorAngle.y, _OffsetColorAngle.x))).b;
		float4 col2 = tex2D(_SecondaryTex, input.texcoordStereo);
		col2 = box(_SecondaryTex, input.texcoordStereo, smoothSize);
		col2 = lerp(col,col2,_TexIntensity);
		float4 last = lerp(col, col2, smoothCut(col2.r)) * (1 - ceil(saturate(abs(UV.y - 0.5) - _Stripes)));
		float2 UV2 = input.texcoord;
		float4 col3 = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, UV2);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, UV).r);
#endif
			_Intensity *= alpha_Mask;
		}

		return lerp(col3, last, _Intensity);

	}

    ENDHLSL

    SubShader
    {
		Cull Off ZWrite Off ZTest Always

			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment Frag
			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment FragDarken
			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment FragMultiply
			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment FragColorBurn
			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment FragLinearBurn
			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment FragDarkerColor
			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment FragLighten
			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment FragScreen
			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment FragColorDodge
			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment FragLinearDodge
			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment FragLighterColor
			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment FragOverlay
			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment FragSoftLight
			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment FragHardLight
			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment FragVividLight
			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment FragLinearLight
			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment FragPinLight
			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment FragHardMix
			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment FragDifference
			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment FragExclusion
			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment FragSubtract
			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment FragDivide
			ENDHLSL
		}
    }
    Fallback Off
}