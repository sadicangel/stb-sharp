namespace StbSharp.Tests;

public class ImageTests
{
    private const int JpegTolerance = 12;
    private static readonly byte[] EmbeddedPngBytes =
        Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAIAAAACCAYAAABytg0kAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAbSURBVBhXYxBUMnYJTSvvYJi5aveZu+8+/AcAOHcI6RQtKe0AAAAASUVORK5CYII=");

    private static readonly byte[] EmbeddedPngPixels =
    [
        0x11, 0x22, 0x33, 0x44,
        0x55, 0x66, 0x77, 0x88,
        0x99, 0xAA, 0xBB, 0xCC,
        0xDD, 0xEE, 0xF0, 0xFF,
    ];

    [Fact]
    public void ImageFromFile_LoadsEmbeddedPngFixture()
    {
        var filePath = CreateTempFilePath(".png");
        try
        {
            File.WriteAllBytes(filePath, EmbeddedPngBytes);

            using var image = new Image(filePath);

            AssertImage(image, 2, 2, PixelComponents.Rgba, EmbeddedPngPixels);
        }
        finally
        {
            DeleteIfExists(filePath);
        }
    }

    [Fact]
    public void ImageFromStream_LoadsEmbeddedPngFixture()
    {
        using var stream = new MemoryStream(EmbeddedPngBytes, writable: false);
        using var image = new Image(stream);

        AssertImage(image, 2, 2, PixelComponents.Rgba, EmbeddedPngPixels);
    }

    [Fact]
    public void ImageFromBytes_LoadsEmbeddedPngFixture()
    {
        using var image = new Image(EmbeddedPngBytes);

        AssertImage(image, 2, 2, PixelComponents.Rgba, EmbeddedPngPixels);
    }

    [Fact]
    public void ImageFromDimensions_ExposesWritablePixelsAndMetadata()
    {
        var expectedPixels = CreateSequentialPixels(3, 2, PixelComponents.Rgba);

        using var image = new Image(3, 2, PixelComponents.Rgba);

        Assert.Equal(3, image.Width);
        Assert.Equal(2, image.Height);
        Assert.Equal(PixelComponents.Rgba, image.Components);
        Assert.Equal(expectedPixels.Length, image.Pixels.Length);

        expectedPixels.CopyTo(image.Pixels);

        Assert.Equal(expectedPixels, image.Pixels.ToArray());
    }

    [Fact]
    public void ImageFromFile_WithInvalidContents_ThrowsArgumentException()
    {
        var filePath = CreateTempFilePath(".png");
        try
        {
            File.WriteAllBytes(filePath, [0x01, 0x02, 0x03, 0x04]);

            Assert.Throws<ArgumentException>(() => new Image(filePath));
        }
        finally
        {
            DeleteIfExists(filePath);
        }
    }

    [Fact]
    public void ImageFromStream_WithInvalidContents_ThrowsArgumentException()
    {
        using var stream = new MemoryStream([0x01, 0x02, 0x03, 0x04], writable: false);

        Assert.Throws<ArgumentException>(() => new Image(stream));
    }

