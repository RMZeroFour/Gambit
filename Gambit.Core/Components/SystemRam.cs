namespace Gambit.Core;

public class SystemRam : IComponent, ISerializable
{
	private readonly byte[] workRam = new byte[0x2000];
	private readonly byte[] highRam = new byte[0x80];

	public void RegisterHandlers (AddressBus bus)
	{
		bus.AttachReader(0xC000..0xE000, i => workRam[i - 0xC000]);
		bus.AttachWriter(0xC000..0xE000, (i, val) => workRam[i - 0xC000] = val);

		bus.AttachReader(0xE000..0xFE00, i => workRam[i - 0xE000]);
		bus.AttachWriter(0xE000..0xFE00, (i, val) => workRam[i - 0xE000] = val);

		bus.AttachReader(0xFF80..0xFFFF, i => highRam[i - 0xFF80]);
		bus.AttachWriter(0xFF80..0xFFFF, (i, val) => highRam[i - 0xFF80] = val);
	}

	public void AdvanceCycle (AddressBus bus, EmulatorMode currentMode, bool debug = false) { }

	public void Serialize (ISaveData saveData)
	{
		saveData.CreateScope(nameof(SystemRam));
		saveData.WriteSpan(nameof(workRam), workRam);
		saveData.WriteSpan(nameof(highRam), highRam);
	}

	public bool Deserialize (ILoadData loadData)
	{
		try
		{
			loadData.OpenScope(nameof(SystemRam));

			loadData.ReadSpan(nameof(workRam), workRam);
			loadData.ReadSpan(nameof(highRam), highRam);

			return true;
		}
		catch
		{
			return false;
		}
	}
}