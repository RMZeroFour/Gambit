using System.Collections.Generic;
using Gambit.Core;

namespace Gambit.UI.ViewModels;

public partial class DisassemblyDebugVM : ObservableObject, IDebugToolVM
{
	public string DisplayName => "Disassembly";

	private const int Count = 8;
	public List<DisassemblyVM> Disassemblies { get; } = new();

	public DisassemblyDebugVM ()
	{
		for (int i = 0; i < Count; i++)
			Disassemblies.Add(new());
	}

	public void Load (Emulator emu)
	{
		Update(emu);
	}

	public void Update (Emulator emu)
	{
		int index = 0, offset = 0;
		byte Fetcher () => emu.Bus[index++];

		while (index < emu.Processor.ProgramCounter)
		{
			offset += Disassembler.Disassemble(Fetcher).Length;
		}

		for (int i = 0; i < Count; i++)
		{
			var (text, len) = Disassembler.Disassemble(Fetcher);
			Disassemblies[i].Offset = offset;
			Disassemblies[i].Text = text;
			offset += len;
		}
	}

	public void Unload () { }

	public partial class DisassemblyVM : ObservableObject
	{
		[ObservableProperty]
		private int offset;
		[ObservableProperty]
		private string text;
	}
}
