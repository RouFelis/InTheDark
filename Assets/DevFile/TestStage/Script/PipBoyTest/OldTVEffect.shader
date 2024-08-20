Shader "Custom/OldTVEffect"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ScanlineIntensity("Scanline Intensity", Range(0, 1)) = 0.5
        _NoiseIntensity("Noise Intensity", Range(0, 1)) = 0.2
        _Resolution("Resolution", Range(0.1, 1.0)) = 0.5
        _JitterFrequency("Jitter Frequency", Range(0.1, 10.0)) = 1.0
        _JitterAmount("Jitter Amount", Range(0.0, 0.1)) = 0.05
        _BarSpeed("Bar Speed", Range(0.1, 10.0)) = 1.0
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
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
                float _ScanlineIntensity;
                float _NoiseIntensity;
                float _Resolution;
                float _JitterFrequency;
                float _JitterAmount;
                float _BarSpeed;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // �ػ� ���߱�
                    float2 lowResUV = floor(i.uv * _Resolution) / _Resolution;

                    // ���� �ؽ�ó���� ���� ��������
                    fixed4 col = tex2D(_MainTex, lowResUV);

                    // �׷��̽����� ȿ��
                    float gray = dot(col.rgb, float3(0.3, 0.59, 0.11));
                    col.rgb = gray;

                    // ��ĵ���� ȿ��
                    float scanline = sin(i.uv.y * _ScreenParams.y * 50.0) * _ScanlineIntensity;
                    col.rgb -= scanline;

                    // ������ ȿ�� �߰�
                    float noise = frac(sin(dot(i.uv.xy, float2(12.9898, 78.233) + _Time.y * 10.0)) * 43758.5453);
                    col.rgb += (noise - 0.5) * _NoiseIntensity;

                    // ���� �Ʒ��� �������� ȿ�� �߰�
                    float barY = fmod(_Time.y * _BarSpeed, 1.0);
                    float jitter = (abs(i.uv.y - barY) < 0.01) ? (sin(i.uv.y * _ScreenParams.y * _JitterFrequency + _Time.y * 10.0) * 0.5 + 0.5) * _JitterAmount : 0.0;
                    i.uv.x += jitter * (noise - 0.5);

                    // ���� �ؽ�ó �ٽ� ��������
                    col = tex2D(_MainTex, i.uv);
                    gray = dot(col.rgb, float3(0.3, 0.59, 0.11));
                    col.rgb = gray;

                    return col;
                }
                ENDCG
            }
        }
            FallBack "Diffuse"
}
