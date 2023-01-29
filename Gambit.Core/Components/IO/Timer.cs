namespace Gambit.Core;

public class Timer : IComponent, ISerializable
{
	private byte divider;
	private byte internalDivider;

	private byte timerCounter;
	private byte timerModulo;
	private byte timerControl;
	private int internalCounter;
	private int currentThreshold;

	public void RegisterHandlers (AddressBus bus)
	{
		bus.AttachReader(0xFF04..0xFF08, i => i switch
		{
			0xFF04 => divider,
			0xFF05 => timerCounter,
			0xFF06 => timerModulo,
			0xFF07 => timerControl,
			_ => 0x00
		});

		bus.AttachWriter(0xFF04..0xFF08, (i, val) =>
		{
			switch (i)
			{
				case 0xFF04: divider = 0; return;
				case 0xFF05: timerCounter = val; return;
				case 0xFF06: timerModulo = val; return;
				case 0xFF07:
					if ((timerControl & 0b11) != (val & 0b11))
					{
						timerControl = val;
						currentThreshold = (timerControl & 0b11) switch
						{
							0b01 => 4,
							0b10 => 16,
							0b11 => 64,
							0b00 or _ => 256,
						};
					}
					return;
			}
		});
	}

	public void AdvanceCycle (AddressBus bus, EmulatorMode currentMode, bool debug = false)
	{
		HandleDivider(currentMode);
		HandleTimer(bus);
	}

	private void HandleDivider (EmulatorMode currentMode)
	{
		if (currentMode == EmulatorMode.Stopped)
		{
			divider = 0;
		}
		else
		{
			++internalDivider;

			while (internalDivider >= 64)
			{
				internalDivider -= 64;
				++divider;
			}
		}
	}

	private void HandleTimer (AddressBus bus)
	{
		if (timerControl.TestBit(2)) // TIMA Enable
		{
			++internalCounter;

			while (internalCounter >= currentThreshold)
			{
				internalCounter -= currentThreshold;
				if (++timerCounter == 0)
				{
					timerCounter = timerModulo;
					bus[0xFF0F] = bus[0xFF0F].SetBit(2);
				}
			}
		}
	}

	public void Serialize (ISaveData saveData)
	{
		saveData.CreateScope(nameof(Timer));

		saveData.WriteByte(nameof(divider), divider);
		saveData.WriteByte(nameof(internalDivider), internalDivider);

		saveData.WriteByte(nameof(timerCounter), timerCounter);
		saveData.WriteByte(nameof(timerModulo), timerModulo);
		saveData.WriteByte(nameof(timerControl), timerControl);
		saveData.WriteInt(nameof(internalCounter), internalCounter);
		saveData.WriteInt(nameof(currentThreshold), currentThreshold);
	}

	public bool Deserialize (ILoadData loadData)
	{
		try
		{
			loadData.OpenScope(nameof(Timer));

			divider = loadData.ReadByte(nameof(divider));
			internalDivider = loadData.ReadByte(nameof(internalDivider));

			timerCounter = loadData.ReadByte(nameof(timerCounter));
			timerModulo = loadData.ReadByte(nameof(timerModulo));
			timerControl = loadData.ReadByte(nameof(timerControl));
			internalCounter = loadData.ReadInt(nameof(internalCounter));
			currentThreshold = loadData.ReadInt(nameof(currentThreshold));

			return true;
		}
		catch
		{
			return false;
		}
	}
}