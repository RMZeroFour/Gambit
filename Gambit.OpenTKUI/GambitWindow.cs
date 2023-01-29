using System;
using System.IO;
using System.Numerics;
using Gambit.Core;
using ImGuiNET;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Gambit.OpenTKUI;

public class GambitWindow : WindowBase
{
	private ImFontPtr regularFont, boldFont;
	private Texture screenTex, bitmapTex;

	private Cartridge cartridge;
	private Emulator emulator;

	private string serialText;

	private ImGuiFileDialog openFileDialog;

	private bool paused = true;

	private static readonly string[] bitmapsArray = { "Background", "Window", "Sprites", "TileData" };
	private int bitmapIndex = 0;
	private VideoBuffer sourceBitmap;

	private string breakpointAddr = "0";

	private bool debugMode;

	private int palleteIndex = 0;
	private Vector4 whiteShade, lightGrayShade, darkGrayShade, blackShade;

	public GambitWindow () : base(800, 600, "Gambit") { }

	protected override void Create ()
	{
		base.FileDrop += (e) =>
		{
			string file = e.FileNames[0];
			if (Path.GetExtension(file) == ".gb" || Path.GetExtension(file) == ".gbc")
				LoadROM(file);
		};

		regularFont = LoadFont(@".\Assets\Fonts\Silkscreen-Regular.ttf", 20);
		boldFont = LoadFont(@".\Assets\Fonts\Silkscreen-Bold.ttf", 18);

		screenTex = new(160, 144);
		serialText = "";

		openFileDialog = new("Load ROM");
		openFileDialog.SetPopupCallback(LoadROM);

		palleteIndex = 0;
		whiteShade = new(224 / 255.0f, 248 / 255.0f, 208 / 255.0f, 1);
		lightGrayShade = new(136 / 255.0f, 192 / 255.0f, 112 / 255.0f, 1);
		darkGrayShade = new(52 / 255.0f, 104 / 255.0f, 86 / 255.0f, 1);
		blackShade = new(8 / 255.0f, 24 / 255.0f, 32 / 255.0f, 1);
	}

	private void LoadROM (string file)
	{
		Title = $"Gambit | {Path.GetFileNameWithoutExtension(file)}";

		var bytes = File.ReadAllBytes(file);
		cartridge = new(bytes);
		emulator = new(cartridge);
		emulator.Serial.SerialWrite += c => serialText += c;
	}

