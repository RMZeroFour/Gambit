using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Gambit.Core;

namespace Gambit.UI.ViewModels;

public partial class GraphicsVM : ObservableObject, IUpdatableVM
{
	[ObservableProperty]
	private WriteableBitmap renderTarget;

	[ObservableProperty]
	private BitmapSource source;
	private bool sourceChanged;

	private VideoBuffer sourceBitmap;

	partial void OnSourceChanged (BitmapSource value)
	{
		sourceChanged = true;
	}

	public void Load (Emulator emu)
	{
		sourceChanged = true;
	}

	public void Update (Emulator emu)
	{
		if (sourceChanged)
		{
			sourceBitmap = source switch
			{
				BitmapSource.FinalBitmap => emu.VideoSystem.FinalBuffer,
				BitmapSource.BackgroundLayer => emu.VideoSystem.BackgroundLayer,
				BitmapSource.WindowLayer => emu.VideoSystem.WindowLayer,
				BitmapSource.SpritesLayer => emu.VideoSystem.SpriteData,
				BitmapSource.TileMap => emu.VideoSystem.TileData,
				_ or BitmapSource.None => null
			};

			if (sourceBitmap is not null)
				RenderTarget = BitmapFactory.New(sourceBitmap.Width, sourceBitmap.Height);

			sourceChanged = false;
		}

		RenderTarget?.ForEach((x, y) => colorMap[sourceBitmap[x, y]]);
	}

	public void Unload ()
	{
		RenderTarget = null;
	}

	private static readonly Dictionary<Pixel, Color> colorMap = new()
	{
		{ Pixel.White, Color.FromRgb(224, 248, 208) },
		{ Pixel.LightGray, Color.FromRgb(136, 192, 112) },
		{ Pixel.DarkGray, Color.FromRgb(52, 104, 86) },
		{ Pixel.Black, Color.FromRgb(8, 24, 32) },
	};

	public enum BitmapSource
	{
		None = 0,
		FinalBitmap,
		BackgroundLayer,
		WindowLayer,
		SpritesLayer,
		TileMap
	}
}
