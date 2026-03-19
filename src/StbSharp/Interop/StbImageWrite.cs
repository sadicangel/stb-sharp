using System.Runtime.InteropServices;

namespace StbSharp.Interop
{
    internal static unsafe partial class StbImageWrite
    {
        [DllImport("stb_image", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int stbi_write_png([NativeTypeName("const char *")] byte* filename, int w, int h, int comp, [NativeTypeName("const void *")] void* data, int stride_in_bytes);

        [DllImport("stb_image", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int stbi_write_bmp([NativeTypeName("const char *")] byte* filename, int w, int h, int comp, [NativeTypeName("const void *")] void* data);

        [DllImport("stb_image", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int stbi_write_tga([NativeTypeName("const char *")] byte* filename, int w, int h, int comp, [NativeTypeName("const void *")] void* data);

        [DllImport("stb_image", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int stbi_write_hdr([NativeTypeName("const char *")] byte* filename, int w, int h, int comp, [NativeTypeName("const float *")] float* data);

        [DllImport("stb_image", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int stbi_write_jpg([NativeTypeName("const char *")] byte* filename, int x, int y, int comp, [NativeTypeName("const void *")] void* data, int quality);

        [DllImport("stb_image", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int stbi_write_png_to_func([NativeTypeName("stbi_write_func *")] delegate* unmanaged[Cdecl]<void*, void*, int, void> func, void* context, int w, int h, int comp, [NativeTypeName("const void *")] void* data, int stride_in_bytes);

        [DllImport("stb_image", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int stbi_write_bmp_to_func([NativeTypeName("stbi_write_func *")] delegate* unmanaged[Cdecl]<void*, void*, int, void> func, void* context, int w, int h, int comp, [NativeTypeName("const void *")] void* data);

        [DllImport("stb_image", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int stbi_write_tga_to_func([NativeTypeName("stbi_write_func *")] delegate* unmanaged[Cdecl]<void*, void*, int, void> func, void* context, int w, int h, int comp, [NativeTypeName("const void *")] void* data);

        [DllImport("stb_image", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int stbi_write_hdr_to_func([NativeTypeName("stbi_write_func *")] delegate* unmanaged[Cdecl]<void*, void*, int, void> func, void* context, int w, int h, int comp, [NativeTypeName("const float *")] float* data);

        [DllImport("stb_image", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int stbi_write_jpg_to_func([NativeTypeName("stbi_write_func *")] delegate* unmanaged[Cdecl]<void*, void*, int, void> func, void* context, int x, int y, int comp, [NativeTypeName("const void *")] void* data, int quality);

        [DllImport("stb_image", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void stbi_flip_vertically_on_write(int flip_boolean);
    }
}
