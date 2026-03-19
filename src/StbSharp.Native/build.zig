const std = @import("std");

pub fn build(b: *std.Build) void {
    const target = b.standardTargetOptions(.{});
    const optimize = b.standardOptimizeOption(.{});

    const lib = b.addLibrary(.{
        .name = "stb_image",
        .linkage = .dynamic,
        .root_module = b.createModule(.{
            .target = target,
            .optimize = optimize,
            .link_libc = true,
        }),
    });

    lib.addIncludePath(b.path("include"));
    lib.addCSourceFiles(.{
        .root = b.path("src"),
        .files = discoverCSources(b, "src"),
        .flags = &.{"-std=c99"},
    });

    const install = b.addInstallArtifact(lib, .{
        .dest_dir = .{ .override = .prefix },
        .implib_dir = .disabled,
        .pdb_dir = .disabled,
        .h_dir = .disabled,
    });

    b.getInstallStep().dependOn(&install.step);
}

fn discoverCSources(b: *std.Build, relative_dir: []const u8) []const []const u8 {
    var dir = std.fs.openDirAbsolute(b.pathFromRoot(relative_dir), .{ .iterate = true }) catch |err| {
        std.debug.panic("unable to open '{s}': {s}", .{ relative_dir, @errorName(err) });
    };
    defer dir.close();

    var iterator = dir.iterate();
    var files = std.ArrayList([]const u8).init(b.allocator);

    while (iterator.next() catch |err| {
        std.debug.panic("unable to enumerate '{s}': {s}", .{ relative_dir, @errorName(err) });
    }) |entry| {
        if (entry.kind != .file or !std.mem.eql(u8, std.fs.path.extension(entry.name), ".c")) {
            continue;
        }

        files.append(b.dupe(entry.name)) catch @panic("OOM");
    }

    if (files.items.len == 0) {
        std.debug.panic("no C sources found in '{s}'", .{relative_dir});
    }

    std.sort.heap([]const u8, files.items, {}, struct {
        fn lessThan(_: void, lhs: []const u8, rhs: []const u8) bool {
            return std.mem.lessThan(u8, lhs, rhs);
        }
    }.lessThan);

    return files.toOwnedSlice() catch @panic("OOM");
}
