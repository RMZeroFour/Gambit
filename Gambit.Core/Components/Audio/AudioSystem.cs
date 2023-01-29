namespace Gambit.Core;

public class AudioSystem : IComponent, ISerializable
{
	private byte[] tempA = new byte[0x17];
	private byte[] tempB = new byte[0x10];

	public void RegisterHandlers (AddressBus bus)
	{
		bus.AttachReader(0xFF10..0xFF27, i => tempA[i - 0xFF10]);
		bus.AttachWriter(0xFF10..0xFF27, (i, val) => tempA[i - 0xFF10] = val);

		bus.AttachReader(0xFF30..0xFF40, i => tempB[i - 0xFF30]);
		bus.AttachWriter(0xFF30..0xFF40, (i, val) => tempB[i - 0xFF30] = val);
	}

	public void AdvanceCycle (AddressBus bus, EmulatorMode currentMode, bool debug = false) { }

	public void Serialize (ISaveData saveData)
	{
		saveData.CreateScope(nameof(AudioSystem));
	}

	public bool Deserialize (ILoadData loadData)
	{
		try
		{
			loadData.OpenScope(nameof(AudioSystem));

			return true;
		}
		catch
		{
			return false;
		}
	}
}