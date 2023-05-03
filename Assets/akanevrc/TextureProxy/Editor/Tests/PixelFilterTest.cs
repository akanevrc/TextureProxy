using NUnit.Framework;
using UnityEngine;

namespace akanevrc.TextureProxy.Tests
{
    public class PixelFilterTest
    {
        [Test]
        public void Normal()
        {
            var filtered = FilterOnePixel(PixelFilterMode.Normal, 0.5F, 0.5F, 1F, 1F);
            Assert.That(filtered[0], Is.EqualTo(Gray(0.75F, 1F)));
        }

        [Test]
        public void Multiply()
        {
            var filtered = FilterOnePixel(PixelFilterMode.Multiply, 0.5F, 1F, 0.5F, 1F);
            Assert.That(filtered[0], Is.EqualTo(Gray(0.25F, 1F)));
        }

        private Color[] FilterOnePixel(PixelFilterMode mode, float filter, float filterAlpha, float pixel, float pixelAlpha)
        {
            var settings = new PixelFilterSettings()
            {
                mode = mode,
                r = filter,
                g = filter,
                b = filter,
                a = filterAlpha,
            };
            var pixels = new Color[] { Gray(pixel, pixelAlpha) };
            return PixelFilter.Filter(settings, pixels);
        }

        private Color Gray(float value, float alpha)
        {
            return new Color(value, value, value, alpha);
        }
    }
}