	protected override void Render (float time)
	{
		Clear(Vector4.Zero);

		ImGui.PushFont(regularFont);

		if (ImGui.BeginMainMenuBar())
		{
			if (ImGui.BeginMenu("File"))
			{
				if (ImGui.MenuItem("Load ROM", emulator is null))
				{
					//openFileDialog.InitPopup(System.Environment.CurrentDirectory);
					openFileDialog.InitPopup(@"D:\Projects\C#\Gambit\Gambit.OpenTKUI\bin\Debug\net6.0\Assets\Roms\");
				}

				if (ImGui.MenuItem("Unload ROM", emulator is not null))
				{
					emulator = null;
					cartridge = null;
				}

				//if (ImGui.MenuItem("Save State", emulator is not null))
				//{
				//	using FileStream fs = File.OpenWrite("./save.txt");
				//	ISaveData sd = new JsonSaveData();
				//	emulator.SaveState(sd);
				//	sd.SaveToStream(fs).Wait();
				//}

				//if (ImGui.MenuItem("Load State ROM", emulator is not null))
				//{
				//	using FileStream fs = File.OpenRead("./save.txt");
				//	ILoadData sd = new JsonLoadData();
				//	sd.LoadFromStream(fs).Wait();
				//	emulator.LoadState(sd);
				//}

				ImGui.EndMenu();
			}

			if (ImGui.BeginMenu("Pallete"))
			{
				if (ImGui.MenuItem("Retro Green", "G", palleteIndex == 0))
				{
					palleteIndex = 0;
					whiteShade = new(224 / 255.0f, 248 / 255.0f, 208 / 255.0f, 1);
					lightGrayShade = new(136 / 255.0f, 192 / 255.0f, 112 / 255.0f, 1);
					darkGrayShade = new(52 / 255.0f, 104 / 255.0f, 86 / 255.0f, 1);
					blackShade = new(8 / 255.0f, 24 / 255.0f, 32 / 255.0f, 1);
				}

				if (ImGui.MenuItem("Crisp White", "W", palleteIndex == 1))
				{
					palleteIndex = 1;
					whiteShade = new(1.0f);
					lightGrayShade = new(0.66f);
					darkGrayShade = new(0.33f);
					blackShade = new(0);
				}

				ImGui.EndMenu();
			}

			if (ImGui.BeginMenu("Debug"))
			{
				ImGui.MenuItem("Debug Mode", "D", ref debugMode);

				if (ImGui.MenuItem(paused ? "Play" : "Pause", "P"))
					paused = !paused;
				if (paused && ImGui.MenuItem("Step", "S"))
					emulator.NextCycle(debugMode);

				ImGui.EndMenu();
			}

			ImGui.EndMainMenuBar();
		}

		ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
		ImGui.SetNextWindowSize(new Vector2(160, 144) * 3, ImGuiCond.FirstUseEver);
		if (ImGui.Begin("Screen", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar))
		{
			ImGui.Text($"{ImGui.GetIO().Framerate} FPS");
			ImGui.Text("IF: " + Convert.ToString(emulator?.Bus[0xFF0F] ?? 0, 2).PadLeft(5, '0'));
			ImGui.SameLine();
			ImGui.Text("IE: " + Convert.ToString(emulator?.Bus[0xFFFF] ?? 0, 2).PadLeft(5, '0'));

			ImGui.SetWindowSize(new(ImGui.GetWindowSize().X, ImGui.GetWindowSize().X * 144 / 160));

			if (emulator is not null)
			{
				Span<Pixel> source = emulator.VideoSystem.FinalBuffer.Pixels;
				Span<Vector4> target = screenTex.Buffer;

				for (int i = 0; i < source.Length; i++)
					target[i] = MapPixel(source[i]);

				//for (int y = 0; y < screenTex.Size.Y; y++)
				//{
				//	for (int x = 0; x < screenTex.Size.X; x++)
				//	{
				//		Vector3 col = MapPixel(emulator.VideoSystem.FinalBuffer[x, y]);
				//		screenTex[x, y].X = col.X;
				//		screenTex[x, y].Y = col.Y;
				//		screenTex[x, y].Z = col.Z;
				//		screenTex[x, y].W = 1.0f;
				//	}
				//}

				screenTex.UpdatePixels();

				ImGui.Image((nint)screenTex.Handle, ImGui.GetContentRegionAvail(), new(0, 0), new(1, 1), Vector4.One);
			}

			ImGui.End();
		}
		ImGui.PopStyleVar();

		if (ImGui.Begin("Registers", ImGuiWindowFlags.NoCollapse))
		{
			if (emulator is not null)
			{
				ImGui.Text($"AF = {emulator.Processor.AF:X4}"); ImGui.SameLine();
				ImGui.Text($"BC = {emulator.Processor.BC:X4}");
				ImGui.Text($"DE = {emulator.Processor.DE:X4}"); ImGui.SameLine();
				ImGui.Text($"HL = {emulator.Processor.HL:X4}");
				ImGui.Text($"PC = {emulator.Processor.ProgramCounter:X4}"); ImGui.SameLine();
				ImGui.Text($"SP = {emulator.Processor.StackPointer:X4}");
			}
			ImGui.End();
		}

		if (ImGui.Begin("Serial", ImGuiWindowFlags.NoCollapse))
		{
			ImGui.Text(serialText);
			ImGui.End();
		}

		if (ImGui.Begin("Joypad", ImGuiWindowFlags.NoCollapse) && emulator is not null)
		{
			bool state = false;
			ImGui.BeginDisabled();
			state = emulator.Joypad.A; ImGui.Checkbox("A", ref state);
			state = emulator.Joypad.B; ImGui.Checkbox("B", ref state);
			state = emulator.Joypad.Start; ImGui.Checkbox("Start", ref state);
			state = emulator.Joypad.Select; ImGui.Checkbox("Select", ref state);
			state = emulator.Joypad.Left; ImGui.Checkbox("Left", ref state);
			state = emulator.Joypad.Right; ImGui.Checkbox("Right", ref state);
			state = emulator.Joypad.Up; ImGui.Checkbox("Up", ref state);
			state = emulator.Joypad.Down; ImGui.Checkbox("Down", ref state);
			ImGui.EndDisabled();
		}

		if (ImGui.Begin("Breakpoints", ImGuiWindowFlags.NoCollapse) && emulator is not null)
		{
			ImGui.InputText("##address", ref breakpointAddr, 10);
			if (ImGui.Button("+"))
			{
				ushort bp = System.Convert.ToUInt16(breakpointAddr, 16);
				emulator.Breakpoints.Add(bp);
				breakpointAddr = "0";
			}

			if (ImGui.BeginListBox("##breakpoints"))
			{
				foreach (ushort bp in emulator.Breakpoints)
				{
					ImGui.Text($"0x{bp:X4}");
					ImGui.SameLine();
					if (ImGui.Button($"X##{bp}"))
						emulator.Breakpoints.Remove(bp);
				}
				ImGui.EndListBox();
			}

			ImGui.End();
		}

		if (ImGui.Begin("Bitmaps", ImGuiWindowFlags.NoCollapse) && emulator is not null)
		{
			if (ImGui.Combo("##bitmap", ref bitmapIndex, bitmapsArray, bitmapsArray.Length))
			{
				sourceBitmap = bitmapIndex switch
				{
					0 => emulator.VideoSystem.BackgroundLayer,
					1 => emulator.VideoSystem.WindowLayer,
					2 => emulator.VideoSystem.SpriteData,
					3 => emulator.VideoSystem.TileData,
					_ => null,
				};

				bitmapTex?.Dispose();
				bitmapTex = new(sourceBitmap.Width, sourceBitmap.Height);
			}

			if (bitmapTex is not null)
			{
				Span<Pixel> source = sourceBitmap.Pixels;
				Span<Vector4> target = bitmapTex.Buffer;

				for (int i = 0; i < source.Length; i++)
					target[i] = MapPixel(source[i]);

				//for (int y = 0; y < bitmapTex.Size.Y; y++)
				//{
				//	for (int x = 0; x < bitmapTex.Size.X; x++)
				//	{
				//		Vector3 col = MapPixel(sourceBitmap[x, y]);
				//		bitmapTex[x, y].X = col.X;
				//		bitmapTex[x, y].Y = col.Y;
				//		bitmapTex[x, y].Z = col.Z;
				//		bitmapTex[x, y].W = 1.0f;
				//	}
				//}

				bitmapTex.UpdatePixels();

				ImGui.Image((nint)bitmapTex.Handle, ImGui.GetContentRegionAvail());
			}
		}

		openFileDialog.RenderPopup();

		ImGui.PopFont();
	}


	private Vector4 MapPixel (Pixel p)
	{
		return p switch
		{
			Pixel.White => whiteShade,
			Pixel.LightGray => lightGrayShade,
			Pixel.DarkGray => darkGrayShade,
			Pixel.Black => blackShade,
			_ => Vector4.One,
		};
	}

	protected override void Update (float time)
	{
		if (emulator is not null && !paused)
		{
			paused = emulator.RunToSyncPoint(debugMode);

			emulator.Joypad.A = KeyboardState.IsKeyDown(Keys.Z);
			emulator.Joypad.B = KeyboardState.IsKeyDown(Keys.X);
			emulator.Joypad.Start = KeyboardState.IsKeyDown(Keys.Enter);
			emulator.Joypad.Select = KeyboardState.IsKeyDown(Keys.Space);
			emulator.Joypad.Left = KeyboardState.IsKeyDown(Keys.Left);
			emulator.Joypad.Right = KeyboardState.IsKeyDown(Keys.Right);
			emulator.Joypad.Up = KeyboardState.IsKeyDown(Keys.Up);
			emulator.Joypad.Down = KeyboardState.IsKeyDown(Keys.Down);
		}
	}

	protected override void Finish ()
	{

	}
}