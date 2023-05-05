using System;
using System.Collections.Generic;
using UnityEngine;

namespace akanevrc.TextureProxy
{
    public enum PixelFilterMode
    {
        Normal = 0,
        Replace,
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
    public struct PixelFilterSettings
    {
        public bool toggle;
        public PixelFilterMode mode;
        public Color color;
    }

    public static class PixelFilter
    {
        public static Color[] FilterAll(IEnumerable<PixelFilterSettings> settingsList, Color[] pixels)
        {
            foreach (var settings in settingsList)
            {
                if (settings.toggle)
                {
                    pixels = PixelFilter.Filter(settings, pixels);
                }
            }
            return pixels;
        }

        public static Color[] Filter(PixelFilterSettings settings, Color[] pixels)
        {
            switch (settings.mode)
            {
                case PixelFilterMode.Normal:
                    return OnePixelFilter(settings, pixels, Normal);
                case PixelFilterMode.Replace:
                    return OnePixelFilter(settings, pixels, Replace);
                case PixelFilterMode.Darken:
                    return OnePixelFilter(settings, pixels, Darken);
                case PixelFilterMode.Multiply:
                    return OnePixelFilter(settings, pixels, Multiply);
                case PixelFilterMode.ColorBurn:
                    return OnePixelFilter(settings, pixels, ColorBurn);
                case PixelFilterMode.LinearBurn:
                    return OnePixelFilter(settings, pixels, LinearBurn);
                case PixelFilterMode.Lighten:
                    return OnePixelFilter(settings, pixels, Lighten);
                case PixelFilterMode.Screen:
                    return OnePixelFilter(settings, pixels, Screen);
                case PixelFilterMode.ColorDodge:
                    return OnePixelFilter(settings, pixels, ColorDodge);
                case PixelFilterMode.LinearDodge:
                    return OnePixelFilter(settings, pixels, LinearDodge);
                case PixelFilterMode.Overlay:
                    return OnePixelFilter(settings, pixels, Overlay);
                case PixelFilterMode.SoftLight:
                    return OnePixelFilter(settings, pixels, SoftLight);
                case PixelFilterMode.HardLight:
                    return OnePixelFilter(settings, pixels, HardLight);
                case PixelFilterMode.VividLight:
                    return OnePixelFilter(settings, pixels, VividLight);
                case PixelFilterMode.LinearLight:
                    return OnePixelFilter(settings, pixels, LinearLight);
                case PixelFilterMode.PinLight:
                    return OnePixelFilter(settings, pixels, PinLight);
                case PixelFilterMode.HardMix:
                    return OnePixelFilter(settings, pixels, HardMix);
                case PixelFilterMode.Difference:
                    return OnePixelFilter(settings, pixels, Difference);
                case PixelFilterMode.Exclusion:
                    return OnePixelFilter(settings, pixels, Exclusion);
                case PixelFilterMode.Subtract:
                    return OnePixelFilter(settings, pixels, Subtract);
                case PixelFilterMode.Divide:
                    return OnePixelFilter(settings, pixels, Divide);
                case PixelFilterMode.Hue:
                    return OnePixelFilter(settings, pixels, Hue);
                case PixelFilterMode.Saturation:
                    return OnePixelFilter(settings, pixels, Saturation);
                case PixelFilterMode.HSVColor:
                    return OnePixelFilter(settings, pixels, HSVColor);
                case PixelFilterMode.Luminosity:
                    return OnePixelFilter(settings, pixels, Luminosity);
                case PixelFilterMode.DarkerColor:
                    return OnePixelFilter(settings, pixels, DarkerColor);
                case PixelFilterMode.LighterColor:
                    return OnePixelFilter(settings, pixels, LighterColor);
                default:
                    throw new NotSupportedException();
            }
        }

