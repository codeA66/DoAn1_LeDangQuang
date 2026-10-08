Shader "LuHanh/CinderHuskFlame"
{
    Properties
    {
        _Ash ("Than tim den", Color) = (0.047,0.039,0.078,1)
        _Steel ("Vo loi", Color) = (0.161,0.153,0.204,1)
        _Ember ("Vien do sam", Color) = (0.561,0.224,0.239,1)
        _Fire ("Lua do cam", Color) = (0.918,0.396,0.278,1)
        _Orange ("Lua nong", Color) = (1,0.56,0.38,1)
        _Hot ("Tam sang", Color) = (1,0.82,0.74,1)
        _PixelStep ("Buoc pixel", Float) = 0.0135
        _Mode ("Loai", Float) = 0
        _Phase ("Thoi gian", Float) = 0
        _Progress ("Tien trinh no", Float) = 0
        _Direction ("Huong bay", Float) = 1
        _Warning ("Canh bao", Float) = 1
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
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 local : TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
                half4 _Ash, _Steel, _Ember, _Fire, _Orange, _Hot;
                float _PixelStep, _Mode, _Phase, _Progress, _Direction, _Warning;
            CBUFFER_END
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.local = v.positionOS.xy;
                return o;
            }
            float wave(float angle, float r, float time)
            {
                return sin(angle * 5 + r * 8 - time * 9) * 0.55
                    + sin(angle * 9 - r * 6 + time * 7) * 0.3
                    + sin(angle * 17 + time * 4) * 0.15;
            }
            half4 frag(Varyings input) : SV_Target
            {
                float2 p = floor(input.local / _PixelStep) * _PixelStep;
                p.x *= _Direction;
                float r = length(p), a = atan2(p.y, p.x);
                float time = floor(_Phase * 20) / 20;
                if (_Mode > 2.5)
                {
                    float t = saturate(_Progress);
                    float expand = 0.10 + 0.9 * (1-pow(1-t,4));
                    float2 cloud = (p-float2(0.55*expand,0))/float2(0.85*expand,1.05*expand);
                    float d = length(cloud), angle = atan2(cloud.y,cloud.x);
                    float eddy = wave(angle,d,time)*0.09 + sin(p.y*21+time*9)*0.025;
                    float mask = step(d,1+eddy)*step(0,p.x);
                    float heat = 1-d+eddy*2;
                    half3 c = lerp(_Ember.rgb,_Fire.rgb,step(0.08,heat));
                    c = lerp(c,_Orange.rgb,step(0.25,heat));
                    c = lerp(c,_Hot.rgb,step(0.57,heat)*(1-smoothstep(.05,.32,t)));
                    float smoke = smoothstep(.24,.7,t);
                    c = lerp(c,lerp(_Steel.rgb,_Ember.rgb,saturate(heat)),smoke);
                    float grain = frac(sin(dot(floor(p*43),float2(12.9898,78.233)))*43758.5453);
                    float dissolve = step(grain,saturate((1-t)*2.5));
                    return half4(c,mask*dissolve*saturate((1-t)*2));
                }
                if (_Mode < 0.5)
                {
                    float boundary = 0.98 + 0.02 * wave(a, r, time);
                    float body = step(r, boundary);
                    float tailT = saturate((p.x + 2.6) / 1.95);
                    float tailCentre = sin(p.x * 5 + time * 11) * 0.10 * (1-tailT);
                    float tailWidth = pow(tailT,0.8) * (0.42 + sin(p.x*11-time*15)*0.065);
                    float tail = step(abs(p.y-tailCentre),tailWidth) * step(p.x,-0.5) * step(-2.6,p.x);
                    float heat = 1-r + wave(a,r,time)*0.09;
                    half3 c = _Ember.rgb;
                    if (body > 0) {
                        c = lerp(_Fire.rgb,_Orange.rgb,step(0.15,heat));
                        c = lerp(c,_Hot.rgb,step(0.59,heat));
                        float swirl = abs(sin(a*3 + r*8 - time*7));
                        c = lerp(c,_Fire.rgb,step(swirl,0.16)*step(0.3,r));
                        c = lerp(_Ember.rgb,c,step(0.045,heat));
                    } else {
                        float inner = 1-abs(p.y-tailCentre)/max(0.01,tailWidth);
                        c = lerp(_Ember.rgb,_Fire.rgb,step(0.18,inner));
                        c = lerp(c,_Orange.rgb,step(0.66,inner)*tailT);
                    }
                    return half4(c,max(body,tail));
                }
                if (_Mode < 1.5)
                {
                    float boundary = 0.98 + sin(a*12)*0.02;
                    float mask = step(r,boundary);
                    half3 c = lerp(_Ash.rgb,_Steel.rgb,saturate(p.y*.6+.45));
                    float rim = step(0.76,r)*step(r,0.90);
                    float teeth = step(0.55,abs(sin(a*6)));
                    c = lerp(c,_Ember.rgb,rim*teeth*0.75);
                    float crack = step(abs(sin(a*3 + r*13)),0.09)*step(r,0.77);
                    float centre = step(r,0.38 + wave(a,r,time)*0.025);
                    float slits = step(abs(sin(a*4)),0.20)*step(0.36,r)*step(r,0.70);
                    half3 heat = lerp(_Fire.rgb,_Hot.rgb,step(r,0.18));
                    c = lerp(c,heat*_Warning,max(centre,max(crack,slits)));
                    return half4(c,mask);
                }
                float t = saturate(_Progress);
                float expansion = 1-pow(1-t,3);
                float ringR = 0.1+expansion*0.87;
                float ring = step(abs(r-ringR),0.025+(1-t)*0.035);
                float flameR = expansion*(0.76 + wave(a,r,time)*0.11);
                float flame = step(r,flameR)*step(0.03,t);
                float hole = step(r,max(0,t-0.28)*1.4);
                float heat = saturate((flameR-r)/max(0.05,flameR));
                half3 c = lerp(_Fire.rgb,_Orange.rgb,step(.15,heat));
                c = lerp(c,_Hot.rgb,step(.5,heat));
                c = lerp(c,_Hot.rgb,ring);
                float alpha=max(ring,flame*(1-hole))*saturate((1-t)*1.8);
                return half4(c,alpha);
            }
            ENDHLSL
        }
    }
}
