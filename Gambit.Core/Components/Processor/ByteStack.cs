using System;

namespace Gambit.Core;

public class ByteStack
{
	private readonly byte[] buffer;
	private int front = 0;

	public Span<byte> Buffer => buffer;

	public ByteStack (int size) => buffer = new byte[size];

	public void Clear () => front = 0;

	public void PushByte (byte b) => buffer[front++] = b;
	public void PushWord (ushort b) { buffer[front++] = (byte)(b & 0xFF); buffer[front++] = (byte)(b >> 8); }

	public byte PopByte () => buffer[--front];
	public ushort PopWord () => (ushort)((buffer[--front] << 8) | buffer[--front]);
}