using System.Runtime.InteropServices;

namespace GDeflateNet;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
public record struct TileStreamHeader() {
	public TileStreamCompressor Id {
		get;
		set {
			field = value;
			Magic = (byte) (0xFF ^ (byte) value);
		}
	}

	public byte Magic { get; private set; }
	public ushort NumTiles { get; set; }
	public uint Flags { get; set; } = 1;

	public int TileSizeIndex {
		get => (int) (Flags & 3);
		set => Flags = (Flags & 0xFFFFFFFCU) | ((uint) value & 3);
	}

	public int LastTileSize {
		get => (int) ((Flags >> 2) & 0x3FFFFU);
		set => Flags = (Flags & 0xFFF00003U) | (((uint) value & 0x3FFFFU) << 2);
	}

	public int Reserved {
		get => (int) (Flags >> 20);
		set => Flags = (Flags & 0xFFFFFU) | (((uint) value & 0xFFFFFU) << 20);
	}

	public bool Valid => (byte) Id == (0xFF ^ Magic);

	public int UncompressedSize {
		get => NumTiles * GDeflate.TileSize - (LastTileSize == 0 ? 0 : GDeflate.TileSize - LastTileSize);
		set {
			NumTiles = (ushort) (value / GDeflate.TileSize);
			LastTileSize = value - NumTiles * GDeflate.TileSize;
			if (LastTileSize > 0) {
				NumTiles++;
			}
		}
	}
}
