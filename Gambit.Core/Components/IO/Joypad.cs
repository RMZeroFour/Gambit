namespace Gambit.Core;

public class Joypad : IComponent
{
	// All the joypad bits are active low.

	public bool Start { get; set; }
	public bool Select { get; set; }
	public bool B { get; set; }
	public bool A { get; set; }

	public bool Down { get; set; }
	public bool Up { get; set; }
	public bool Left { get; set; }
	public bool Right { get; set; }

	private bool dirnButtons;

	private byte lastValue, currentValue;

	private byte GetValue ()
	{
		int value = 0b_1100_0000;

		if (dirnButtons)
		{
			value |= 0x10;
			value |= (Down ? 0x00 : 0x08);
			value |= (Up ? 0x00 : 0x04);
			value |= (Left ? 0x00 : 0x02);
			value |= (Right ? 0x00 : 0x01);
		}
		else
		{
			value |= 0x20;
			value |= (Start ? 0x00 : 0x08);
			value |= (Select ? 0x00 : 0x04);
			value |= (B ? 0x00 : 0x02);
			value |= (A ? 0x00 : 0x01);
		}

		return (byte)value;
	}

	public void RegisterHandlers (AddressBus bus)
	{
		bus.AttachReader(0xFF00, (_) => currentValue);
		bus.AttachWriter(0xFF00, (_, val) =>
		{
			if (val == 0x10)
				dirnButtons = false;
			else if (val == 0x20)
				dirnButtons = true;

			lastValue = currentValue;
			currentValue = GetValue();
		});
	}

	public void AdvanceCycle (AddressBus bus, EmulatorMode currentMode, bool debug = false)
	{
		currentValue = GetValue();

		if (((lastValue & 0x0F) & ~(currentValue & 0x0F)) != 0)
			bus[0xFF0F] = bus[0xFF0F].SetBit(4);

		lastValue = currentValue;
	}

	public void Serialize (ISaveData saveData)
	{
		saveData.CreateScope(nameof(Joypad));

		saveData.WriteByte(nameof(lastValue), lastValue);
		saveData.WriteByte(nameof(currentValue), currentValue);

		saveData.WriteBool(nameof(dirnButtons), dirnButtons);

		saveData.WriteBool(nameof(Start), Start);
		saveData.WriteBool(nameof(Select), Select);
		saveData.WriteBool(nameof(B), B);
		saveData.WriteBool(nameof(A), A);

		saveData.WriteBool(nameof(Down), Down);
		saveData.WriteBool(nameof(Up), Up);
		saveData.WriteBool(nameof(Left), Left);
		saveData.WriteBool(nameof(Right), Right);
	}

	public bool Deserialize (ILoadData loadData)
	{
		try
		{
			loadData.OpenScope(nameof(Joypad));

			lastValue = loadData.ReadByte(nameof(lastValue));
			currentValue = loadData.ReadByte(nameof(currentValue));

			dirnButtons = loadData.ReadBool(nameof(dirnButtons));

			Start = loadData.ReadBool(nameof(Start));
			Select = loadData.ReadBool(nameof(Select));
			B = loadData.ReadBool(nameof(B));
			A = loadData.ReadBool(nameof(A));

			Down = loadData.ReadBool(nameof(Down));
			Up = loadData.ReadBool(nameof(Up));
			Left = loadData.ReadBool(nameof(Left));
			Right = loadData.ReadBool(nameof(Right));

			return true;
		}
		catch
		{
			return false;
		}
	}
}