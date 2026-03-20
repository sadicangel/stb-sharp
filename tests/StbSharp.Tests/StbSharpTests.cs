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
}
