Shader "LuHanh/FoundryWardenFx"
{
    Properties
    {
        _BaseColor ("Màu hiệu ứng", Color) = (0.918,0.396,0.278,1)
        _FxKind ("Loại hiệu ứng", Float) = 0
        _Progress ("Tiến trình", Range(0,1)) = 0
        _Direction ("Hướng", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 local : TEXCOORD0; half4 color : COLOR; };
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _FxKind, _Progress, _Direction;
            CBUFFER_END
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.local = v.positionOS.xy + 0.5;
                o.color = v.color;
                return o;
            }
            float hash(float2 p) { return frac(sin(dot(p, float2(127.1,311.7))) * 43758.5453); }
            half4 frag(Varyings i) : SV_Target
            {
                float time = floor(_Time.y * 16) / 16;
                float2 p = floor(i.local * float2(28,24)) / float2(28,24);
                if (_Direction < 0) p.x = 1 - p.x;
                float flow = hash(float2(floor(p.x * 28), floor(p.y * 24 - time * 18)));
                half3 deep = half3(0.561,0.224,0.239);
                half3 hot = half3(1,0.87,0.72);
                half3 shade = _BaseColor.rgb;
                float alpha = _BaseColor.a;
                if (_FxKind < 0.5)
                {
                    float dash = step(0.25,frac(p.x * 8 - time * 3));
                    shade = lerp(_BaseColor.rgb, hot, step(0.86,frac(p.x * 8 - time * 3)) * 0.3);
                    alpha *= lerp(0.4,1,dash);
                }
                else if (_FxKind < 1.5)
                {
                    float crest = 0.22 + 0.72 * exp(-pow((p.x - 0.67) * 5,2))
                        + 0.24 * exp(-pow((p.x - 0.28) * 7,2));
                    float height = crest + (flow - 0.5) * 0.08;
                    float edge = 1 - smoothstep(0.02,0.11,abs(p.y - height));
                    float core = saturate(1 - p.y / max(0.1,height));
                    shade = lerp(deep,_BaseColor.rgb,saturate(core * 2));
                    shade = lerp(shade,hot,max(edge * 0.9,step(p.y,0.07)));
                    alpha *= max(step(p.y,height),step(0.982,flow) * step(height,p.y));
                }
                else if (_FxKind < 2.5)
                {
                    float width = 0.34 - p.y * 0.12 + (flow - 0.5) * 0.13;
                    float dx = abs(p.x - 0.5);
                    float core = saturate(1 - dx / max(0.05,width));
                    float heat = saturate(core + flow * 0.3 - p.y * 0.45);
                    shade = lerp(deep,_BaseColor.rgb,saturate(heat * 2));
                    shade = lerp(shade,hot,saturate((heat - 0.55) * 2.8));
                    shade = lerp(shade,half3(0.60,0.48,0.66),step(0.75,p.y) * 0.7);
                    alpha *= step(dx,width) * lerp(1,0.55,step(0.7,p.y));
                }
                else if (_FxKind < 3.5)
                {
                    float2 q = float2((p.x - 0.5) * 2,p.y * 1.2);
                    float radius = 0.22 + 0.88 * sqrt(saturate(_Progress));
                    float ring = 1 - step(0.07,abs(length(q) - radius));
                    float pillar = step(abs(p.x - 0.5),0.22 * (1 - _Progress))
                        * step(p.y,0.58 * (1 - _Progress));
                    float floorCrack = step(p.y,0.07 + flow * 0.045);
                    float debris = step(0.97,flow) * step(p.y,radius * 0.8) * (1 - _Progress);
                    shade = lerp(_BaseColor.rgb,hot,max(pillar * 0.6,ring * 0.75));
                    alpha *= max(max(ring,pillar),max(floorCrack,debris));
                }
                else if (_FxKind < 4.5)
                {
                    shade = lerp(_BaseColor.rgb,hot,0.15 + 0.2 * _Progress);
                    alpha *= max(step(p.x,0.16),step(p.y,0.16));
                }
                else if (_FxKind < 5.5)
                {
                    float head = lerp(0.08,0.97,_Progress);
                    float curve = 0.47 + sin(p.x * 3.14159) * 0.17;
                    float band = abs(p.y - curve);
                    float leading = step(abs(p.x - head),0.10);
                    float trail = saturate(1 - (head - p.x) * 1.6) * step(p.x,head + 0.1);
                    float blade = 1 - step(0.105,band);
                    float after = (1 - step(0.035,abs(band - 0.19))) * 0.2;
                    shade = lerp(_BaseColor.rgb,hot,max(leading,1 - saturate(band * 14)));
                    alpha *= max(blade,after) * max(trail,leading);
                    float coreBand = 1 - smoothstep(0.02,0.105,band);
                    float fold = 0.5 + 0.5 * sin(p.x * 28 - time * 15);
                    shade *= lerp(0.6,1.05,max(coreBand,leading));
                    alpha *= max(leading,max(coreBand * 0.9,fold * 0.42));
                }
                else if (_FxKind < 6.5)
                {
                    alpha *= lerp(0.25,0.75,step(0.75,frac((p.x + p.y) * 7 - time)));
                }
                else if (_FxKind < 7.5)
                {
                    float glow = saturate(1 - length((p - 0.5) * float2(2,1.8)));
                    alpha *= floor(glow * 5) / 5;
                }
                else if (_FxKind < 8.5)
                {
                    float2 q = float2((p.x - 0.5) * 2,p.y * 1.2);
                    float radius = 0.3 + sqrt(saturate(_Progress)) * 0.95;
                    float ring = 1 - step(0.04,abs(length(q) - radius));
                    float spokes = step(0.77,frac(atan2(q.y,q.x) * 3.7 + 0.2));
                    float shards = spokes * (1 - step(0.17,abs(length(q) - radius * 0.76)));
                    shade = lerp(_BaseColor.rgb,hot,ring * 0.7);
                    alpha *= max(ring,shards);
                }
                else if (_FxKind < 9.5)
                {
                    float y = frac(p.y - time * 0.35);
                    float wobble = sin(floor(p.y * 8 - time * 4) * 2.5) * 0.12;
                    float cloud = saturate(1 - length((p - float2(0.5 + wobble,0.68)) * float2(2.6,2.1)));
                    float folds = 0.5 + 0.5 * sin(p.y * 18 - time * 8 + sin(p.x * 13) * 2);
                    alpha *= cloud * lerp(0.12,0.8,folds) * (0.6 + flow * 0.4) * step(0.38,p.y);
                    shade = lerp(_BaseColor.rgb,half3(0.78,0.67,0.83),y * 0.5);
                }
                else if (_FxKind < 10.5)
                {
                    // Khiên công nghiệp: viền sáng, lớp giáp mờ, cung năng lượng và tàn lửa.
                    float2 sp = floor(i.local * float2(144,80)) / float2(144,80);
                    float2 q = (sp - 0.5) * 2;
                    float radius = length(q);
                    float angle = atan2(q.y,q.x);
                    float breath = 0.89 + sin(time * 3.2) * 0.008;
                    float ring = 1 - smoothstep(0.009,0.03,abs(radius - breath));
                    float halo = (1 - smoothstep(0.02,0.11,abs(radius - breath))) * 0.24;
                    float arc = pow(saturate(0.5 + 0.5 * sin(angle * 3 - time * 2.1)),6);
                    float innerRing = (1 - smoothstep(0.012,0.035,abs(radius - 0.79))) * arc;
                    float radial = saturate((radius - 0.45) / 0.44);
                    float plates = step(0.91,frac(angle * 4.77465 + 0.5)) * radial * step(radius,0.87);
                    float flecks = step(0.977,hash(floor(sp * float2(144,80)) + floor(time * 5)))
                        * (1 - smoothstep(0.04,0.095,abs(radius - 0.94)));
                    float block = saturate(abs(_Direction) - 1);
                    float impactRing = (1 - smoothstep(0.008,0.035,abs(radius - (0.74 + block * 0.15)))) * block;
                    float ignition = smoothstep(0,0.035,_Progress);
                    float expiry = 1 - smoothstep(0.94,1,_Progress);
                    float intensity = max(ring * (0.65 + arc * 0.35),max(innerRing * 0.65,impactRing));
                    shade = lerp(deep,_BaseColor.rgb,saturate(radial + intensity));
                    shade = lerp(shade,hot,saturate(intensity * (0.45 + block * 0.55) + flecks));
                    alpha *= max(intensity,max(halo,max(plates * 0.18,flecks * 0.8))) * ignition * expiry;
                }
                else
                {
                    // Đầu vệt ở phía boss, sóng dồn nối tiếp lùi về đuôi khi húc.
                    float2 wake = floor(i.local * float2(96,48)) / float2(96,48);
                    if (_Direction < 0) wake.x = 1 - wake.x;
                    float envelope = pow(saturate(wake.x),0.7);
                    float centre = 0.46 + sin(wake.x * 20 + time * 19) * 0.09 * (1 - wake.x);
                    float fold = 0.5 + 0.5 * sin(wake.x * 32 + time * 27);
                    float band = abs(wake.y - centre);
                    float mainWave = 1 - smoothstep(0.016,0.08 + fold * 0.025,band);
                    float upper = 1 - smoothstep(0.01,0.035,abs(wake.y - centre - 0.18 * (1 - wake.x)));
                    float lower = 1 - smoothstep(0.01,0.025,abs(wake.y - centre + 0.23 * (1 - wake.x)));
                    float core = (1 - smoothstep(0.008,0.025,band)) * envelope;
                    shade = lerp(deep,_BaseColor.rgb,fold * 0.7 + 0.3);
                    shade = lerp(shade,hot,core * 0.8);
                    alpha *= max(mainWave * (0.45 + fold * 0.55),max(upper,lower) * 0.3)
                        * envelope * (1 - smoothstep(0.94,1,wake.x));
                }
                return half4(shade * i.color.rgb, alpha * i.color.a);
            }
            ENDHLSL
        }
    }
}
