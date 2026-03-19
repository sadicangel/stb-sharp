#if defined(_WIN32)
#define STBIWDEF __declspec(dllexport)
#define STBIW_WINDOWS_UTF8
#else
#define STBIWDEF __attribute__((visibility("default")))
#endif

#define STB_IMAGE_WRITE_IMPLEMENTATION
#include "stb_image_write.h"
