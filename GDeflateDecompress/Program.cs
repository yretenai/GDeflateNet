using System.Buffers;
using System.Runtime.InteropServices;
using GDeflateNet;

namespace GDeflateDecompress;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
internal record struct TileStreamHeader {
	private const uint TileSizeIdxMask = 0x3u;
	private const uint LastTileSizeMask = 0xFFFFCu;
	private const uint ReservedMask = 0xFFF00000U;
	private const int LastTileSizeShift = 2;
	private const int ReservedShift = 20;

	public byte Id { get; set; }
	public byte Magic { get; set; }
	public ushort NumTiles { get; set; }
	public uint Info { get; set; }

	public int TileSizeIdx => (int) (Info & TileSizeIdxMask);
	public int LastTileSize => (int) (Info & LastTileSizeMask) >> LastTileSizeShift;
	public int Reserved => (int) (Info & ReservedMask) >> ReservedShift;
}

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
		if (header.Id != (header.Magic ^ 0xFF)) {
			Console.Error.WriteLine("Invalid magic value!");
			return;
		}

		var size = header.NumTiles * 0x10000 - (header.LastTileSize == 0 ? 0 : 0x10000 - header.LastTileSize);
		using var dec = MemoryPool<byte>.Shared.Rent(size);
		if (!GDeflate.Decompress(com.Memory[..(int) info.Length], dec.Memory[..size], 1)) {
			Console.Error.WriteLine("GDeflate failure");
			return;
		}

		output.SetLength(size);
		output.Write(dec.Memory[..size].Span);
	}
}
