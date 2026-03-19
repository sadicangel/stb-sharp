# StbSharp

StbSharp packages `stb_image` and `stb_image_write` as a managed x64-only NuGet with native assets for Windows, Linux, and macOS.
`dotnet build` builds the managed library plus the current host's native asset, and `dotnet pack` bundles all three native runtimes by invoking the Zig project in `src/StbSharp.Native`.
Any `.c` file added under `src/StbSharp.Native/src` is compiled into the same shared library.
