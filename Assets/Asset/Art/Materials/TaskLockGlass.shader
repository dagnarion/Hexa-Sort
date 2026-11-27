Shader "Hexa Sort/Task Lock Glass"
{
    Properties
    {
        [MainColor] _BaseColor ("Glass Tint", Color) = (0.74, 0.89, 1, 0.12)
        [NoScaleOffset] _ShineMap ("Sweep Mask", 2D) = "black" {}
        _ShineOpacity ("Sweep Opacity", Range(0, 1)) = 0.34
        _SweepDuration ("Sweep Duration (seconds)", Float) = 1.5
        _SweepInterval ("Time Between Starts (seconds)", Float) = 6
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent-10" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_ShineMap);
            SAMPLER(sampler_ShineMap);
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _ShineOpacity;
                float _SweepDuration;
                float _SweepInterval;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                half3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 plateUV : TEXCOORD0;
                half bevel : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                // The board rotates the plate 30 degrees relative to the camera.
                float2 xz = input.positionOS.xz;
                output.plateUV = 0.5 + 0.5 * float2(dot(xz, float2(0.866, 0.5)),
                                                    dot(xz, float2(-0.5, 0.866)));
                output.bevel = 1.0h - saturate(input.normalOS.y);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float duration = max(0.01, _SweepDuration);
                float elapsed = fmod(_Time.y, max(duration, _SweepInterval));
                float2 uv = input.plateUV;
                uv.x += lerp(1.0, -1.0, saturate(elapsed / duration));
                // The source texture has a faint alpha floor around its two bright stripes.
                half mask = SAMPLE_TEXTURE2D(_ShineMap, sampler_ShineMap, saturate(uv)).a;
                half shine = saturate((mask - 0.17h) / 0.83h) * _ShineOpacity;
                shine *= step(0.0, uv.x) * step(uv.x, 1.0) * (1.0h - input.bevel);
                half glass = saturate(_BaseColor.a + 0.22h * input.bevel);
                half alpha = shine + glass * (1.0h - shine);
                half3 color = (_BaseColor.rgb * glass * (1.0h - shine) + shine) / max(alpha, 0.001h);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}