        private static Color[] OnePixelFilter(PixelFilterSettings settings, Color[] pixels, Func<Color, Color, Color> op)
        {
            var filtered = (Color[])pixels.Clone();
            for (var i = 0; i < pixels.Length; i++)
            {
                filtered[i] = op(settings.color, pixels[i]);
            }
            return filtered;
        }

        private static Color BlendAlpha(Color c, Color c1, Color c0)
        {
            return new Color
            (
                c.r * c1.a + c0.r * (1F - c1.a),
                c.g * c1.a + c0.g * (1F - c1.a),
                c.b * c1.a + c0.b * (1F - c1.a),
                c.a + c0.a * (1F - c1.a)
            );
        }

        private static Color Normal(Color c1, Color c0)
        {
            return BlendAlpha(
                new Color
                (
                    c1.r,
                    c1.g,
                    c1.b,
                    c1.a
                ),
                c1,
                c0
            );
        }

        private static Color Replace(Color c1, Color c0)
        {
            return c1;
        }

        private static Color Darken(Color c1, Color c0)
        {
            return BlendAlpha(
                new Color
                (
                    Mathf.Min(c1.r, c0.r),
                    Mathf.Min(c1.g, c0.g),
                    Mathf.Min(c1.b, c0.b),
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
                    c1.r * c0.r,
                    c1.g * c0.g,
                    c1.b * c0.b,
                    c1.a
                ),
                c1,
                c0
            );
        }

        private static Color ColorBurn(Color c1, Color c0)
        {
            return BlendAlpha(
                new Color
                (
                    Mathf.Clamp01(1F - (1F - c0.r) / c1.r),
                    Mathf.Clamp01(1F - (1F - c0.g) / c1.g),
                    Mathf.Clamp01(1F - (1F - c0.b) / c1.b),
                    c1.a
                ),
                c1,
                c0
            );
        }

        private static Color LinearBurn(Color c1, Color c0)
        {
            return BlendAlpha(
                new Color
                (
                    Mathf.Clamp01(c1.r + c0.r - 1F),
                    Mathf.Clamp01(c1.g + c0.g - 1F),
                    Mathf.Clamp01(c1.b + c0.b - 1F),
                    c1.a
                ),
                c1,
                c0
            );
        }

        private static Color Lighten(Color c1, Color c0)
        {
            return BlendAlpha(
                new Color
                (
                    Mathf.Max(c1.r, c0.r),
                    Mathf.Max(c1.g, c0.g),
                    Mathf.Max(c1.b, c0.b),
                    c1.a
                ),
                c1,
                c0
            );
        }

        private static Color Screen(Color c1, Color c0)
        {
            return BlendAlpha(
                new Color
                (
                    1F - (1F - c1.r) * (1F - c0.r),
                    1F - (1F - c1.g) * (1F - c0.g),
                    1F - (1F - c1.b) * (1F - c0.b),
                    c1.a
                ),
                c1,
                c0
            );
        }

        private static Color ColorDodge(Color c1, Color c0)
        {
            return BlendAlpha(
                new Color
                (
                    Mathf.Clamp01(c0.r / (1F - c1.r)),
                    Mathf.Clamp01(c0.g / (1F - c1.g)),
                    Mathf.Clamp01(c0.b / (1F - c1.b)),
                    c1.a
                ),
                c1,
                c0
            );
        }

        private static Color LinearDodge(Color c1, Color c0)
        {
            return BlendAlpha(
                new Color
                (
                    Mathf.Clamp01(c1.r + c0.r),
                    Mathf.Clamp01(c1.g + c0.g),
                    Mathf.Clamp01(c1.b + c0.b),
                    c1.a
                ),
                c1,
                c0
            );
        }

