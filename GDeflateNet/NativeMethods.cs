using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace GDeflateNet;

internal static partial class NativeMethods {
	private const string LibName = "libGDeflateCore";

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
	internal static partial long GDeflateCompressBound(long size);

	[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static unsafe partial bool GDeflateCompress(void* output, ref long outputSize, void* input, long inputSize, int level, GDeflateFlags flags);

	[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static unsafe partial bool GDeflateDecompress(void* output, long outputSize, void* input, long inputSize, int numWorkers);
}
