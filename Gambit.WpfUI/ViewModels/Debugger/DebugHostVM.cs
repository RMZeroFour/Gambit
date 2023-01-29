using System.Collections.Generic;
using System.Linq;
using Gambit.Core;

namespace Gambit.UI.ViewModels;

public partial class DebugHostVM : ObservableObject, IUpdatableVM
{
	public IDebugToolVM[] DebugTools { get; }

	[ObservableProperty]
	private IDebugToolVM current;

	public DebugHostVM (IEnumerable<IDebugToolVM> debugVMs)
	{
		DebugTools = debugVMs.ToArray();
		Current = null;
	}

	public void Load (Emulator emu)
	{
		foreach (var tool in DebugTools)
			tool.Load(emu);
	}

	public void Update (Emulator emu) => Current?.Update(emu);

	public void Unload ()
	{
		foreach (var tool in DebugTools)
			tool.Unload();
	}
}