    [Fact]
    public void ImageFromBytes_WithInvalidContents_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Image([0x01, 0x02, 0x03, 0x04]));
    }

    [Fact]
    public void Dispose_CanBeCalledTwice_ForManagedImage()
    {
        var image = new Image(1, 1, PixelComponents.Rgba);

        var exception = Record.Exception(() =>
        {
            image.Dispose();
            image.Dispose();
        });

        Assert.Null(exception);
    }

    [Fact]
    public void Dispose_CanBeCalledTwice_ForStbOwnedImage()
    {
        var image = new Image(EmbeddedPngBytes);

        var exception = Record.Exception(() =>
        {
            image.Dispose();
            image.Dispose();
        });

        Assert.Null(exception);
    }

    [Fact]
    public void SaveAsPng_String_DefaultStride_RoundTripsExactPixels()
    {
        var expectedPixels = CreateSequentialPixels(3, 2, PixelComponents.Rgba);

        AssertRoundTripViaFile(
            3,
            2,
            PixelComponents.Rgba,
            expectedPixels,
            ".png",
            (image, path) => image.SaveAsPng(path));
    }

    [Fact]
    public void SaveAsPng_Stream_DefaultStride_RoundTripsExactPixels()
    {
        var expectedPixels = CreateSequentialPixels(3, 2, PixelComponents.Rgba);

        AssertRoundTripViaStream(
            3,
            2,
            PixelComponents.Rgba,
            expectedPixels,
            (image, stream) => image.SaveAsPng(stream));
    }

    [Fact]
    public void SaveAsPng_String_ExplicitStride_RoundTripsExactPixels()
    {
        var expectedPixels = CreateSequentialPixels(3, 2, PixelComponents.Rgba);

        AssertRoundTripViaFile(
            3,
            2,
            PixelComponents.Rgba,
            expectedPixels,
            ".png",
            (image, path) => image.SaveAsPng(path, 3 * PixelComponents.Rgba.Count));
    }

    [Fact]
    public void SaveAsBmp_String_RoundTripsExactPixels()
    {
        var expectedPixels = CreateSequentialPixels(3, 2, PixelComponents.Rgb);

        AssertRoundTripViaFile(
            3,
            2,
            PixelComponents.Rgb,
            expectedPixels,
            ".bmp",
            (image, path) => image.SaveAsBmp(path));
    }

    [Fact]
    public void SaveAsBmp_Stream_RoundTripsExactPixels()
    {
        var expectedPixels = CreateSequentialPixels(3, 2, PixelComponents.Rgb);

        AssertRoundTripViaStream(
            3,
            2,
            PixelComponents.Rgb,
            expectedPixels,
            (image, stream) => image.SaveAsBmp(stream));
    }

    [Fact]
    public void SaveAsTga_String_RoundTripsExactPixels()
    {
        var expectedPixels = CreateSequentialPixels(2, 2, PixelComponents.Rgba);

        AssertRoundTripViaFile(
            2,
            2,
            PixelComponents.Rgba,
            expectedPixels,
            ".tga",
            (image, path) => image.SaveAsTga(path));
    }

    [Fact]
    public void SaveAsTga_Stream_RoundTripsExactPixels()
    {
        var expectedPixels = CreateSequentialPixels(2, 2, PixelComponents.Rgba);

        AssertRoundTripViaStream(
            2,
            2,
            PixelComponents.Rgba,
            expectedPixels,
            (image, stream) => image.SaveAsTga(stream));
    }

    [Fact]
    public void SaveAsJpg_String_DefaultQuality_RoundTripsWithinTolerance()
    {
        var expectedPixels = CreateSolidPixels(16, 16, PixelComponents.Rgb, 0x30, 0x80, 0xD0);

        AssertRoundTripViaFileWithinTolerance(
            16,
            16,
            PixelComponents.Rgb,
            expectedPixels,
            ".jpg",
            JpegTolerance,
            (image, path) => image.SaveAsJpg(path));
    }

    [Fact]
    public void SaveAsJpg_Stream_ExplicitQuality_RoundTripsWithinTolerance()
    {
        var expectedPixels = CreateSolidPixels(16, 16, PixelComponents.Rgb, 0xA0, 0x50, 0x20);

        AssertRoundTripViaStreamWithinTolerance(
            16,
            16,
            PixelComponents.Rgb,
            expectedPixels,
            JpegTolerance,
            (image, stream) => image.SaveAsJpg(stream, quality: 90));
    }

    [Fact]
    public void SaveAsHdr_String_RoundTripsExtremeValues()
    {
        var expectedPixels = CreateExtremeHdrPixels();

        AssertRoundTripViaFile(
            2,
            2,
            PixelComponents.Rgb,
            expectedPixels,
            ".hdr",
            (image, path) => image.SaveAsHdr(path));
    }

    [Fact]
    public void SaveAsHdr_Stream_RoundTripsExtremeValues()
    {
        var expectedPixels = CreateExtremeHdrPixels();

        AssertRoundTripViaStream(
            2,
            2,
            PixelComponents.Rgb,
            expectedPixels,
            (image, stream) => image.SaveAsHdr(stream));
    }

    private static void AssertRoundTripViaFile(
        int width,
        int height,
        PixelComponents components,
        byte[] expectedPixels,
        string extension,
        Action<Image, string> save)
    {
        var filePath = CreateTempFilePath(extension);

        try
        {
            using var image = CreateImage(width, height, components, expectedPixels);
            save(image, filePath);

            using var roundTripped = new Image(filePath);
            AssertImage(roundTripped, width, height, components, expectedPixels);
        }
        finally
        {
            DeleteIfExists(filePath);
        }
    }

    private static void AssertRoundTripViaStream(
        int width,
        int height,
        PixelComponents components,
        byte[] expectedPixels,
        Action<Image, Stream> save)
    {
        using var image = CreateImage(width, height, components, expectedPixels);
        using var stream = new MemoryStream();

        save(image, stream);

        using var roundTripped = new Image(stream.ToArray());
        AssertImage(roundTripped, width, height, components, expectedPixels);
    }

    private static void AssertRoundTripViaFileWithinTolerance(
        int width,
        int height,
        PixelComponents components,
        byte[] expectedPixels,
        string extension,
        int tolerance,
        Action<Image, string> save)
    {
        var filePath = CreateTempFilePath(extension);

        try
        {
            using var image = CreateImage(width, height, components, expectedPixels);
            save(image, filePath);

            using var roundTripped = new Image(filePath);
            AssertImageWithinTolerance(roundTripped, width, height, components, expectedPixels, tolerance);
        }
        finally
        {
            DeleteIfExists(filePath);
        }
    }

    private static void AssertRoundTripViaStreamWithinTolerance(
        int width,
        int height,
        PixelComponents components,
        byte[] expectedPixels,
        int tolerance,
        Action<Image, Stream> save)
    {
        using var image = CreateImage(width, height, components, expectedPixels);
        using var stream = new MemoryStream();

        save(image, stream);

        using var roundTripped = new Image(stream.ToArray());
        AssertImageWithinTolerance(roundTripped, width, height, components, expectedPixels, tolerance);
    }

    private static void AssertImage(
        Image image,
        int width,
        int height,
        PixelComponents components,
        byte[] expectedPixels)
    {
        Assert.Equal(width, image.Width);
        Assert.Equal(height, image.Height);
        Assert.Equal(components, image.Components);
        Assert.Equal(expectedPixels, image.Pixels.ToArray());
    }

    private static void AssertImageWithinTolerance(
        Image image,
        int width,
        int height,
        PixelComponents components,
        byte[] expectedPixels,
        int tolerance)
    {
        Assert.Equal(width, image.Width);
        Assert.Equal(height, image.Height);
        Assert.Equal(components, image.Components);

        var actualPixels = image.Pixels.ToArray();
        Assert.Equal(expectedPixels.Length, actualPixels.Length);

        for (var index = 0; index < expectedPixels.Length; index++)
        {
            var expected = expectedPixels[index];
            Assert.InRange((int)actualPixels[index], Math.Max(0, expected - tolerance), Math.Min(255, expected + tolerance));
        }
    }

    private static Image CreateImage(int width, int height, PixelComponents components, byte[] pixels)
    {
        var expectedLength = checked(width * height * components.Count);
        Assert.Equal(expectedLength, pixels.Length);

        var image = new Image(width, height, components);
        pixels.CopyTo(image.Pixels);
        return image;
    }

    private static byte[] CreateSequentialPixels(int width, int height, PixelComponents components)
    {
        var pixels = new byte[checked(width * height * components.Count)];

        for (var index = 0; index < pixels.Length; index++)
        {
            pixels[index] = (byte)(index + 1);
        }

        return pixels;
    }

    private static byte[] CreateSolidPixels(int width, int height, PixelComponents components, params byte[] pixel)
    {
        Assert.Equal(components.Count, pixel.Length);

        var pixels = new byte[checked(width * height * components.Count)];

        for (var index = 0; index < pixels.Length; index += components.Count)
        {
            pixel.CopyTo(pixels, index);
        }

        return pixels;
    }

    private static byte[] CreateExtremeHdrPixels() =>
    [
        0x00, 0x00, 0x00,
        0xFF, 0xFF, 0xFF,
        0xFF, 0x00, 0x00,
        0x00, 0xFF, 0x00,
    ];

    private static string CreateTempFilePath(string extension) =>
        Path.Combine(AppContext.BaseDirectory, $"{nameof(ImageTests)}-{Guid.NewGuid():N}{extension}");

    private static void DeleteIfExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
