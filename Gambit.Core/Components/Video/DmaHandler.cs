namespace Gambit.Core;

public class DmaHandler : IComponent, ISerializable
{
	private ushort startAddress;
	private bool transferStarted;
	private byte currentCycles;

	public void RegisterHandlers (AddressBus bus)
	{
		bus.AttachReader(0xFF46, _ => (byte)(startAddress >> 8));

		bus.AttachWriter(0xFF46, (i, val) =>
		{
			startAddress = (ushort)(val << 8);
			transferStarted = true;
			currentCycles = 0;
		});
	}

	public void AdvanceCycle (AddressBus bus, EmulatorMode currentMode, bool debug = false)
	{
		if (transferStarted)
		{
			var sourceAddress = startAddress | currentCycles;
			var destinationAddress = 0xFE00 | currentCycles;
			bus[destinationAddress] = bus[sourceAddress];

			if (++currentCycles == 0xA0)
				transferStarted = false;
		}
	}

	public void Serialize (ISaveData saveData)
	{
		saveData.CreateScope(nameof(DmaHandler));

		saveData.WriteWord(nameof(startAddress), startAddress);
		saveData.WriteBool(nameof(transferStarted), transferStarted);
		saveData.WriteByte(nameof(currentCycles), currentCycles);
	}

	public bool Deserialize (ILoadData loadData)
	{
		try
		{
			loadData.OpenScope(nameof(DmaHandler));

			startAddress = loadData.ReadWord(nameof(startAddress));
			transferStarted = loadData.ReadBool(nameof(transferStarted));
			currentCycles = loadData.ReadByte(nameof(currentCycles));

			return true;
		}
		catch
		{
			return false;
		}
	}
}