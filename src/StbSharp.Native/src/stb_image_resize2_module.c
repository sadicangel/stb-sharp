#if defined(_WIN32)
#define STBIRDEF __declspec(dllexport)
#else
#define STBIRDEF __attribute__((visibility("default")))
#endif

#define STB_IMAGE_RESIZE_IMPLEMENTATION
#include "stb_image_resize2.h"