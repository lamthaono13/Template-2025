Shader "Custom/ShockWave"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _WaveTex("Wave Texture (Normal/Distort)", 2D) = "gray" {}
        _Color("Tint", Color) = (1,1,1,1)
        _Strength("Distortion Strength", Range(0,1)) = 0.2
        _Speed("Wave Speed", Range(-5,5)) = 1
        _Alpha("Alpha", Range(0,1)) = 1
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            LOD 200
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 3.0

                #include "UnityCG.cginc"

                sampler2D _MainTex;
                float4 _MainTex_ST;

                sampler2D _WaveTex;
                float4 _WaveTex_ST;

                fixed4 _Color;
                float _Strength;
                float _Speed;
                float _Alpha;

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float2 uvWave : TEXCOORD1;
                };

                v2f vert(appdata v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    o.uvWave = TRANSFORM_TEX(v.uv, _WaveTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // Sample wave texture (dùng làm distortion map)
                    float2 waveUV = i.uvWave + float2(_Time.y * _Speed, _Time.y * _Speed);
                    fixed4 wave = tex2D(_WaveTex, waveUV);

                    // Lấy offset từ wave texture (ví dụ dùng red/green làm normal map)
                    float2 offset = (wave.rg * 2 - 1) * _Strength;

                    // Distort UV
                    float2 distortedUV = i.uv + offset;

                    fixed4 col = tex2D(_MainTex, distortedUV) * _Color;
                    col.a *= _Alpha;

                    return col;
                }
                ENDCG
            }
        }
            FallBack "Transparent/Diffuse"
}