        private static Color Overlay(Color c1, Color c0)
        {
            return BlendAlpha(
                new Color
                (
                    c0.r < 0.5F ? 2F * c1.r * c0.r : 1F - 2F * (1F - c1.r) * (1F - c0.r),
                    c0.g < 0.5F ? 2F * c1.g * c0.g : 1F - 2F * (1F - c1.g) * (1F - c0.g),
                    c0.b < 0.5F ? 2F * c1.b * c0.b : 1F - 2F * (1F - c1.b) * (1F - c0.b),
                    c1.a
                ),
                c1,
                c0
            );
        }

        private static Color SoftLight(Color c1, Color c0)
        {
            return BlendAlpha(
                new Color
                (
                    c1.r < 0.5F ? 2F * (c1.r - 1F) * (c0.r - Mathf.Pow(c0.r, 2F)) + c0.r : 2F * (c1.r - 1F) * (Mathf.Sqrt(c0.r) - c0.r) + c0.r,
                    c1.g < 0.5F ? 2F * (c1.g - 1F) * (c0.g - Mathf.Pow(c0.g, 2F)) + c0.g : 2F * (c1.g - 1F) * (Mathf.Sqrt(c0.g) - c0.g) + c0.g,
                    c1.b < 0.5F ? 2F * (c1.b - 1F) * (c0.b - Mathf.Pow(c0.b, 2F)) + c0.b : 2F * (c1.b - 1F) * (Mathf.Sqrt(c0.b) - c0.b) + c0.b,
                    c1.a
                ),
                c1,
                c0
            );
        }

        private static Color HardLight(Color c1, Color c0)
        {
            return BlendAlpha(
                new Color
                (
                    c1.r < 0.5F ? 2F * c1.r * c0.r : 1F - 2F * (1F - c1.r) * (1F - c0.r),
                    c1.g < 0.5F ? 2F * c1.g * c0.g : 1F - 2F * (1F - c1.g) * (1F - c0.g),
                    c1.b < 0.5F ? 2F * c1.b * c0.b : 1F - 2F * (1F - c1.b) * (1F - c0.b),
                    c1.a
                ),
                c1,
                c0
            );
        }

        private static Color VividLight(Color c1, Color c0)
        {
            return BlendAlpha(
                new Color
                (
                    Mathf.Clamp01(c1.r < 0.5F ? 1F - (1F - c0.r) / (2F * c1.r) : c0.r / (2F * (1F - c1.r))),
                    Mathf.Clamp01(c1.g < 0.5F ? 1F - (1F - c0.g) / (2F * c1.g) : c0.g / (2F * (1F - c1.g))),
                    Mathf.Clamp01(c1.b < 0.5F ? 1F - (1F - c0.b) / (2F * c1.b) : c0.b / (2F * (1F - c1.b))),
                    c1.a
                ),
                c1,
                c0
            );
        }

        private static Color LinearLight(Color c1, Color c0)
        {
            return BlendAlpha(
                new Color
                (
                    Mathf.Clamp01(c0.r + 2F * c1.r - 1F),
                    Mathf.Clamp01(c0.g + 2F * c1.g - 1F),
                    Mathf.Clamp01(c0.b + 2F * c1.b - 1F),
                    c1.a
                ),
                c1,
                c0
            );
        }

        private static Color PinLight(Color c1, Color c0)
        {
            return BlendAlpha(
                new Color
                (
                    Mathf.Clamp01(c0.r < 2F * c1.r - 1F ? 2F * c1.r - 1F : c0.r > 2F * c1.r ? 2F * c1.r : c0.r),
                    Mathf.Clamp01(c0.g < 2F * c1.g - 1F ? 2F * c1.g - 1F : c0.g > 2F * c1.g ? 2F * c1.g : c0.g),
                    Mathf.Clamp01(c0.b < 2F * c1.b - 1F ? 2F * c1.b - 1F : c0.b > 2F * c1.b ? 2F * c1.b : c0.b),
                    c1.a
                ),
                c1,
                c0
            );
        }

