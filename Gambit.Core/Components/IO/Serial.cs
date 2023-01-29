using System;

namespace Gambit.Core;

public class Serial : IComponent
{
	private byte serialData;
	private byte serialControl;

	public event Action<char> SerialWrite;

	public void RegisterHandlers (AddressBus bus)
	{
		bus.AttachReader(0xFF01, (_) => serialData);
		bus.AttachWriter(0xFF01, (_, val) => serialData = val);

		bus.AttachReader(0xFF02, (_) => serialControl);
		bus.AttachWriter(0xFF02, (_, val) =>
		{
			serialControl = val;
			if (serialControl == 0x81)
			{
				SerialWrite?.Invoke(Convert.ToChar(serialData));
				bus[0xFF0F] = bus[0xFF0F].SetBit(3);
			}
		});
	}

	public void AdvanceCycle (AddressBus bus, EmulatorMode currentMode, bool debug = false) { }

	public void Serialize (ISaveData saveData)
	{
		saveData.CreateScope(nameof(Serial));

		saveData.WriteByte(nameof(serialData), serialData);
		saveData.WriteByte(nameof(serialControl), serialControl);
	}

	public bool Deserialize (ILoadData loadData)
	{
		try
		{
			loadData.OpenScope(nameof(Serial));

			serialData = loadData.ReadByte(nameof(serialData));
			serialControl = loadData.ReadByte(nameof(serialControl));

			return true;
		}
		catch
		{
			return false;
		}
	}
}