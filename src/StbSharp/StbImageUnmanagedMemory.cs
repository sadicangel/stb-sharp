using DotNext.Buffers;
using DotNext.Runtime.InteropServices;
using StbSharp.Interop;

namespace StbSharp;

internal sealed unsafe class StbImageUnmanagedMemory(byte* pixels, int length) : IUnmanagedMemory<byte>
{
    private byte* _pixels = pixels;

    public void Dispose()
    {
        if (_pixels == null) return;
        StbImage.stbi_image_free(_pixels);
        _pixels = null;
        Length = 0;
    }

    public Memory<byte> Memory => UnmanagedMemory.FromPointer(_pixels, Length);
    public int Length { get; private set; } = length;

    public Pointer<byte> Pointer => new(_pixels);
    public Span<byte> Span => new(_pixels, Length);
}
