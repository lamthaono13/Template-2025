Shader "Custom/Opaque"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("MainTex", 2D) = "white" {}
        _Alpha_Clipping("Alpha Clip", Range(0,1)) = 0.5

            // Tuỳ chọn PBR cơ bản
            _Metallic("Metallic", Range(0,1)) = 0
            _Glossiness("Smoothness", Range(0,1)) = 0.5

            // (Giữ cho tương thích với code cũ nếu bạn có set bằng script;
            // không bắt buộc dùng trong shader)
            _QueueOffset("_QueueOffset", Float) = 0
            _QueueControl("_QueueControl", Float) = 0
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
            LOD 200
            Cull Back
            ZWrite On
            ZTest LEqual
            Blend Off

            CGPROGRAM
            // -------- Surface Shader ----------
            #pragma surface surf Standard fullforwardshadows addshadow
            #pragma target 3.0
            #pragma multi_compile_instancing

            sampler2D _MainTex;
            fixed4 _Color;
            half _Alpha_Clipping;
            half _Metallic;
            half _Glossiness;

            struct Input
            {
                float2 uv_MainTex;
            };

            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

                // Alpha clip (tương đương "Alpha Clipping" của Shader Graph)
                clip(c.a - _Alpha_Clipping);

                o.Albedo = c.rgb;
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;
                o.Alpha = 1; // opaque sau khi đã clip
            }
            ENDCG
        }

            FallBack "Diffuse"
}