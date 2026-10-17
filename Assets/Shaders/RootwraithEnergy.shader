// Nang luong cua boss Rootwraith, ve hoan toan bang toan (khong can anh) tren mot sprite vuong trang.
// _Shape: 0 = cau (dan), 1 = vet canh bao ngang (gai sap troi), 2 = vong song xung kich, 3 = vet lao (duoi dai).
// Cong sang (Additive) + nhieu chop chay theo thoi gian -> cam giac lua/nang luong song.
Shader "LuHanh/RootwraithEnergy"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Shape ("Shape", Float) = 0
        _CoreColor ("Core", Color) = (1, 0.93, 0.85, 1)
        _RimColor ("Rim", Color) = (0.92, 0.4, 0.28, 1)
        _Intensity ("Intensity", Range(0, 6)) = 2
        _Flicker ("Flicker", Range(0, 1)) = 0.35
        _Thickness ("Ring thickness", Range(0.01, 0.5)) = 0.12
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

        Blend SrcAlpha One
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

            CBUFFER_START(UnityPerMaterial)
                half _Shape;
                half4 _CoreColor;
                half4 _RimColor;
                half _Intensity;
                half _Flicker;
                half _Thickness;
            CBUFFER_END

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float noise(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                f = f * f * (3 - 2 * f);
                float a = hash21(i), b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1)), d = hash21(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = input.uv;
                o.color = input.color;
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float2 p = uv * 2 - 1;
                float t = _Time.y;
                float n = noise(uv * 6 + float2(t * 2.3, -t * 3.1));
                float flick = 1 - _Flicker + _Flicker * n;
                float core, glow;

                if (_Shape < 0.5)          // cau
                {
                    float r = length(p) * (0.9 + 0.2 * n);
                    core = saturate(1 - r / 0.45);
                    glow = saturate(1 - r);
                    glow *= glow;
                }
                else if (_Shape < 1.5)     // vet canh bao: day sang o day, mo len tren, hai dau mo
                {
                    float ends = saturate(1 - abs(p.x));
                    ends = sqrt(ends);
                    float h = uv.y;
                    core = saturate(1 - h / 0.18) * ends;
                    glow = saturate(1 - h) * ends * (0.6 + 0.4 * n);
                    glow *= glow;
                }
                else if (_Shape < 2.5)     // vong
                {
                    float r = length(p);
                    float d = abs(r - (1 - _Thickness));
                    core = saturate(1 - d / (_Thickness * 0.35));
                    glow = saturate(1 - d / _Thickness) * step(r, 1);
                    glow *= glow;
                }
                else                       // vet lao: dau sang ben phai, duoi mo dan sang trai
                {
                    float along = uv.x;
                    float width = 1 - abs(p.y) / max(0.05, along);
                    width = saturate(width);
                    float wisps = noise(float2(uv.x * 10 - t * 9, uv.y * 4));
                    core = saturate(width * 2 - 1) * along * along;
                    glow = width * along * (0.5 + 0.5 * wisps);
                }

                half3 col = lerp(_RimColor.rgb, _CoreColor.rgb, saturate(core));
                half a = saturate(max(core, glow)) * flick * input.color.a;
                return half4(col * input.color.rgb * _Intensity, a);
            }
            ENDHLSL
        }
    }
}
