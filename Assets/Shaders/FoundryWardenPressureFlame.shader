Shader "LuHanh/FoundryWardenPressureFlame"
{
    Properties
    {
        _BaseColor ("Lửa đỏ cam", Color) = (0.918,0.396,0.278,1)
        _Progress ("Tiến trình xả áp", Range(0,1)) = 0
        _Direction ("Hướng", Float) = 1
        _FxKind ("Loại hiệu ứng", Float) = 2
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
                float _Progress, _Direction, _FxKind;
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
                float2 p = floor(i.local * float2(22,36)) / float2(22,36);
                float phase = floor(_Time.y * 20) / 20 + _Progress * 0.7;
                float rise = smoothstep(0,0.12,_Progress);
                float pulse = pow(0.5 + 0.5 * sin(phase * 19 - p.y * 9),2);
                float noise = hash(float2(floor(p.x * 22),floor(p.y * 36 - phase * 30)));
                float curl = sin(p.y * 10 - phase * 12) * 0.075 * p.y;
                float bulge = exp(-pow((p.y - 0.47) * 3.8,2)) * (0.13 + pulse * 0.13);
                float width = min(0.47,(0.15 + (1 - p.y) * 0.08 + bulge + (noise - 0.5) * 0.05)
                    * lerp(0.6,1,rise));
                float dx = abs(p.x - 0.5 - curl);
                float body = step(dx,width);
                float heat = saturate(1 - dx / max(0.01,width) + noise * 0.3 - p.y * 0.32);
                float core = smoothstep(0.45,0.93,heat);
                half3 ember = half3(0.561,0.224,0.239);
                half3 hot = half3(1,0.90,0.77);
                half3 fire = lerp(ember,_BaseColor.rgb,saturate(heat * 2));
                fire = lerp(fire,hot,core * (0.8 + pulse * 0.2));
                float tongues = step(0.90,noise) * step(0.6,p.y) * step(dx,width + 0.04);
                float shell = saturate(1 - dx / max(0.01,width));
                float folds = 0.5 + 0.5 * sin(p.y * 22 - phase * 16 + sin(p.x * 12) * 2.5);
                float density = lerp(0.18,0.94,smoothstep(0.05,0.68,shell));
                density *= lerp(0.55,1,folds);
                density = max(density,core * 0.98);
                fire *= lerp(0.65,1.08,folds);
                float alpha = _BaseColor.a * max(body * density,tongues * 0.35);
                return half4(fire * i.color.rgb,alpha * i.color.a);
            }
            ENDHLSL
        }
    }
}
