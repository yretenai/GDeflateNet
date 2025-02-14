using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace GDeflateNet;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal record struct GDeflatePage(nint Data, int Size);

internal static partial class NativeMethods {
	private const string LibName = "GDeflate";

	static NativeMethods() {
		NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
	}

	private static nint DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath) {
		if (NativeLibrary.TryLoad(libraryName, assembly, searchPath, out var handle)) {
			return handle;
		}

		if (searchPath != null && !searchPath.Value.HasFlag(DllImportSearchPath.AssemblyDirectory)) {
			return nint.Zero;
		}

		var name = Path.GetFileNameWithoutExtension(libraryName);
		var cwd = AppDomain.CurrentDomain.BaseDirectory;

		string ext;
		if (OperatingSystem.IsWindows()) {
			ext = ".dll";
		} else if (OperatingSystem.IsLinux()) {
			ext = ".so";
		} else if (OperatingSystem.IsMacOS()) {
			ext = ".dylib";
		} else {
			return nint.Zero;
		}

		foreach (var dir in new[] { Path.Combine(cwd, $"runtimes/{RuntimeInformation.RuntimeIdentifier}/native/"), cwd }) {
			foreach (var libName in new[] { name, "lib" + name, name + "-0", $"lib{name}-0" }) {
				var target = Path.Combine(dir, libName) + ext;
				if (File.Exists(target)) {
					var ptr = NativeLibrary.Load(target);
					if (ptr != nint.Zero) {
						return ptr;
					}
				}
			}
		}

		return nint.Zero;
	}

	[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories | DllImportSearchPath.AssemblyDirectory)]
	internal static partial nint libdeflate_alloc_gdeflate_compressor(int level);

	[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories | DllImportSearchPath.AssemblyDirectory)]
	internal static unsafe partial nint libdeflate_gdeflate_compress(nint compressor, nint src, nint srcSize, GDeflatePage* pages, nint numPages);

	[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories | DllImportSearchPath.AssemblyDirectory)]
	internal static partial void libdeflate_free_gdeflate_compressor(nint compressor);

	[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories | DllImportSearchPath.AssemblyDirectory)]
	internal static partial nint libdeflate_alloc_gdeflate_decompressor();

	[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories | DllImportSearchPath.AssemblyDirectory)]
	internal static unsafe partial int libdeflate_gdeflate_decompress(nint compressor, GDeflatePage* pages, nint numPages, nint dst, nint dstSize, out nint bytes);

	[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories | DllImportSearchPath.AssemblyDirectory)]
	internal static partial void libdeflate_free_gdeflate_decompressor(nint compressor);
}
