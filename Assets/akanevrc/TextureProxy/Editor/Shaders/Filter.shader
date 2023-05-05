Shader "akanevrc_TextureProxy/Filter"
{
    Properties
    {
        [KeywordEnum(Normal, Replace, Darken, Multiply, ColorBurn, LinearBurn, Lighten, Screen, ColorDodge, LinearDodge, Overlay, SoftLight, HardLight, VividLight, LinearLight, PinLight, HardMix, Difference, Exclusion, Subtract, Divide, Hue, Saturation, HSVColor, Luminosity, DarkerColor, LighterColor)]
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
            #pragma multi_compile _MODE_NORMAL _MODE_REPLACE _MODE_DARKEN _MODE_MULTIPLY _MODE_COLORBURN _MODE_LINEARBURN _MODE_LIGHTEN _MODE_SCREEN _MODE_COLORDODGE _MODE_LINEARDODGE _MODE_OVERLAY _MODE_SOFTLIGHT _MODE_HARDLIGHT _MODE_VIVIDLIGHT _MODE_LINEARLIGHT _MODE_PINLIGHT _MODE_HARDMIX _MODE_DIFFERENCE _MODE_EXCLUSION _MODE_SUBTRACT _MODE_DIVIDE _MODE_HUE _MODE_SATURATION _MODE_HSVCOLOR _MODE_LUMINOSITY _MODE_DARKERCOLOR _MODE_LIGHTERCOLOR

            #include "UnityCG.cginc"
            #include "Filter.cginc"

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
            #elif _MODE_REPLACE
                return BLEND(ca, cb);
            #elif _MODE_DARKEN
                return BLEND(ca, cb);
            #elif _MODE_MULTIPLY
                return BLEND(ca, cb);
            #elif _MODE_COLORBURN
                return BLEND(ca, cb);
            #elif _MODE_LINEARBURN
                return BLEND(ca, cb);
            #elif _MODE_LIGHTEN
                return BLEND(ca, cb);
            #elif _MODE_SCREEN
                return BLEND(ca, cb);
            #elif _MODE_COLORDODGE
                return BLEND(ca, cb);
            #elif _MODE_LINEARDODGE
                return BLEND(ca, cb);
            #elif _MODE_OVERLAY
                return BLEND(ca, cb);
            #elif _MODE_SOFTLIGHT
                return BLEND(ca, cb);
            #elif _MODE_HARDLIGHT
                return BLEND(ca, cb);
            #elif _MODE_VIVIDLIGHT
                return BLEND(ca, cb);
            #elif _MODE_LINEARLIGHT
                return BLEND(ca, cb);
            #elif _MODE_PINLIGHT
                return BLEND(ca, cb);
            #elif _MODE_HARDMIX
                return BLEND(ca, cb);
            #elif _MODE_DIFFERENCE
                return BLEND(ca, cb);
            #elif _MODE_EXCLUSION
                return BLEND(ca, cb);
            #elif _MODE_SUBTRACT
                return BLEND(ca, cb);
            #elif _MODE_DIVIDE
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
