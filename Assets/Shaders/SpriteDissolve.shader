// To sprite thanh mot mau phang roi "bi gio thoi" thanh lan khoi: hinh bi keo gian va uon luon
// theo huong gio, bi an mon thanh soc dai doc theo gio, mat hung gio tan truoc.
// _Dissolve 0 = con nguyen, 1 = tan het. _Wind (xy) = huong gio trong khong gian UV cua sprite.
// Mau, _Dissolve, _Wind dat qua MaterialPropertyBlock vi SpriteRenderer.color khong vao shader tu viet.
Shader "LuHanh/SpriteDissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        [PerRendererData] _SpriteUVRect ("Vung UV sprite (goc, kich thuoc)", Vector) = (0, 0, 1, 1)
        _NoiseTex ("Noise", 2D) = "gray" {}
        _NoiseScale ("Noise Scale", Float) = 1.5
        _StreakRatio ("Streak Ratio", Float) = 10
        [HDR] _Color ("Color", Color) = (0.37, 0.97, 1, 0.6)
        [HDR] _EdgeColor ("Edge Color", Color) = (0.8, 1, 1, 1)
        _EdgeWidth ("Edge Width", Range(0, 0.3)) = 0.12
        _Stretch ("Stretch", Range(0, 1)) = 0.5
        _Wobble ("Wobble", Range(0, 0.1)) = 0.03
        _Directional ("Directional", Range(0, 1)) = 0.35
        _Wind ("Wind", Vector) = (-1, 0, 0, 0)
        _Dissolve ("Dissolve", Range(0, 1)) = 0
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
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);  SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);
            // Unity tu dien; de ngoai CBUFFER de khong pha SRP Batcher 2D.
            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _EdgeColor;
                half _EdgeWidth;
                half _NoiseScale;
                half _StreakRatio;
                half _Stretch;
                half _Wobble;
                half _Directional;
                float4 _Wind;
                float4 _SpriteUVRect;
                half _Dissolve;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 w = normalize(_Wind.xy + float2(1e-4, 0));
                float2 side = float2(-w.y, w.x);
                // Tinh gio/noise trong khung 0..1 rieng, khong tren toan sprite sheet.
                float2 rectSize = max(_SpriteUVRect.zw, float2(1e-6, 1e-6));
                float2 uv = (input.uv - _SpriteUVRect.xy) / rectSize;
                half d = _Dissolve;

                // Noise doi nhanh theo chieu ngang gio, cham doc gio -> chia hinh thanh nhieu soi manh.
                float along = dot(uv, w);
                float across = dot(uv, side);
                half n = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex,
                    float2(along * _NoiseScale, across * _NoiseScale * _StreakRatio)).r;

                // Moi soi bi thoi di mot do dai khac nhau; lay max nhieu mau phia hung gio de
                // than bong keo thanh vet lien, nhat dan ve cuoi (lan khoi).
                float trail = d * _Stretch * (0.3 + 1.4 * n);
                half alpha = 0;
                [unroll] for (int k = 0; k < 6; k++)
                {
                    float f = k / 5.0;
                    float2 suv = uv - w * (trail * f + d * _Stretch * 0.25 * n)
                               + side * sin(along * 14 + n * 6.28 + d * 5) * _Wobble * d * f;
                    // Ngoai khung la trong suot. Kep vao tam texel de loc bilinear
                    // khong doc pixel cua khung ke ben ngay ca tai mep.
                    float inside = step(0.0, suv.x) * step(suv.x, 1.0)
                                 * step(0.0, suv.y) * step(suv.y, 1.0);
                    float2 inset = min(abs(_MainTex_TexelSize.xy) * 0.5, rectSize * 0.5);
                    float2 sampleUV = clamp(_SpriteUVRect.xy + suv * rectSize,
                        _SpriteUVRect.xy + inset, _SpriteUVRect.xy + rectSize - inset);
                    alpha = max(alpha, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, sampleUV).a
                        * inside * (1 - f * 0.85));
                }

                // Mat hung gio (along nho) bi an truoc.
                half v = lerp(n, saturate(along - dot(float2(0.5, 0.5), w) + 0.5), _Directional);
                half t = d * (1 + _EdgeWidth * 2) - _EdgeWidth;
                // Mep tan mem nhu khoi thay vi cat sac.
                half keep = smoothstep(t, t + _EdgeWidth, v);
                half edge = (1 - smoothstep(t + _EdgeWidth, t + _EdgeWidth * 2, v)) * step(0.001, d);
                half4 c = lerp(_Color, _EdgeColor, edge);
                return half4(c.rgb, alpha * keep * c.a * (1 - d * 0.3));
            }
            ENDHLSL
        }
    }
}
