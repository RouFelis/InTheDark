Shader "Custom/LaserBeam"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,0,0,1)
        _ScrollSpeed ("Scroll Speed", Float) = 1.0
        _Brightness ("Brightness", Float) = 2.0
        _EdgeGlow ("Edge Glow", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent+100" 
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _ScrollSpeed;
            float _Brightness;
            float _EdgeGlow;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.x += _Time.y * _ScrollSpeed; // UV 스크롤
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                float alpha = tex.a * _Color.a;
                
                // 중심 밝기 조절
                float center = saturate(1 - abs(i.uv.y * 2 - 1)); 
                alpha *= center * _Brightness;
                
                // 가장자리 발광 효과
                float edge = smoothstep(0, _EdgeGlow, center);
                alpha += edge * _EdgeGlow;
                
                return fixed4(_Color.rgb * alpha, alpha);
            }
            ENDCG
        }
    }
}