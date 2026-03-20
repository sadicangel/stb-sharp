namespace StbSharp;

public enum PixelComponents
{
    Default = 0,
    Grey = 1,
    GreyAlpha = 2,
    Rgb = 3,
    Rgba = 4
}

public static class PixelComponentsExtensions
{
    extension(PixelComponents components)
    {
        public int Count => (int)components;
    }
}
