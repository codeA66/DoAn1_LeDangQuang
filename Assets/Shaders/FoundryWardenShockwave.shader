Shader "LuHanh/FoundryWardenShockwave"
{
    Properties
    {
        _BaseColor ("Lửa xung kích", Color) = (0.918,0.396,0.278,1)
        _FxKind ("Sóng đất hoặc đập", Float) = 1
        _Progress ("Tiến trình", Range(0,1)) = 0
        _Direction ("Hướng", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; float4 color : COLOR; };
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
            float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7))) * 43758.5453); }
            half4 frag(Varyings i) : SV_Target
            {
                float2 p = floor(i.local * float2(30,24)) / float2(30,24);
                if (_Direction < 0) p.x = 1 - p.x;
                float phase = floor(_Time.y * 18) / 18;
                float noise = hash(float2(floor(p.x * 30),floor(p.y * 24 - phase * 14)));
                float mask, heat, density;
                if (_FxKind < 2)
                {
                    float crest = 0.18 + 0.76 * exp(-pow((p.x - 0.69) * 4.5,2));
                    crest += (noise - 0.5) * 0.07;
                    float edge = 1 - step(0.065,abs(p.y - crest));
                    mask = step(p.y,crest);
                    heat = max(edge,saturate(1 - p.y / max(0.1,crest)) * 0.7);
                    float fold = 0.5 + 0.5 * sin(p.x * 23 - phase * 13 + p.y * 9);
                    density = max(edge,lerp(0.22,0.8,fold) * saturate(1 - p.y / max(0.1,crest)));
                }
                else
                {
                    float2 q = float2((p.x - 0.5) * 2,p.y * 1.2);
                    float r = length(q);
                    float radius = 0.18 + sqrt(saturate(_Progress)) * 0.94;
                    float ring = 1 - step(0.065,abs(r - radius));
                    float ring2 = (1 - step(0.04,abs(r - radius * 0.6))) * 0.65;
                    float flash = step(p.y,0.48 * (1 - _Progress))
                        * step(abs(p.x - 0.5),0.22 * (1 - _Progress));
                    float cracks = step(p.y,0.08 + noise * 0.045);
                    mask = max(max(ring,ring2),max(flash,cracks));
                    heat = max(ring,max(flash * 0.65,ring2));
                    density = max(max(ring,ring2 * 0.8),max(flash * 0.92,cracks * lerp(0.25,0.65,noise)));
                }
                half3 ember = half3(0.561,0.224,0.239);
                half3 hot = half3(1,0.88,0.72);
                half3 color = lerp(ember,_BaseColor.rgb,saturate(heat * 2));
                color = lerp(color,hot,saturate(heat - 0.35) * 1.2);
                return half4(color * i.color.rgb,mask * density * _BaseColor.a * i.color.a);
            }
            ENDHLSL
        }
    }
}
