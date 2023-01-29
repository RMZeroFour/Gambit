using Gambit.Core;

namespace Gambit.UI.ViewModels;

public partial class SerialDebugVM : ObservableObject, IDebugToolVM
{
	public string DisplayName => "Serial";

	[ObservableProperty]
	private string outputText = "";

	public void Load (Emulator emu) => emu.Serial.SerialWrite += OnSerialWritten;
	public void Update (Emulator emu) { }
	public void Unload () { }

	private void OnSerialWritten (char next) => OutputText += next;

	[RelayCommand]
	private void ClearText () => OutputText = "";
}
