#if defined(_WIN32)
#define STBIDEF __declspec(dllexport)
#else
#define STBIDEF __attribute__((visibility("default")))
#endif

#define STB_IMAGE_IMPLEMENTATION
#include "stb_image.h"
