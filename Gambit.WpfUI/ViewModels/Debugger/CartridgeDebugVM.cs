using Gambit.Core;

namespace Gambit.UI.ViewModels;

public partial class CartridgeDebugVM : ObservableObject, IDebugToolVM
{
	public string DisplayName => "Cartridge";

	[ObservableProperty]
	private string mbc;

	[ObservableProperty]
	private string gameTitle;

	[ObservableProperty]
	private bool battery;

	[ObservableProperty]
	private int romBanks, ramBanks;

	public void Load (Emulator emu)
	{
		Mbc = emu.Cartridge.BankController.Name;
		GameTitle = emu.Cartridge.GameTitle;
		Battery = emu.Cartridge.HasBattery;
		RomBanks = emu.Cartridge.RomBankCount;
		RamBanks = emu.Cartridge.RamBankCount;
	}

	public void Update (Emulator emu) { }

	public void Unload ()
	{
		Mbc = "None";
		GameTitle = "None";
		Battery = false;
		RomBanks = 0;
		RamBanks = 0;
	}
}