        private static Color HardMix(Color c1, Color c0)
        {
            return BlendAlpha(
                new Color
                (
                    c1.r < c0.r - 1F ? 0F : 1F,
                    c1.g < c0.g - 1F ? 0F : 1F,
                    c1.b < c0.b - 1F ? 0F : 1F,
                    c1.a
                ),
                c1,
                c0
            );
        }

        private static Color Difference(Color c1, Color c0)
        {
            return BlendAlpha(
                new Color
                (
                    Mathf.Abs(c1.r - c0.r),
                    Mathf.Abs(c1.g - c0.g),
                    Mathf.Abs(c1.b - c0.b),
                    c1.a
                ),
                c1,
                c0
            );
        }

        private static Color Exclusion(Color c1, Color c0)
        {
            return BlendAlpha(
                new Color
                (
                    c1.r + c0.r - 2F * c1.r * c0.r,
                    c1.g + c0.g - 2F * c1.g * c0.g,
                    c1.b + c0.b - 2F * c1.b * c0.b,
                    c1.a
                ),
                c1,
                c0
            );
        }

        private static Color Subtract(Color c1, Color c0)
        {
            return BlendAlpha(
                new Color
                (
                    Mathf.Clamp01(c0.r - c1.r),
                    Mathf.Clamp01(c0.g - c1.g),
                    Mathf.Clamp01(c0.b - c1.b),
                    c1.a
                ),
                c1,
                c0
            );
        }

        private static Color Divide(Color c1, Color c0)
        {
            return BlendAlpha(
                new Color
                (
                    Mathf.Clamp01(c0.r / c1.r),
                    Mathf.Clamp01(c0.g / c1.g),
                    Mathf.Clamp01(c0.b / c1.b),
                    c1.a
                ),
                c1,
                c0
            );
        }

        private static Color Hue(Color c1, Color c0)
        {
            Color.RGBToHSV(c1, out var h1, out var s1, out var v1);
            Color.RGBToHSV(c0, out var h0, out var s0, out var v0);
            var c2 = Color.HSVToRGB(h1, s0, v0);
            c2.a = c1.a;
            return BlendAlpha(
                c2,
                c1,
                c0
            );
        }

        private static Color Saturation(Color c1, Color c0)
        {
            Color.RGBToHSV(c1, out var h1, out var s1, out var v1);
            Color.RGBToHSV(c0, out var h0, out var s0, out var v0);
            var c2 = Color.HSVToRGB(h0, s1, v0);
            c2.a = c1.a;
            return BlendAlpha(
                c2,
                c1,
                c0
            );
        }

        private static Color HSVColor(Color c1, Color c0)
        {
            Color.RGBToHSV(c1, out var h1, out var s1, out var v1);
            Color.RGBToHSV(c0, out var h0, out var s0, out var v0);
            var c2 = Color.HSVToRGB(h1, s1, v0);
            c2.a = c1.a;
            return BlendAlpha(
                c2,
                c1,
                c0
            );
        }

        private static Color Luminosity(Color c1, Color c0)
        {
            Color.RGBToHSV(c1, out var h1, out var s1, out var v1);
            Color.RGBToHSV(c0, out var h0, out var s0, out var v0);
            var c2 = Color.HSVToRGB(h0, s0, v1);
            c2.a = c1.a;
            return BlendAlpha(
                c2,
                c1,
                c0
            );
        }

        private static Color DarkerColor(Color c1, Color c0)
        {
            Color.RGBToHSV(c1, out var h1, out var s1, out var v1);
            Color.RGBToHSV(c0, out var h0, out var s0, out var v0);
            return BlendAlpha(
                v1 < v0 ? c1 : c0,
                c1,
                c0
            );
        }

        private static Color LighterColor(Color c1, Color c0)
        {
            Color.RGBToHSV(c1, out var h1, out var s1, out var v1);
            Color.RGBToHSV(c0, out var h0, out var s0, out var v0);
            return BlendAlpha(
                v1 > v0 ? c1 : c0,
                c1,
                c0
            );
        }
    }
}
