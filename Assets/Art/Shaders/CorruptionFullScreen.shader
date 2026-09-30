// Hieu ung Nhiem toan man hinh - GDD muc 10:
// "nhieu hinh, vien toi, mau tim den lan rong", nang dan theo muc Nhiem.
//
// Can bang do manh theo GDD muc 5:
//   0-49  gan nhu khong thay gi
//   50-79 nhuom tim va toi vien, van doc duoc man hinh de choi tiep
//   80-99 meo hinh va giat dai ro rang, nhung boss van phai danh duoc
Shader "LuHanh/CorruptionFullScreen"
{
    Properties
    {
        _Corruption   ("Muc Nhiem 0-1", Range(0,1)) = 0
        _CorruptColor ("Mau Nhiem", Color) = (0.42, 0.13, 0.55, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            Name "CorruptionPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float  _Corruption;
            float4 _CorruptColor;

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                float  c  = saturate(_Corruption);
                float  t  = _Time.y;

                // Mau va vien di theo c; meo hinh, giat dai va hat nhieu di theo c*c
                // nen chung chi thuc su xuat hien o nguong cao.
                float hard = c * c;

                float2 centre = uv - 0.5;
                float  r      = length(centre);

                // 1. Meo hinh: mot song ngang cham, bien do rat nho.
                //    Bo song tan so cao vi tren nen gradient no hien ra thanh soc doc.
                float wob = sin(uv.y * 13.0 + t * 1.7) * 0.0022;
                uv.x += wob * hard;

                // 2. Giat ngang tung dai - chi bat dau tu khoang Nhiem 70 tro len
                float band   = floor(uv.y * 24.0);
                float glitch = step(0.997 - hard * 0.03, Hash21(float2(band, floor(t * 9.0))));
                uv.x += glitch * (Hash21(float2(band, t)) - 0.5) * 0.028 * hard;

                // 3. Tach kenh mau theo huong xuyen tam, manh dan ra ria
                float2 ca = normalize(centre + 1e-5) * (0.0009 + r * 0.0026) * c;
                half4 col;
                col.r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + ca).r;
                col.g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv     ).g;
                col.b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv - ca).b;
                col.a = 1;

                // 4. Do lai mau theo kieu duotone: bong toi ve tim gan den, vung sang ve tim.
                //    Anh xa theo do sang nen tuong phan khong bi bet nhu cach nhan truc tiep.
                const float3 LUMA = float3(0.299, 0.587, 0.114);
                float  lum    = dot(col.rgb, LUMA);
                float3 shadow = float3(0.020, 0.010, 0.055);
                float3 light  = saturate(_CorruptColor.rgb * 1.7);
                float3 graded = lerp(shadow, light, saturate(lum * 1.15));

                col.rgb = lerp(col.rgb, graded, 0.38 * c);

                // 5. Vien toi khep dan vao giua
                float vig = 1.0 - smoothstep(0.30, 0.90, r) * (0.12 + 0.40 * c);
                col.rgb *= vig;

                // 6. Hat nhieu - lay theo o 2x2 pixel de khoi tao van moire
                float2 grainUV = floor(uv * _ScreenParams.xy * 0.5);
                float  grain   = Hash21(grainUV + floor(t * 24.0)) - 0.5;
                col.rgb += grain * 0.045 * hard;

                return col;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
