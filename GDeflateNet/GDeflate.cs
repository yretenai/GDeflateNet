using System;
using System.Buffers;

namespace GDeflateNet;

public static class GDeflate {
	public static long GetBounds(long bufferSize) => NativeMethods.GDeflateCompressBound(bufferSize);

	public static unsafe Memory<byte> Compress(ReadOnlyMemory<byte> buffer, int level, GDeflateFlags flags) {
		var outSize = GetBounds(buffer.Length);
		using var pool = MemoryPool<byte>.Shared.Rent((int) outSize);

		using var inPin = buffer.Pin();
		using var outPin = pool.Memory.Pin();

		if (level > 12) {
			level = 12;
		}

		if (level < 0) {
			level = 0;
		}

		var success = NativeMethods.GDeflateCompress(outPin.Pointer, ref outSize, inPin.Pointer, buffer.Length, level, flags);
		return !success ? Memory<byte>.Empty : pool.Memory[..(int) outSize].ToArray();
	}

	public static unsafe bool Decompress(ReadOnlyMemory<byte> buffer, Memory<byte> decompressedBuffer, int numWorkers) {
		using var inPin = buffer.Pin();
		using var outPin = decompressedBuffer.Pin();

		return NativeMethods.GDeflateDecompress(outPin.Pointer, decompressedBuffer.Length, inPin.Pointer, buffer.Length, numWorkers);
	}
}
