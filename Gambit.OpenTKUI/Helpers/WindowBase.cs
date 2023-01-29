using System.ComponentModel;
using System.Numerics;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Gambit.OpenTKUI;

public abstract class WindowBase : GameWindow
{
	private ImGuiController controller;

	public WindowBase (int w, int h, string title) : base(new(), new() { Size = new(w, h), Title = title }) { }

	protected abstract void Create ();
	protected abstract void Update (float time);
	protected abstract void Render (float time);
	protected abstract void Finish ();

	protected override void OnLoad ()
	{
		base.OnLoad();

		controller = new ImGuiController(ClientSize.X, ClientSize.Y);

		Create();
	}

	protected override void OnResize (ResizeEventArgs e)
	{
		base.OnResize(e);

		GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

		controller.WindowResized(ClientSize.X, ClientSize.Y);
	}

	protected override void OnUpdateFrame (FrameEventArgs e)
	{
		base.OnUpdateFrame(e);

		Update((float)e.Time);

		controller.Update(MouseState, KeyboardState, (float)e.Time);
	}

	protected override void OnRenderFrame (FrameEventArgs e)
	{
		base.OnRenderFrame(e);

		Render((float)e.Time);

		controller.Render();

		ImGuiController.CheckGLError("End of frame");

		SwapBuffers();
	}

	protected override void OnTextInput (TextInputEventArgs e)
	{
		base.OnTextInput(e);
		controller.PressChar((char)e.Unicode);
	}

	protected override void OnMouseWheel (MouseWheelEventArgs e)
	{
		base.OnMouseWheel(e);
		controller.MouseScroll(e.Offset);
	}

	protected override void OnClosing (CancelEventArgs e)
	{
		base.OnClosing(e);
		Finish();
	}

	protected ImFontPtr LoadFont (string path, int size)
	{
		var font = ImGui.GetIO().Fonts.AddFontFromFileTTF(path, size);
		controller.RecreateFontDeviceTexture();
		return font;
	}

	protected void Clear (Vector4 col)
	{
		GL.ClearColor(col.X, col.Y, col.Z, col.W);
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
	}
}