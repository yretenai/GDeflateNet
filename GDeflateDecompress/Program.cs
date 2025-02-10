using System.Buffers;
using System.Runtime.InteropServices;
using GDeflateNet;

namespace GDeflateDecompress;

internal class Program {
	private static void Main(string[] args) {
		if (args.Length < 1) {
			Console.Out.WriteLine("Usage: GDeflateDecompress path/to/file [path/to/target]");
			return;
		}

		var compressedPath = args[0];
		var uncompressedPath = args.Length > 1 ? args[1] : compressedPath;

		var info = new FileInfo(compressedPath);
		using var com = MemoryPool<byte>.Shared.Rent((int) info.Length);
		using var input = new FileStream(compressedPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		using var output = new FileStream(uncompressedPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
		input.ReadExactly(com.Memory[..(int) info.Length].Span);
		var header = MemoryMarshal.Read<TileStreamHeader>(com.Memory.Span);
		if (!header.Valid) {
			Console.Error.WriteLine("Invalid magic value!");
			return;
		}

		var size = header.UncompressedSize;
		using var dec = MemoryPool<byte>.Shared.Rent(size);
		if (!GDeflate.Decompress(com.Memory[..(int) info.Length], dec.Memory[..size])) {
			Console.Error.WriteLine("GDeflate failure");
			return;
		}

		output.SetLength(size);
		output.Write(dec.Memory[..size].Span);
	}
}
