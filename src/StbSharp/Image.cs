using System.Buffers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
using DotNext.Buffers;
using StbSharp.Interop;

namespace StbSharp;

public enum PixelFormat
{
    Default = 0,
    Grey = 1,
    GreyAlpha = 2,
    Rgb = 3,
    Rgba = 4
}

public sealed class Image : IDisposable
{
    private unsafe byte* _pixels;
    private readonly bool _isStbPointer;
    public int Width { get; }
    public int Height { get; }
    public PixelFormat Format { get; }

    public Span<byte> Pixels
    {
        get
        {
            unsafe
            {
                return new Span<byte>(_pixels, Width * Height * (int)Format);
            }
        }
    }

    private ref struct Utf8String : IDisposable
    {
        private SpanOwner<byte> _spanOwner;
        private readonly int _length;

        public Utf8String(string utf16String)
        {
            _spanOwner = new SpanOwner<byte>(Encoding.UTF8.GetMaxByteCount(utf16String.Length));
            var operationStatus = Utf8.FromUtf16(utf16String, _spanOwner.Span, out _, out _length);
            if (operationStatus != OperationStatus.Done)
                throw new ArgumentException("Failed to convert UTF16 to UTF8");
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref byte GetPinnableReference() => ref _spanOwner.Span[.._length].GetPinnableReference();

        public void Dispose() => _spanOwner.Dispose();
    }

    public Image(string fileName)
    {
        int x, y, c;
        unsafe
        {
            using var fileNameUtf8 = new Utf8String(fileName);
            fixed (byte* pUtf8 = fileNameUtf8)
                _pixels = StbImage.stbi_load(pUtf8, &x, &y, &c, 0);
            if (_pixels is null) throw new ArgumentException("Invalid file");
        }

        Width = x;
        Height = y;
        Format = (PixelFormat)c;
        _isStbPointer = true;
    }

    public Image(int width, int height, PixelFormat format)
    {
        unsafe
        {
            _pixels = (byte*)NativeMemory.Alloc((nuint)(width * height), (nuint)format);
            Width = width;
            Height = height;
            Format = format;
            _isStbPointer = false;
        }
    }

    public void Dispose()
    {
        unsafe
        {
            if (_pixels is null) return;
            if (_isStbPointer) StbImage.stbi_image_free(_pixels);
            else NativeMemory.Free(_pixels);

            _pixels = null;
        }
    }

    public void SaveToPng(string fileName)
    {
        using var fileNameUtf8 = new Utf8String(fileName);
        unsafe
        {
            var result = 0;
            fixed (byte* pUtf8 = fileNameUtf8)
                result = StbImageWrite.stbi_write_png(pUtf8, Width, Height, (int)Format, _pixels, 4);
            if (result == 0)
            {
                throw new InvalidOperationException("Failed to save");
            }
        }
    }
}
