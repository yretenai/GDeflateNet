using System.Buffers;
using GDeflateNet;

namespace GDeflateCompress;

internal class Program {
	private static void Main(string[] args) {
		if (args.Length < 1) {
			Console.Out.WriteLine("Usage: GDeflateCompress path/to/file [path/to/target]");
			return;
		}

		var uncompressedPath = args[0];
		var compressedPath = args.Length > 1 ? args[1] : uncompressedPath;

		var info = new FileInfo(uncompressedPath);
		using var dec = MemoryPool<byte>.Shared.Rent((int) info.Length);
		using var input = new FileStream(uncompressedPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		using var output = new FileStream(compressedPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
		input.ReadExactly(dec.Memory[..(int) info.Length].Span);

		using var com = GDeflate.Compress(dec.Memory[..(int) info.Length], 12, out var size);
		output.SetLength(size);
		output.Write(com.Memory[..size].Span);
	}
}
