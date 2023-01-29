using System;
using System.Numerics;
using OpenTK.Graphics.OpenGL4;

namespace Gambit.OpenTKUI;

public class Texture
{
	public int Handle { get; }

	public Vector2 Size { get; }
	public Vector4[] Buffer { get; }

	public ref Vector4 this[int x, int y] => ref Buffer[(int)(x + y * Size.X)];

	public Texture (int w, int h)
	{
		Size = new(w, h);
		Buffer = new Vector4[w * h];

		Handle = GL.GenTexture();
		GL.BindTexture(TextureTarget.Texture2D, Handle);

		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

		GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
			(int)Size.X, (int)Size.Y, 0, PixelFormat.Rgba, PixelType.Float, Buffer);
	}

	public void Clear (Vector4 c) => Array.Fill(Buffer, c);

	public void UpdatePixels ()
	{
		GL.BindTexture(TextureTarget.Texture2D, Handle);

		GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0,
			(int)Size.X, (int)Size.Y, PixelFormat.Rgba, PixelType.Float, Buffer);
	}

	public void Dispose ()
	{
		GL.DeleteTexture(Handle);
	}
}