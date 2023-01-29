namespace Gambit.Core;

public partial class Processor
{
	public byte FetchNext ()
	{
		if (haltBug)
		{
			haltBug = false;
			return bus[ProgramCounter];
		}
		return bus[ProgramCounter++];
	}

	private byte ReadR8 (int reg)
	{
		return (reg) switch
		{
			0b000 => B,
			0b001 => C,
			0b010 => D,
			0b011 => E,
			0b100 => H,
			0b101 => L,
			0b110 => bus[HL],
			0b111 => A,
			_ => 0x00
		};
	}

	private void WriteR8 (int reg, byte value)
	{
		switch (reg)
		{
			case 0b000: B = value; return;
			case 0b001: C = value; return;
			case 0b010: D = value; return;
			case 0b011: E = value; return;
			case 0b100: H = value; return;
			case 0b101: L = value; return;
			case 0b110: bus[HL] = value; return;
			case 0b111: A = value; return;
		};
	}

	private ushort ReadR16GroupA (int reg)
	{
		return (reg) switch
		{
			0b00 => BC,
			0b01 => DE,
			0b10 => HL,
			0b11 => StackPointer,
			_ => 0x0000
		};
	}

	private byte ReadR16GroupB (int reg)
	{
		return (reg) switch
		{
			0b00 => bus[BC],
			0b01 => bus[DE],
			0b10 => bus[HL++],
			0b11 => bus[HL--],
			_ => 0x0000
		};
	}

	private ushort ReadR16GroupC (int reg)
	{
		return (reg) switch
		{
			0b00 => BC,
			0b01 => DE,
			0b10 => HL,
			0b11 => AF,
			_ => 0x0000
		};
	}

	private void WriteR16GroupA (int reg, ushort value)
	{
		switch (reg)
		{
			case 0b00: BC = value; return;
			case 0b01: DE = value; return;
			case 0b10: HL = value; return;
			case 0b11: StackPointer = value; return;
		};
	}

	private void WriteR16GroupB (int reg, byte value)
	{
		switch (reg)
		{
			case 0b00: bus[BC] = value; return;
			case 0b01: bus[DE] = value; return;
			case 0b10: bus[HL++] = value; return;
			case 0b11: bus[HL--] = value; return;
		};
	}

	private void WriteR16GroupC (int reg, ushort value)
	{
		switch (reg)
		{
			case 0b00: BC = value; return;
			case 0b01: DE = value; return;
			case 0b10: HL = value; return;
			case 0b11: AF = value; return;
		};
	}

	private bool CheckCondition (int type)
	{
		return type switch
		{
			0b00 => !FlagZ,
			0b01 => FlagZ,
			0b10 => !FlagC,
			0b11 => FlagC,
			_ => false,
		};
	}
}