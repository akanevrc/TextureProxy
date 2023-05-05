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
        HSVColor,
        Luminosity,
        DarkerColor,
        LighterColor
    }

    [Serializable]
    public struct FilterSettings
    {
        public bool toggle;
        public FilterMode mode;
        public Color color;
    }
}
