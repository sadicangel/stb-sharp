# AGENTS.md

## Repo Summary

`StbSharp` is a .NET 10 x64-only wrapper around `stb_image`, `stb_image_write`, and related native functionality packaged as a NuGet with native assets for Windows, Linux, and macOS.

The repo is split into:

- `src/StbSharp`: managed API surface and higher-level wrapper code.
- `src/StbSharp/Interop`: P/Invoke bindings for the native library. Treat these files as generated unless there is a strong reason not to.
- `src/StbSharp.Native`: Zig-based native build that produces the shared `stb_image` library.
- `tests/StbSharp.Tests`: xUnit tests that exercise real read/write flows against the packaged native library.
- `scripts/Generate-Bindings.ps1`: regeneration script for interop bindings from the native headers.

## Tooling And Constraints

- Use the .NET 10 SDK.
- Use Zig from `PATH`, or set `ZIG_EXE` to the Zig executable.
- The library is x64-only. Keep `Platform` and `PlatformTarget` on `x64`.
- Warnings are treated as errors, `nullable` is enabled, and the repo uses preview C# language features.

## Common Commands

Build the solution:

```powershell
dotnet build StbSharp.slnx -p:Platform=x64
```

Run tests:

```powershell
dotnet test tests/StbSharp.Tests/StbSharp.Tests.csproj -p:Platform=x64
```

Create the NuGet package and all native runtime assets:

```powershell
dotnet pack src/StbSharp/StbSharp.csproj -c Release -p:Platform=x64
```

If the environment is sandboxed or the .NET CLI cannot write to the default user profile, set `DOTNET_CLI_HOME` to the repo-local `.dotnet` directory before running SDK commands.

## Native And Interop Workflow

- `src/StbSharp.Native/build.zig` automatically compiles every `.c` file under `src/StbSharp.Native/src` into the same shared library.
- If you add or update native headers under `src/StbSharp.Native/include`, regenerate the corresponding bindings in `src/StbSharp/Interop`.
- Regenerate bindings with:

```powershell
pwsh ./scripts/Generate-Bindings.ps1
```

- The binding script expects `ClangSharpPInvokeGenerator` to be available and downloads `Humanizer.Core` into `%TEMP%` on first use.
- Prefer updating the source header plus regeneration over hand-editing generated P/Invoke declarations.

## Change Guidelines

- Put public API behavior in `src/StbSharp`; keep low-level marshalling details in `Interop`.
- If you change packaging or runtime selection behavior, review both `src/StbSharp/StbSharp.csproj` and `src/StbSharp/buildTransitive/StbSharp.targets`.
- When adding new functionality, include at least one end-to-end test in `tests/StbSharp.Tests` that exercises the managed wrapper against the native library.
- Keep changes consistent with the current design: small wrapper types, explicit native ownership, and real file or byte-buffer roundtrip coverage.
