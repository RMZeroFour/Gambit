namespace Gambit.Core;

public static class BinaryExtensions
{
	public static byte GetBit(this byte b, int pos) => (byte)((b >> pos) & 1u);
	public static bool TestBit(this byte b, int pos) => (b & (1u << pos)) != 0;
	public static byte SetBit(this byte b, int pos) => (byte)(b | (1u << pos));
	public static byte UnsetBit(this byte b, int pos) => (byte)(b & ~(1u << pos));
	public static byte FlipBit(this byte b, int pos) => (byte)(b ^ (1u << pos));
	public static byte ModifyBit(this byte b, int pos, bool value) => (byte)((b & ~(1u << pos)) | ((value ? 1u : 0) << pos));

	public static byte AsBit(this bool b) => (byte)(b ? 1 : 0);
}
