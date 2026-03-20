using System.Runtime.InteropServices;
using StbSharp.Interop;

namespace StbSharp;

public sealed class Image : IDisposable
{
    private unsafe void* _pixels;
    private readonly bool _isStbPointer;
    public int Width { get; }
    public int Height { get; }
    public PixelComponents Components { get; }

    public Span<byte> Pixels
    {
        get
        {
            unsafe
            {
                return new Span<byte>(_pixels, Width * Height * Components.Count);
            }
        }
    }

    public Image(string fileName)
    {
        int x, y, c;
        unsafe
        {
            using var fileNameCString = fileName.ToCString();
            fixed (byte* pUtf8 = fileNameCString.Span)
                _pixels = StbImage.stbi_load(pUtf8, &x, &y, &c, 0);
            if (_pixels is null) throw new ArgumentException("Invalid file");
        }

        Width = x;
        Height = y;
        Components = (PixelComponents)c;
        _isStbPointer = true;
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
                _pixels = StbImage.stbi_load_from_callbacks(&callbacks, (void*)GCHandle.ToIntPtr(handle), &x, &y, &c, 0);
                if (_pixels is null) throw new ArgumentException("Invalid file");
            }
            finally
            {
                handle.Free();
            }
        }

        Width = x;
        Height = y;
        Components = (PixelComponents)c;
        _isStbPointer = true;
    }

    public Image(ReadOnlySpan<byte> buffer)
    {
        int x, y, c;
        unsafe
        {
            fixed (byte* ptr = buffer)
                _pixels = StbImage.stbi_load_from_memory(ptr, buffer.Length, &x, &y, &c, 0);
            if (_pixels is null) throw new ArgumentException("Invalid file");
        }

        Width = x;
        Height = y;
        Components = (PixelComponents)c;
        _isStbPointer = true;
    }

    public Image(int width, int height, PixelComponents components)
    {
        unsafe
        {
            _pixels = (byte*)NativeMemory.Alloc((nuint)(width * height), (nuint)components);
            Width = width;
            Height = height;
            Components = components;
            _isStbPointer = false;
        }
    }

    public void Dispose()
    {
        unsafe
        {
            if (_isStbPointer) StbImage.stbi_image_free(_pixels);
            else NativeMemory.Free(_pixels);

            _pixels = null;
        }
    }

    public void SaveAsPng(string fileName, int stride = 4)
    {
        unsafe { SaveAs(fileName, _pixels, ImageFormat.Png, Width, Height, Components, stride); }
    }


    public void SaveAsBmp(string fileName)
    {
        unsafe { SaveAs(fileName, _pixels, ImageFormat.Bmp, Width, Height, Components); }
    }


    public void SaveAsTga(string fileName)
    {
        unsafe { SaveAs(fileName, _pixels, ImageFormat.Tga, Width, Height, Components); }
    }


    public void SaveAsHdr(string fileName)
    {
        unsafe { SaveAs(fileName, _pixels, ImageFormat.Hdr, Width, Height, Components); }
    }

    public void SaveAsJpg(string fileName, int quality = 50)
    {
        unsafe { SaveAs(fileName, _pixels, ImageFormat.Jpg, Width, Height, Components, quality: quality); }
    }

    private static unsafe void SaveAs(
        string fileName,
        void* pixels,
        ImageFormat format,
        int width,
        int height,
        PixelComponents components,
        int stride = 4,
        int quality = 50)
    {
        using var fileNameCString = fileName.ToCString();
        int result;
        fixed (byte* pUtf8 = fileNameCString.Span)
            result = format switch
            {
                ImageFormat.Png => StbImageWrite.stbi_write_png(pUtf8, width, height, components.Count, pixels, stride),
                ImageFormat.Bmp => StbImageWrite.stbi_write_bmp(pUtf8, width, height, components.Count, pixels),
                ImageFormat.Tga => StbImageWrite.stbi_write_tga(pUtf8, width, height, components.Count, pixels),
                ImageFormat.Hdr => StbImageWrite.stbi_write_hdr(pUtf8, width, height, components.Count, (float*)pixels),
                ImageFormat.Jpg => StbImageWrite.stbi_write_jpg(pUtf8, width, height, components.Count, pixels, quality),
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
            };
        if (result == 0)
        {
            throw new InvalidOperationException("Failed to save");
        }
    }
}
