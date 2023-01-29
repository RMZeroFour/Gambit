using Gambit.Core;
using Gambit.UI.Services;

namespace Gambit.UI.ViewModels;

public class InputVM : ObservableObject, IUpdatableVM
{
	private readonly IGamepadService gamepad;
	public InputVM (IGamepadService gs) => gamepad = gs;

	public void Load (Emulator emu) { }
	public void Update (Emulator emu) => gamepad.UpdateJoypad(emu.Joypad);
	public void Unload () { }
}