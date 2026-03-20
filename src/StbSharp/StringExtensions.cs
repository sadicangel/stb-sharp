using System.Buffers;
using System.Text;
using System.Text.Unicode;
using DotNext.Buffers;

namespace StbSharp;

internal static class StringExtensions
{
    extension(string value)
    {
        internal SpanOwner<byte> ToCString()
        {
            var spanOwner = new SpanOwner<byte>(Encoding.UTF8.GetMaxByteCount(value.Length) + 1);
            var operationStatus = Utf8.FromUtf16(value, spanOwner.Span, out _, out int length);
            if (operationStatus != OperationStatus.Done)
            {
                spanOwner.Dispose();
                throw new ArgumentException("Failed to convert UTF16 to UTF8");
            }

            spanOwner.Span[length] = 0;
            return spanOwner;
        }
    }
}
