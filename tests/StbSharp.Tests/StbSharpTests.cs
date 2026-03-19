namespace StbSharp.Tests;

public class StbSharpTests
{
    [Fact]
    public unsafe void CanWriteAndReadPngUsingSameNativeLibrary()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, $"{Guid.NewGuid():N}.png");
        {
            using var wImage = new Image(1, 1, PixelFormat.Rgba);
            wImage.Pixels[0] = 0x11;
            wImage.Pixels[1] = 0x22;
            wImage.Pixels[2] = 0x33;
            wImage.Pixels[3] = 0x44;
            wImage.SaveToPng(filePath);
        }

        try
        {
            using var rImage = new Image(filePath);
            Assert.Equal(1, rImage.Width);
            Assert.Equal(1, rImage.Height);
            Assert.Equal(PixelFormat.Rgba, rImage.Format);
            Assert.Equal(0x11, rImage.Pixels[0]);
            Assert.Equal(0x22, rImage.Pixels[1]);
            Assert.Equal(0x33, rImage.Pixels[2]);
            Assert.Equal(0x44, rImage.Pixels[3]);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}
