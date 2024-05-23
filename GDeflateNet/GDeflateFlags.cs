using System;

namespace GDeflateNet;

[Flags]
public enum GDeflateFlags : uint {
	CompressSingleThread = 0x200,
}

public static class GDeflateFlagsExtensions {
	public static bool HasFlagFast(this GDeflateFlags value, GDeflateFlags flag) => (value & flag) != 0;
}
