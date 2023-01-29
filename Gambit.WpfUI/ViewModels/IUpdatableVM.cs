using Gambit.Core;

namespace Gambit.UI.ViewModels;

public interface IUpdatableVM
{
	void Load (Emulator emu);
	void Update (Emulator emu);
	void Unload ();
}