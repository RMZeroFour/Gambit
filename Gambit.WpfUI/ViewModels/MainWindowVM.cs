using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using Gambit.Core;
using Gambit.UI.Models;
using Gambit.UI.Services;

namespace Gambit.UI.ViewModels;

public partial class MainWindowVM : ObservableRecipient
{
	public DebugHostVM TopHost { get; }
	public DebugHostVM BottomHost { get; }
	public GraphicsVM Screen { get; }

	private Cartridge gameCart;
	private string romFilePath;

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(LoadRomCommand))]
	[NotifyCanExecuteChangedFor(nameof(UnloadRomCommand))]
	[NotifyPropertyChangedFor(nameof(EmulatorReady))]
	private Emulator emulator;
	public bool EmulatorReady => Emulator is not null;

	[ObservableProperty]
	private bool showDebugTools;

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(PressPlayPauseCommand))]
	[NotifyCanExecuteChangedFor(nameof(PressStepCommand))]
	private bool isRunning = false;
	private bool queuedFrame;

	private readonly InputVM input;

	private readonly ITimerService timer;
	private readonly IFilePickerService picker;

	private readonly IUpdatableVM[] allComponents;
	private readonly IUpdatableVM[] nonDebugComponents;

	public MainWindowVM (ITimerService ts, IFilePickerService fs, GraphicsVM gvm, InputVM ivm, Func<DebugHostVM> host)
	{
		timer = ts;
		timer.SetInterval(TimeSpan.FromSeconds(1.0 / 60))
			.SetCallback(NextFrame);
		CompositionTarget.Rendering += (_, _) => UpdateUI();

		picker = fs;
		picker.SetInitialDir(Environment.CurrentDirectory)
			.SetFilter("GameBoy ROMs (*.gb)|*.gb;*.gbc;*.bin;*.dat;*.rom")
			.SetExtension(".gb");

		TopHost = host();
		BottomHost = host();

		input = ivm;

		Screen = gvm;
		Screen.Source = GraphicsVM.BitmapSource.FinalBitmap;

		allComponents = new IUpdatableVM[] { TopHost, BottomHost, Screen, input };
		nonDebugComponents = new IUpdatableVM[] { Screen, input };
	}

	private void NextFrame ()
	{
		if (IsRunning)
		{
			//lock (Emulator)
			{
				Emulator.RunToSyncPoint(debug: ShowDebugTools);
				queuedFrame = true;
			}
		}
	}

	private void UpdateUI ()
	{
		if (IsRunning && queuedFrame)
		{
			IUpdatableVM[] components = ShowDebugTools ? allComponents : nonDebugComponents;
			foreach (var component in components)
				component.Update(Emulator);
		}
	}

	[RelayCommand]
	private void PressPlayPause () => IsRunning = !IsRunning;

	private bool CanPressStep () => !IsRunning;
	[RelayCommand(CanExecute = nameof(CanPressStep))]
	private void PressStep ()
	{
		Emulator.NextCycle(debug: ShowDebugTools);
		foreach (var component in allComponents)
			component.Update(Emulator);
	}

	private static string GetSavFilePath (string rom) => Path.ChangeExtension(rom, ".sav");

	private bool CanLoadRom () => !EmulatorReady;
	[RelayCommand(CanExecute = nameof(CanLoadRom))]
	private async Task LoadRom ()
	{
		romFilePath = picker.OpenFile();
		if (romFilePath is null)
			return;

		byte[] bytes = await File.ReadAllBytesAsync(romFilePath);
		gameCart = new(bytes);

		if (gameCart.HasBattery)
		{
			string savFilePath = GetSavFilePath(romFilePath);
			if (File.Exists(savFilePath))
			{
				using FileStream stream = File.OpenRead(savFilePath);
				await gameCart.LoadRamFromStream(stream);
			}
		}

		Emulator = new Emulator(gameCart);
		foreach (var component in allComponents)
			component.Load(Emulator);

		IsRunning = true;
		timer.Start();
	}

	private bool CanUnloadRom () => EmulatorReady;
	[RelayCommand(CanExecute = nameof(CanUnloadRom))]
	private async Task UnloadRom ()
	{
		if (gameCart.HasBattery)
		{
			string savFilePath = GetSavFilePath(romFilePath);
			using FileStream stream = File.OpenWrite(savFilePath);
			await gameCart.WriteRamToStream(stream);
		}

		gameCart = null;

		Emulator = null;
		foreach (var component in allComponents)
			component.Unload();

		IsRunning = false;
		timer.Stop();
	}

	[RelayCommand]
	private async Task SaveState ()
	{
		ISaveData saveData = new JsonSaveData();
		Emulator.SaveState(saveData);

		// set path to same as rom file path with diff ext
		string path = @"C:\Users\Ritam\Desktop\save.json";
		using Stream stream = File.Open(path, FileMode.Create);
		await saveData.SaveToStream(stream);
	}

	[RelayCommand]
	private async Task LoadState ()
	{
		ILoadData loadData = new JsonLoadData();

		string path = @"C:\Users\Ritam\Desktop\save.json";
		using Stream stream = File.OpenRead(path);
		await loadData.LoadFromStream(stream);

		// use return value somehow
		Emulator.LoadState(loadData);
	}

	[RelayCommand]
	private async Task CloseWindow ()
	{
		if (gameCart is not null)
			await UnloadRom();
	}
}