namespace StbSharp.Tests;

public class StbSharpTests
{
    [Fact]
    public unsafe void ToCString_IsNullTerminated()
    {
        const string Value = "roundtrip.png";

        using var utf8 = Value.ToCString();

        fixed (byte* ptr = utf8.Span)
        {
            Assert.Equal(Value, System.Runtime.InteropServices.Marshal.PtrToStringUTF8((nint)ptr));
            Assert.Equal((byte)0, utf8.Span[System.Text.Encoding.UTF8.GetByteCount(Value)]);
        }
    }

    [Fact]
    public unsafe void CanWriteAndReadPngUsingSameNativeLibrary_String()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, $"{Guid.NewGuid():N}.png");
        {
            using var wImage = new Image(1, 1, PixelComponents.Rgba);
            wImage.Pixels[0] = 0x11;
            wImage.Pixels[1] = 0x22;
            wImage.Pixels[2] = 0x33;
            wImage.Pixels[3] = 0x44;
            wImage.SaveAsPng(filePath);
        }

        try
        {
            using var rImage = new Image(filePath);
            Assert.Equal(1, rImage.Width);
            Assert.Equal(1, rImage.Height);
            Assert.Equal(PixelComponents.Rgba, rImage.Components);
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

    [Fact]
    public unsafe void CanWriteAndReadPngUsingSameNativeLibrary_Stream()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, $"{Guid.NewGuid():N}.png");
        {
            using var wImage = new Image(1, 1, PixelComponents.Rgba);
            wImage.Pixels[0] = 0x11;
            wImage.Pixels[1] = 0x22;
            wImage.Pixels[2] = 0x33;
            wImage.Pixels[3] = 0x44;
            wImage.SaveAsPng(filePath);
        }

        try
        {
            using var stream = File.OpenRead(filePath);
            using var rImage = new Image(stream);
            Assert.Equal(1, rImage.Width);
            Assert.Equal(1, rImage.Height);
            Assert.Equal(PixelComponents.Rgba, rImage.Components);
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

    [Fact]
    public unsafe void CanWriteAndReadPngUsingSameNativeLibrary_Bytes()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, $"{Guid.NewGuid():N}.png");
        {
            using var wImage = new Image(1, 1, PixelComponents.Rgba);
            wImage.Pixels[0] = 0x11;
            wImage.Pixels[1] = 0x22;
            wImage.Pixels[2] = 0x33;
            wImage.Pixels[3] = 0x44;
            wImage.SaveAsPng(filePath);
        }

        try
        {
            var bytes = File.ReadAllBytes(filePath);
            using var rImage = new Image(bytes);
            Assert.Equal(1, rImage.Width);
            Assert.Equal(1, rImage.Height);
            Assert.Equal(PixelComponents.Rgba, rImage.Components);
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
