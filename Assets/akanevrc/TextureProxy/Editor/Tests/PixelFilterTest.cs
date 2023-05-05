using NUnit.Framework;
using UnityEngine;

namespace akanevrc.TextureProxy.Tests
{
    public class PixelFilterTest
    {
        [Test]
        public void Normal()
        {
            var filtered = FilterOnePixel(PixelFilterMode.Normal, Gray(0.5F, 0.5F), Color.white);
            Assert.That(filtered[0], Is.EqualTo(Gray(0.75F)));
        }

        [Test]
        public void Multiply()
        {
            var filtered = FilterOnePixel(PixelFilterMode.Multiply, Gray(0.5F), Gray(0.5F));
            Assert.That(filtered[0], Is.EqualTo(Gray(0.25F)));
        }

        private Color[] FilterOnePixel(PixelFilterMode mode, Color filter, Color pixel)
        {
            var settings = new PixelFilterSettings() { mode = mode, color = filter };
            var pixels = new Color[] { pixel };
            return PixelFilter.Filter(settings, pixels);
        }

        private Color Gray(float value, float alpha)
        {
            return new Color(value, value, value, alpha);
        }

        private Color Gray(float value)
        {
            return new Color(value, value, value);
        }
    }
}
