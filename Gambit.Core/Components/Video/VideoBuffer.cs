using System;

namespace Gambit.Core;

public class VideoBuffer
{
	public int Width { get; }
	public int Height { get; }

	public Span<Pixel> Pixels => pixels;
	private readonly Pixel[] pixels;

	public ref Pixel this[int x, int y] => ref pixels[x + y * Width];

	public VideoBuffer (int width, int height)
	{
		Width = width;
		Height = height;
		pixels = new Pixel[Width * Height];
	}
}