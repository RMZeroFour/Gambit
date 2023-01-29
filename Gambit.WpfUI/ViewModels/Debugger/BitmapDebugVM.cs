using Gambit.Core;

namespace Gambit.UI.ViewModels;

public partial class BitmapDebugVM : ObservableObject, IDebugToolVM
{
	public string DisplayName => "Bitmaps";

	public GraphicsVM Bitmap { get; }
	public GraphicsVM.BitmapSource[] Sources { get; }

	[ObservableProperty]
	private GraphicsVM.BitmapSource current;

	public BitmapDebugVM (GraphicsVM gvm)
	{
		Bitmap = gvm;

		Sources = new[]
		{
			GraphicsVM.BitmapSource.BackgroundLayer,
			GraphicsVM.BitmapSource.WindowLayer,
			GraphicsVM.BitmapSource.SpritesLayer,
			GraphicsVM.BitmapSource.TileMap,
		};
		Current = Sources[^1];
	}

	partial void OnCurrentChanged (GraphicsVM.BitmapSource value)
	{
		Bitmap.Source = Current;
	}

	public void Load (Emulator emu) => Bitmap.Load(emu);
	public void Update (Emulator emu) => Bitmap.Update(emu);
	public void Unload () => Bitmap.Unload();
}
