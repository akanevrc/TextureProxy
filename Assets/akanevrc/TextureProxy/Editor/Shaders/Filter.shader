Shader "akanevrc_TextureProxy/Filter"
{
    Properties
    {
        [KeywordEnum(Normal, Clear, Darken, Multiply, ColorBurn, LinearBurn, Lighten, Screen, ColorDodge, LinearDodge, Overlay, SoftLight, HardLight, VividLight, LinearLight, PinLight, HardMix, Difference, Exclusion, Subtract, Divide, Hue, Saturation, HSVColor, Luminosity, DarkerColor, LighterColor)]
        _Mode ("Filter Mode", Float) = 0

        _MainTex ("Main Texture", 2D) = "white" {}
        _SubTex ("Sub Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0, 0, 0, 0)
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
            #pragma multi_compile _MODE_NORMAL _MODE_CLEAR _MODE_DARKEN _MODE_MULTIPLY _MODE_COLORBURN _MODE_LINEARBURN _MODE_LIGHTEN _MODE_SCREEN _MODE_COLORDODGE _MODE_LINEARDODGE _MODE_OVERLAY _MODE_SOFTLIGHT _MODE_HARDLIGHT _MODE_VIVIDLIGHT _MODE_LINEARLIGHT _MODE_PINLIGHT _MODE_HARDMIX _MODE_DIFFERENCE _MODE_EXCLUSION _MODE_SUBTRACT _MODE_DIVIDE _MODE_HUE _MODE_SATURATION _MODE_HSVCOLOR _MODE_LUMINOSITY _MODE_DARKERCOLOR _MODE_LIGHTERCOLOR

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uvMain : TEXCOORD0;
                float2 uvSub : TEXCOORD1;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _SubTex;
            float4 _SubTex_ST;
            half4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uvMain = TRANSFORM_TEX(v.uv, _MainTex);
                o.uvSub = TRANSFORM_TEX(v.uv, _SubTex);
                return o;
            }

            #define BLEND(ca, cb) half4(ca.rgb * ca.a + cb.rgb * (1 - ca.a), ca.a + cb.a * (1 - ca.a))

            fixed4 frag(v2f i) : SV_Target
            {
                half4 sub = tex2D(_SubTex, i.uvSub);
                half4 ca = half4(sub.rgb * sub.a * _Color.rgb * _Color.a, sub.a * _Color.a);
                half4 cb = tex2D(_MainTex, i.uvMain);
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
                return BLEND(ca, cb);
            #elif _MODE_SATURATION
                return BLEND(ca, cb);
            #elif _MODE_HSVCOLOR
                return BLEND(ca, cb);
            #elif _MODE_LUMINOSITY
                return BLEND(ca, cb);
            #elif _MODE_DARKERCOLOR
                return BLEND(ca, cb);
            #elif _MODE_LIGHTERCOLOR
                return BLEND(ca, cb);
            #endif
            }
            ENDCG
        }
    }
    Fallback "Legacy Shaders/Transparent/Diffuse"
}
