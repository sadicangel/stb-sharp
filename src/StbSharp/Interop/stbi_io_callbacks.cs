using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace StbSharp.Interop;

internal unsafe partial struct stbi_io_callbacks
{
    public static stbi_io_callbacks Create() =>
        new stbi_io_callbacks
        {
            read = &Read,
            skip = &Skip,
            eof = &Eof,
        };

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int Read(void* context, byte* buffer, int count)
    {
        var handle = GCHandle.FromIntPtr((nint)context);
        var stream = (Stream?)handle.Target ?? throw new ArgumentNullException(nameof(context));
        return stream.Read(new Span<byte>(buffer, count));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void Skip(void* context, int count)
    {
        var handle = GCHandle.FromIntPtr((nint)context);
        var stream = (Stream?)handle.Target ?? throw new ArgumentNullException(nameof(context));
        stream.Position += count;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int Eof(void* context)
    {
        var handle = GCHandle.FromIntPtr((nint)context);
        var stream = (Stream?)handle.Target ?? throw new ArgumentNullException(nameof(context));
        return unchecked((int)(stream.Length - stream.Position));
    }
}
