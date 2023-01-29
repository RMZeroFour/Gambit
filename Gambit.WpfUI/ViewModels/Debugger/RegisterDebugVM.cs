using Gambit.Core;

namespace Gambit.UI.ViewModels;

public partial class RegisterDebugVM : ObservableObject, IDebugToolVM
{
	public string DisplayName => "Registers";

	[ObservableProperty]
	private byte a, f, b, c, d, e, h, l;
	[ObservableProperty]
	private ushort programCounter, stackPointer;
	[ObservableProperty]
	private bool flagZ, flagN, flagH, flagC;

	public void Load (Emulator emu) { }

	public void Update (Emulator emu)
	{
		A = emu.Processor.A;
		F = emu.Processor.F;
		B = emu.Processor.B;
		C = emu.Processor.C;
		D = emu.Processor.D;
		E = emu.Processor.E;
		H = emu.Processor.H;
		L = emu.Processor.L;
		ProgramCounter = emu.Processor.ProgramCounter;
		StackPointer = emu.Processor.StackPointer;
		FlagZ = emu.Processor.FlagZ;
		FlagN = emu.Processor.FlagN;
		FlagH = emu.Processor.FlagH;
		FlagC = emu.Processor.FlagC;
	}

	public void Unload ()
	{
		A = F = B = C = D = E = H = L = 0;
		ProgramCounter = 0;
		StackPointer = 0;
		FlagZ = FlagN = FlagH = FlagC = false;
	}
}
