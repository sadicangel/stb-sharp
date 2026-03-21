using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DotNext.Buffers;
using DotNext.Runtime.InteropServices;
using StbSharp.Interop;

namespace StbSharp;

public sealed class Image : IDisposable
{
    private const float HdrGamma = 2.2f;
    private const float ByteScale = 1f / byte.MaxValue;

    private readonly IUnmanagedMemory<byte> _memory;

    public int Width { get; }
    public int Height { get; }
    public PixelComponents Components { get; }
    public Span<byte> Pixels => _memory.Span;

    public Image(string fileName)
    {
        int x, y, c;
        unsafe
        {
            using var fileNameCString = fileName.ToCString();
            fixed (byte* pUtf8 = fileNameCString)
            {
                var pixels = StbImage.stbi_load(pUtf8, &x, &y, &c, 0);
                if (pixels is null) throw new ArgumentException("Invalid file");
                _memory = new StbImageUnmanagedMemory(pixels, x * y * c);
            }
        }

        Width = x;
        Height = y;
        Components = (PixelComponents)c;
    }

    public Image(Stream stream)
    {
        int x, y, c;
        unsafe
        {
            var handle = GCHandle.Alloc(stream);
            try
            {
                var callbacks = stbi_io_callbacks.Create();
                var pixels = StbImage.stbi_load_from_callbacks(&callbacks, (void*)GCHandle.ToIntPtr(handle), &x, &y, &c, 0);
                if (pixels is null) throw new ArgumentException("Invalid file");
                _memory = new StbImageUnmanagedMemory(pixels, x * y * c);
            }
            finally
            {
                handle.Free();
            }
        }

        Width = x;
        Height = y;
        Components = (PixelComponents)c;
    }

    public Image(ReadOnlySpan<byte> buffer)
    {
        int x, y, c;
        unsafe
        {
            fixed (byte* ptr = buffer)
            {
                var pixels = StbImage.stbi_load_from_memory(ptr, buffer.Length, &x, &y, &c, 0);
                if (pixels is null) throw new ArgumentException("Invalid file");
                _memory = new StbImageUnmanagedMemory(pixels, x * y * c);
            }
        }

        Width = x;
        Height = y;
        Components = (PixelComponents)c;
    }

    public Image(int width, int height, PixelComponents components)
    {
        _memory = UnmanagedMemory.Allocate<byte>(width * height * components.Count);
        Width = width;
        Height = height;
        Components = components;
    }

    public void Dispose() => _memory.Dispose();

    public void SaveAsPng(string fileName, int stride = 0) => SaveAs(fileName, _memory, ImageFormat.Png, Width, Height, Components, stride);
    public void SaveAsBmp(string fileName) => SaveAs(fileName, _memory, ImageFormat.Bmp, Width, Height, Components);
    public void SaveAsTga(string fileName) => SaveAs(fileName, _memory, ImageFormat.Tga, Width, Height, Components);
    public void SaveAsHdr(string fileName) => SaveAs(fileName, _memory, ImageFormat.Hdr, Width, Height, Components);
    public void SaveAsJpg(string fileName, int quality = 50) => SaveAs(fileName, _memory, ImageFormat.Jpg, Width, Height, Components, quality: quality);

    private static unsafe void SaveAs(
        string fileName,
        IUnmanagedMemory memory,
        ImageFormat format,
        int width,
        int height,
        PixelComponents components,
        int stride = 0,
        int quality = 50)
    {
        using var fileNameCString = fileName.ToCString();
        fixed (byte* pUtf8 = fileNameCString.Span)
        {
            int result;
            var pixels = (void*)memory.Pointer.Address;

            switch (format)
            {
                case ImageFormat.Png:
                    result = StbImageWrite.stbi_write_png(pUtf8, width, height, components.Count, pixels, GetPngStride(width, components, stride));
                    break;
                case ImageFormat.Bmp:
                    result = StbImageWrite.stbi_write_bmp(pUtf8, width, height, components.Count, pixels);
                    break;
                case ImageFormat.Tga:
                    result = StbImageWrite.stbi_write_tga(pUtf8, width, height, components.Count, pixels);
                    break;
                case ImageFormat.Hdr:
                    {
                        var hdrPixels = ConvertToHdrPixels(pixels, width, height, components);
                        fixed (float* pHdrPixels = hdrPixels)
                            result = StbImageWrite.stbi_write_hdr(pUtf8, width, height, components.Count, pHdrPixels);

                        break;
                    }
                case ImageFormat.Jpg:
                    result = StbImageWrite.stbi_write_jpg(pUtf8, width, height, components.Count, pixels, quality);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }

            if (result == 0)
            {
                throw new InvalidOperationException("Failed to save");
            }
        }
    }

