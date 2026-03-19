namespace StbSharp.Interop;

public sealed class NativeTypeNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
