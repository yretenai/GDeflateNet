using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace GDeflateNet;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal record struct GDeflatePage(nint Data, int Size);

internal static partial class NativeMethods {
	private const string LibName = "libGDeflate";

	static NativeMethods() {
		NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
	}

	private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath) {
		if (libraryName != LibName) {
			return IntPtr.Zero;
		}

		if (NativeLibrary.TryLoad(libraryName, assembly, searchPath, out var handle)) {
			return handle;
		}

		var cwd = AppDomain.CurrentDomain.BaseDirectory;
		var target = Path.Combine(cwd, $"runtimes/{RuntimeInformation.RuntimeIdentifier}/native/{libraryName}");

		switch (Environment.OSVersion.Platform) {
			case PlatformID.Win32NT:
				target += ".dll";
				break;
			case PlatformID.MacOSX:
				target += ".dylib";
				break;
			case PlatformID.Unix:
				target += ".so";
				break;
		}

		if (File.Exists(target)) {
			return NativeLibrary.Load(target);
		}

		throw new DllNotFoundException($"Unable to load {libraryName} ({RuntimeInformation.ProcessArchitecture}/{RuntimeInformation.RuntimeIdentifier}/{searchPath})");
	}

	[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	internal static partial nint libdeflate_alloc_gdeflate_compressor(int level);

	[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	internal static unsafe partial nint libdeflate_gdeflate_compress(nint compressor, nint src, nint srcSize, GDeflatePage* pages, nint numPages);

	[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	internal static partial void libdeflate_free_gdeflate_compressor(nint compressor);

	[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	internal static partial nint libdeflate_alloc_gdeflate_decompressor();

	[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	internal static unsafe partial int libdeflate_gdeflate_decompress(nint compressor, GDeflatePage* pages, nint numPages, nint dst, nint dstSize, out nint bytes);

	[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	internal static partial void libdeflate_free_gdeflate_decompressor(nint compressor);
}
