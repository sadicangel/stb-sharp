using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace StbSharp.Interop;

internal unsafe partial struct stbi_io_callbacks
{
    public static stbi_io_callbacks Create() => new()
    {
        read = &Read,
        skip = &Skip,
        eof = &Eof,
    };

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int Read(void* user, byte* buffer, int count)
    {
        var handle = GCHandle.FromIntPtr((nint)user);
        var stream = (Stream?)handle.Target ?? throw new ArgumentNullException(nameof(user));
        return stream.Read(new Span<byte>(buffer, count));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void Skip(void* user, int count)
    {
        var handle = GCHandle.FromIntPtr((nint)user);
        var stream = (Stream?)handle.Target ?? throw new ArgumentNullException(nameof(user));
        stream.Position += count;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int Eof(void* user)
    {
        var handle = GCHandle.FromIntPtr((nint)user);
        var stream = (Stream?)handle.Target ?? throw new ArgumentNullException(nameof(user));
        return unchecked((int)(stream.Length - stream.Position));
    }
}
