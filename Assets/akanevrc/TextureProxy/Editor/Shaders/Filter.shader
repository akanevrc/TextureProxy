Shader "akanevrc_TextureProxy/Filter"
{
    Properties
    {
        [KeywordEnum(Normal, Clear, Darken, Multiply, ColorBurn, LinearBurn, Lighten, Screen, ColorDodge, LinearDodge, Overlay, SoftLight, HardLight, VividLight, LinearLight, PinLight, HardMix, Difference, Exclusion, Subtract, Divide, Hue, Saturation, HSLColor, Luminosity, DarkerColor, LighterColor, ColorCorrection, ContrastCorrection)]
        _Mode ("Filter Mode", Float) = 0

        _MainTex ("Main Texture", 2D) = "white" {}
        _SubTex ("Sub Texture", 2D) = "white" {}
        _Mask ("Mask", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Hue ("Hue", Float) = 0
        _Saturation ("Saturation", Float) = 0
        _Luminosity ("Luminosity", Float) = 0
        _Contrast ("Contrast", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "DisableBatching"="True" }
        LOD 100
        Cull Off
        ZWrite Off
        ZTest Off

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _MODE_NORMAL _MODE_CLEAR _MODE_DARKEN _MODE_MULTIPLY _MODE_COLORBURN _MODE_LINEARBURN _MODE_LIGHTEN _MODE_SCREEN _MODE_COLORDODGE _MODE_LINEARDODGE _MODE_OVERLAY _MODE_SOFTLIGHT _MODE_HARDLIGHT _MODE_VIVIDLIGHT _MODE_LINEARLIGHT _MODE_PINLIGHT _MODE_HARDMIX _MODE_DIFFERENCE _MODE_EXCLUSION _MODE_SUBTRACT _MODE_DIVIDE _MODE_HUE _MODE_SATURATION _MODE_HSLCOLOR _MODE_LUMINOSITY _MODE_DARKERCOLOR _MODE_LIGHTERCOLOR _MODE_COLORCORRECTION _MODE_CONTRASTCORRECTION

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _SubTex;
            float4 _SubTex_ST;
            sampler2D _Mask;
            float4 _Mask_ST;
            half4 _Color;
            half _Hue;
            half _Saturation;
            half _Luminosity;
            half _Contrast;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            #define BLEND(ca, cb) half4(ca.rgb * ca.a + cb.rgb * (1 - ca.a), ca.a + cb.a * (1 - ca.a))

            #define HSL_H(c) (c.r == c.g && c.g == c.b ? 0 : c.r >= c.g && c.g >= c.b ? 60 * (c.g - c.b) / (c.r - c.b) : c.r >= c.b && c.b >= c.g ? 60 * (c.g - c.b) / (c.r - c.g) : c.g >= c.r && c.r >= c.b ? 60 * (c.b - c.r) / (c.g - c.b) + 120 : c.g >= c.b && c.b >= c.r ? 60 * (c.b - c.r) / (c.g - c.r) + 120 : c.b >= c.r && c.r >= c.g ? 60 * (c.r - c.g) / (c.b - c.g) + 240 : 60 * (c.r - c.g) / (c.b - c.r) + 240)
            #define _HSL_S_MAX(c) (max(c.r, max(c.g, c.b)))
            #define _HSL_S_MIN(c) (min(c.r, min(c.g, c.b)))
            #define HSL_S(c) (c.r == c.g && c.g == c.b ? 0 : _HSL_S_MAX(c) + _HSL_S_MIN(c) <= 1 ? (_HSL_S_MAX(c) - _HSL_S_MIN(c)) / (_HSL_S_MAX(c) + _HSL_S_MIN(c)) : (_HSL_S_MAX(c) - _HSL_S_MIN(c)) / (2 - _HSL_S_MAX(c) - _HSL_S_MIN(c)))
            #define HSL_L(c) ((max(c.r, max(c.g, c.b)) + min(c.r, min(c.g, c.b))) * 0.5)
            #define _HSL_MAX(s, l) ((l) <= 0.5 ? (l) + (s) * (l) : (l) + (s) * (1 - (l)))
            #define _HSL_MIN(s, l) ((l) <= 0.5 ? (l) - (s) * (l) : (l) - (s) * (1 - (l)))
            #define _HSL_MID(h, s, l) ((h) * (_HSL_MAX(s, l) - _HSL_MIN(s, l)) + _HSL_MIN(s, l))
            #define HSL_RGB(h, s, l) ((h) < 60 ? half3(_HSL_MAX(s, l), _HSL_MID((h) / 60, s, l), _HSL_MIN(s, l)) : (h) < 120 ? half3(_HSL_MID((120 - (h)) / 60, s, l), _HSL_MAX(s, l), _HSL_MIN(s, l)) : (h) < 180 ? half3(_HSL_MIN(s, l), _HSL_MAX(s, l), _HSL_MID(((h) - 120) / 60, s, l)) : (h) < 240 ? half3(_HSL_MIN(s, l), _HSL_MID((240 - (h)) / 60, s, l), _HSL_MAX(s, l)) : (h) < 300 ? half3(_HSL_MID(((h) - 240) / 60, s, l), _HSL_MIN(s, l), _HSL_MAX(s, l)) : half3(_HSL_MAX(s, l), _HSL_MIN(s, l), _HSL_MID((360 - (h)) / 60, s, l)))

            fixed4 frag(v2f i) : SV_Target
            {
                half4 sub = tex2D(_SubTex, TRANSFORM_TEX(i.uv, _SubTex));
                half4 mask = tex2D(_Mask, TRANSFORM_TEX(i.uv, _Mask));
                half4 ca = half4(sub.rgb * sub.a * _Color.rgb * _Color.a, sub.a * mask.r * _Color.a);
                half4 cb = tex2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex));
            #ifdef _MODE_NORMAL
                return BLEND(ca, cb);
            #elif _MODE_CLEAR
                return half4(cb.rgb, ca.a * cb.a);
            #elif _MODE_DARKEN
                ca = half4(min(ca.rgb, cb.rgb), ca.a);
                return BLEND(ca, cb);
            #elif _MODE_MULTIPLY
                ca = half4(ca.rgb * cb.rgb, ca.a);
                return BLEND(ca, cb);
            #elif _MODE_COLORBURN
                ca = half4(saturate(1 - (1 - cb.rgb) / ca.rgb), ca.a);
                return BLEND(ca, cb);
            #elif _MODE_LINEARBURN
                ca = half4(saturate(ca.rgb + cb.rgb - 1), ca.a);
                return BLEND(ca, cb);
            #elif _MODE_LIGHTEN
                ca = half4(max(ca.rgb, cb.rgb), ca.a);
                return BLEND(ca, cb);
            #elif _MODE_SCREEN
                ca = half4(1 - (1 - ca.rgb) * (1 - cb.rgb), ca.a);
                return BLEND(ca, cb);
            #elif _MODE_COLORDODGE
                ca = half4(saturate(cb.rgb / (1 - ca.rgb)), ca.a);
                return BLEND(ca, cb);
            #elif _MODE_LINEARDODGE
                ca = half4(saturate(ca.rgb + cb.rgb), ca.a);
                return BLEND(ca, cb);
            #elif _MODE_OVERLAY
                ca = half4
                (
                    cb.r < 0.5 ? 2 * ca.r * cb.r : 1 - 2 * (1 - ca.r) * (1 - cb.r),
                    cb.g < 0.5 ? 2 * ca.g * cb.g : 1 - 2 * (1 - ca.g) * (1 - cb.g),
                    cb.b < 0.5 ? 2 * ca.b * cb.b : 1 - 2 * (1 - ca.b) * (1 - cb.b),
                    ca.a
                );
                return BLEND(ca, cb);
            #elif _MODE_SOFTLIGHT
                ca = half4
                (
                    ca.r < 0.5 ? 2 * (ca.r - 1) * (cb.r - pow(cb.r, 2)) + cb.r : 2 * (ca.r - 1) * (sqrt(cb.r) - cb.r) + cb.r,
                    ca.g < 0.5 ? 2 * (ca.g - 1) * (cb.g - pow(cb.g, 2)) + cb.g : 2 * (ca.g - 1) * (sqrt(cb.g) - cb.g) + cb.g,
                    ca.b < 0.5 ? 2 * (ca.b - 1) * (cb.b - pow(cb.b, 2)) + cb.b : 2 * (ca.b - 1) * (sqrt(cb.b) - cb.b) + cb.b,
                    ca.a
                );
                return BLEND(ca, cb);
            #elif _MODE_HARDLIGHT
                ca = half4
                (
                    ca.r < 0.5 ? 2 * ca.r * cb.r : 1 - 2 * (1 - ca.r) * (1 - cb.r),
                    ca.g < 0.5 ? 2 * ca.g * cb.g : 1 - 2 * (1 - ca.g) * (1 - cb.g),
                    ca.b < 0.5 ? 2 * ca.b * cb.b : 1 - 2 * (1 - ca.b) * (1 - cb.b),
                    ca.a
                );
                return BLEND(ca, cb);
            #elif _MODE_VIVIDLIGHT
                ca = half4
                (
                    saturate(ca.r < 0.5 ? 1 - (1 - cb.r) / (2 * ca.r) : cb.r / (2 * (1 - ca.r))),
                    saturate(ca.g < 0.5 ? 1 - (1 - cb.g) / (2 * ca.g) : cb.g / (2 * (1 - ca.g))),
                    saturate(ca.b < 0.5 ? 1 - (1 - cb.b) / (2 * ca.b) : cb.b / (2 * (1 - ca.b))),
                    ca.a
                );
                return BLEND(ca, cb);
            #elif _MODE_LINEARLIGHT
                ca = half4
                (
                    saturate(ca.r + 2 * cb.r - 1),
                    saturate(ca.g + 2 * cb.g - 1),
                    saturate(ca.b + 2 * cb.b - 1),
                    ca.a
                );
                return BLEND(ca, cb);
            #elif _MODE_PINLIGHT
                ca = half4
                (
                    saturate(ca.r + 2 * cb.r - 1),
                    saturate(ca.g + 2 * cb.g - 1),
                    saturate(ca.b + 2 * cb.b - 1),
                    ca.a
                );
                return BLEND(ca, cb);
            #elif _MODE_HARDMIX
                ca = half4
                (
                    ca.r < cb.r - 1 ? 0 : 1,
                    ca.g < cb.g - 1 ? 0 : 1,
                    ca.b < cb.b - 1 ? 0 : 1,
                    ca.a
                );
                return BLEND(ca, cb);
            #elif _MODE_DIFFERENCE
                ca = half4(abs(ca.rgb - cb.rgb), ca.a);
                return BLEND(ca, cb);
            #elif _MODE_EXCLUSION
                ca = half4(ca.rgb + cb.rgb - 2 * ca.rgb * cb.rgb, ca.a);
                return BLEND(ca, cb);
            #elif _MODE_SUBTRACT
                ca = half4(saturate(cb.rgb - ca.rgb), ca.a);
                return BLEND(ca, cb);
            #elif _MODE_DIVIDE
                ca = half4(saturate(cb.rgb / ca.rgb), ca.a);
                return BLEND(ca, cb);
            #elif _MODE_HUE
                ca = half4(HSL_RGB(HSL_H(ca), HSL_S(cb), HSL_L(cb)), ca.a);
                return BLEND(ca, cb);
            #elif _MODE_SATURATION
                ca = half4(HSL_RGB(HSL_H(cb), HSL_S(ca), HSL_L(cb)), ca.a);
                return BLEND(ca, cb);
            #elif _MODE_HSLCOLOR
                ca = half4(HSL_RGB(HSL_H(ca), HSL_S(ca), HSL_L(cb)), ca.a);
                return BLEND(ca, cb);
            #elif _MODE_LUMINOSITY
                ca = half4(HSL_RGB(HSL_H(cb), HSL_S(cb), HSL_L(ca)), ca.a);
                return BLEND(ca, cb);
            #elif _MODE_DARKERCOLOR
                ca = half4(HSL_L(ca) < HSL_L(cb) ? ca.rgb : cb.rgb, ca.a);
                return BLEND(ca, cb);
            #elif _MODE_LIGHTERCOLOR
                ca = half4(HSL_L(ca) > HSL_L(cb) ? ca.rgb : cb.rgb, ca.a);
                return BLEND(ca, cb);
            #elif _MODE_COLORCORRECTION
                ca = half4(HSL_RGB(((HSL_H(cb) + _Hue) % 360 + 360) % 360, saturate(HSL_S(cb) + _Saturation), saturate(HSL_L(cb) + _Luminosity)), ca.a);
                return BLEND(ca, cb);
            #elif _MODE_CONTRASTCORRECTION
                ca = half4(HSL_RGB(HSL_H(cb), HSL_S(cb), saturate((HSL_L(cb) - 0.5) * (_Contrast + 1) + 0.5 + _Luminosity)), ca.a);
                return BLEND(ca, cb);
            #endif
            }
            ENDCG
        }
    }
    Fallback "Legacy Shaders/Transparent/Diffuse"
}
