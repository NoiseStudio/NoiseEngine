using NoiseEngine.Rendering;
using System;

namespace NoiseEngine.Tests.Rendering;

public class ColorConversionsTest {

    [Fact]
    public void ColorToColor32ToColor() {
        Random random = new Random(0);

        for (int i = 0; i < 16; i++) {
            Color color = new Color(
                random.NextSingle(),
                random.NextSingle(),
                random.NextSingle(),
                random.NextSingle()
            );
            Color converted = (Color32)color;

            Assert.True(Math.Abs(color.R - converted.R) <= 0.01f);
            Assert.True(Math.Abs(color.G - converted.G) <= 0.01f);
            Assert.True(Math.Abs(color.B - converted.B) <= 0.01f);
            Assert.True(Math.Abs(color.A - converted.A) <= 0.01f);
        }
    }

}
