using System;
using UnityEngine;

namespace akanevrc.TextureProxy
{
    public enum PixelFilterMode
    {
        Normal = 0,
        Multiply
    }

    [Serializable]
    public struct PixelFilterSettings
    {
        public bool toggle;
        public PixelFilterMode mode;
        public float r;
        public float g;
        public float b;
        public float a;

        public Color ParamsToColor()
        {
            return new Color(this.r, this.g, this.b, this.a);
        }
    }

    public static class PixelFilter
    {
        public static Color[] Filter(PixelFilterSettings settings, Color[] pixels)
        {
            var filtered = (Color[])pixels.Clone();
            switch (settings.mode)
            {
                case PixelFilterMode.Normal:
                {
                    var filter = settings.ParamsToColor();
                    for (var i = 0; i < pixels.Length; i++)
                    {
                        filtered[i] = Normal(filter, pixels[i]);
                    }
                    return filtered;
                }
                case PixelFilterMode.Multiply:
                {
                    var filter = settings.ParamsToColor();
                    for (var i = 0; i < pixels.Length; i++)
                    {
                        filtered[i] = Multiply(filter, pixels[i]);
                    }
                    return filtered;
                }
                default:
                    throw new NotSupportedException();
            }
        }

        private static Color BlendAlpha(Color c, Color c1, Color c0)
        {
            return new Color
            (
                c.r + c0.r * c0.a * (1F - c1.a),
                c.g + c0.g * c0.a * (1F - c1.a),
                c.b + c0.b * c0.a * (1F - c1.a),
                c.a + c0.a * (1F - c1.a)
            );
        }

        private static Color Normal(Color c1, Color c0)
        {
            return BlendAlpha(
                new Color
                (
                    c1.r * c1.a,
                    c1.g * c1.a,
                    c1.b * c1.a,
                    c1.a
                ),
                c1,
                c0
            );
        }

        private static Color Multiply(Color c1, Color c0)
        {
            return BlendAlpha(
                new Color
                (
                    c1.r * c1.a * c0.r * c0.a,
                    c1.g * c1.a * c0.g * c0.a,
                    c1.b * c1.a * c0.b * c0.a,
                    c1.a * c0.a
                ),
                c1,
                c0
            );
        }
    }
}
