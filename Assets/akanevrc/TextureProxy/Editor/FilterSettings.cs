using System;
using UnityEngine;

namespace akanevrc.TextureProxy
{
    public enum FilterMode
    {
        Normal = 0,
        Clear,
        Darken,
        Multiply,
        ColorBurn,
        LinearBurn,
        Lighten,
        Screen,
        ColorDodge,
        LinearDodge,
        Overlay,
        SoftLight,
        HardLight,
        VividLight,
        LinearLight,
        PinLight,
        HardMix,
        Difference,
        Exclusion,
        Subtract,
        Divide,
        Hue,
        Saturation,
        HSLColor,
        Luminosity,
        DarkerColor,
        LighterColor,
        ColorCorrection,
        ContrastCorrection
    }

    [Serializable]
    public struct FilterSettings
    {
        public bool toggle;
        public FilterMode mode;
        public Texture2D colorTexture;
        public Vector2 colorTextureScale;
        public Vector2 colorTextureOffset;
        public Texture2D maskTexture;
        public Vector2 maskTextureScale;
        public Vector2 maskTextureOffset;
        public Color color;
        public float hue;
        public float saturation;
        public float luminosity;
        public float contrast;
    }
}
