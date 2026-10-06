// Lop phu toan man hinh co mot lo tron trong suot (iris): ngoai vong tron la mau _Color, trong vong la trong suot.
// _Center: tam lo theo toa do man hinh 0..1. _Radius: ban kinh tinh theo chieu cao man hinh (2 = mo het, 0 = dong kin).
Shader "LuHanh/UIIris"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Color", Color) = (0.02, 0.01, 0.05, 1)
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius", Float) = 2
        _Softness ("Softness", Float) = 0.02
    }

    SubShader
    {
        Tags { "Queue" = "Overlay" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }
        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float4 screen : TEXCOORD0; };

            fixed4 _Color;
            float4 _Center;
            float _Radius;
            float _Softness;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screen = ComputeScreenPos(o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.screen.xy / i.screen.w;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float d = length((uv - _Center.xy) * float2(aspect, 1));
                float a = smoothstep(_Radius, _Radius + _Softness, d);
                return fixed4(_Color.rgb, _Color.a * a);
            }
            ENDCG
        }
    }
}
