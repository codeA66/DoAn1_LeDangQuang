// Ve vien quanh hinh sprite (chi phan vien, khong ve than): diem trong suot ma co diem dac trong ban kinh
// _Width texel xung quanh thi to mau _OutlineColor. Dung cho lop vien sau quai de quai tach khoi nen.
// Sprite can Mesh Type = Full Rect, neu Tight thi vien ngoai mep luoi bi cat.
Shader "LuHanh/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0.56, 0.22, 0.24, 1)
        _Width ("Width (texel)", Range(1, 16)) = 6
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                float _Width;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color;
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half self = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                float2 d = _MainTex_TexelSize.xy * _Width;
                half around = 0;
                // 8 huong + 8 huong o nua ban kinh cho vien day deu.
                [unroll] for (int i = 0; i < 8; i++)
                {
                    float a = i * 0.785398;
                    float2 dir = float2(cos(a), sin(a));
                    around = max(around, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + dir * d).a);
                    around = max(around, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + dir * d * 0.5).a);
                }
                half edge = saturate(around - self);
                return half4(_OutlineColor.rgb, edge * _OutlineColor.a * input.color.a);
            }
            ENDHLSL
        }
    }
}
