namespace Gambit.Core;

public partial class Processor
{
	public void AcknowledgeInterrupt ()
	{
		byte valid = (byte)(interruptEnable & interruptFlags);

		if (valid.TestBit(0))
		{
			currentInterrupt = InterruptType.VBlank;
			interruptFlags = interruptFlags.UnsetBit(0);
		}
		else if (valid.TestBit(1))
		{
			currentInterrupt = InterruptType.LCDStatus;
			interruptFlags = interruptFlags.UnsetBit(1);
		}
		else if (valid.TestBit(2))
		{
			currentInterrupt = InterruptType.Timer;
			interruptFlags = interruptFlags.UnsetBit(2);
		}
		else if (valid.TestBit(3))
		{
			currentInterrupt = InterruptType.Serial;
			interruptFlags = interruptFlags.UnsetBit(3);
		}
		else if (valid.TestBit(4))
		{
			currentInterrupt = InterruptType.Joypad;
			interruptFlags = interruptFlags.UnsetBit(4);
		}

		currentInterruptCycles = 0;
	}

	public void ExecuteInterruptCycle ()
	{
		switch (++currentInterruptCycles)
		{
			case 1 or 2:
				// Do nothing...
				return;

			case 3:
				bus[--StackPointer] = (byte)(ProgramCounter >> 8);
				return;

			case 4:
				bus[--StackPointer] = (byte)(ProgramCounter & 0xFF);
				return;

			case 5:
				ProgramCounter = currentInterrupt switch
				{
					InterruptType.VBlank => 0x40,
					InterruptType.LCDStatus => 0x48,
					InterruptType.Timer => 0x50,
					InterruptType.Serial => 0x58,
					InterruptType.Joypad => 0x60,

					_ or InterruptType.None => 0x00,
				};
				currentInterrupt = InterruptType.None;
				return;
		}
	}

	private enum InterruptType : byte
	{
		None = 0,
		VBlank,
		LCDStatus,
		Timer,
		Serial,
		Joypad
	}
}