    public void SaveAsPng(Stream stream, int stride = 0) => SaveAs(stream, _memory, ImageFormat.Png, Width, Height, Components, stride);
    public void SaveAsBmp(Stream stream) => SaveAs(stream, _memory, ImageFormat.Bmp, Width, Height, Components);
    public void SaveAsTga(Stream stream) => SaveAs(stream, _memory, ImageFormat.Tga, Width, Height, Components);
    public void SaveAsHdr(Stream stream) => SaveAs(stream, _memory, ImageFormat.Hdr, Width, Height, Components);
    public void SaveAsJpg(Stream stream, int quality = 50) => SaveAs(stream, _memory, ImageFormat.Jpg, Width, Height, Components, quality: quality);

    private static unsafe void SaveAs(
        Stream stream,
        IUnmanagedMemory memory,
        ImageFormat format,
        int width,
        int height,
        PixelComponents components,
        int stride = 0,
        int quality = 50)
    {
        var handle = GCHandle.Alloc(stream);
        try
        {
            int result;
            var pixels = (void*)memory.Pointer.Address;

            switch (format)
            {
                case ImageFormat.Png:
                    result = StbImageWrite.stbi_write_png_to_func(&Write, (void*)GCHandle.ToIntPtr(handle), width, height, components.Count, pixels, GetPngStride(width, components, stride));
                    break;
                case ImageFormat.Bmp:
                    result = StbImageWrite.stbi_write_bmp_to_func(&Write, (void*)GCHandle.ToIntPtr(handle), width, height, components.Count, pixels);
                    break;
                case ImageFormat.Tga:
                    result = StbImageWrite.stbi_write_tga_to_func(&Write, (void*)GCHandle.ToIntPtr(handle), width, height, components.Count, pixels);
                    break;
                case ImageFormat.Hdr:
                    {
                        var hdrPixels = ConvertToHdrPixels(pixels, width, height, components);
                        fixed (float* pHdrPixels = hdrPixels)
                            result = StbImageWrite.stbi_write_hdr_to_func(&Write, (void*)GCHandle.ToIntPtr(handle), width, height, components.Count, pHdrPixels);

                        break;
                    }
                case ImageFormat.Jpg:
                    result = StbImageWrite.stbi_write_jpg_to_func(&Write, (void*)GCHandle.ToIntPtr(handle), width, height, components.Count, pixels, quality);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }

            if (result == 0)
            {
                throw new InvalidOperationException("Failed to save");
            }
        }
        finally
        {
            handle.Free();
        }


        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        static void Write(void* context, void* buffer, int count)
        {
            var handle = GCHandle.FromIntPtr((nint)context);
            var stream = (Stream?)handle.Target ?? throw new ArgumentNullException(nameof(context));
            stream.Write(new ReadOnlySpan<byte>(buffer, count));
        }
    }

    private static int GetPngStride(int width, PixelComponents components, int stride) =>
        stride == 0 ? checked(width * components.Count) : stride;

    private static unsafe float[] ConvertToHdrPixels(void* pixels, int width, int height, PixelComponents components)
    {
        var componentCount = components.Count;
        var pixelCount = checked(width * height * componentCount);
        var hdrPixels = GC.AllocateUninitializedArray<float>(pixelCount);
        var ldrPixels = new ReadOnlySpan<byte>(pixels, pixelCount);
        var nonAlphaComponentCount = (componentCount & 1) == 1 ? componentCount : componentCount - 1;

        for (var pixelIndex = 0; pixelIndex < pixelCount; pixelIndex += componentCount)
        {
            for (var componentIndex = 0; componentIndex < nonAlphaComponentCount; componentIndex++)
            {
                hdrPixels[pixelIndex + componentIndex] =
                    MathF.Pow(ldrPixels[pixelIndex + componentIndex] * ByteScale, HdrGamma);
            }

            if (nonAlphaComponentCount < componentCount)
            {
                hdrPixels[pixelIndex + nonAlphaComponentCount] =
                    ldrPixels[pixelIndex + nonAlphaComponentCount] * ByteScale;
            }
        }

        return hdrPixels;
    }
}
