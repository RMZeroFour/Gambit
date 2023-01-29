namespace Gambit.Core;

public class MBC1 : IMBC
{
	public string Name => "MBC 1";

	private bool ramEnabled = false;
	private byte bankingMode = 0;
	private byte lowerRegister = 0;
	private byte upperRegister = 0;

	private int lowRomBank;
	private int highRomBank = 1;
	private int ramBank;

	private void CalculateAddresses ()
	{
		lowRomBank = (bankingMode == 0) ? 0 : upperRegister << 5;
		highRomBank = (upperRegister << 5) | lowerRegister;
		ramBank = (bankingMode == 0) ? 0 : upperRegister;
	}

	public byte ReadByte (int addr, byte[] romBanks, byte[] ramBanks)
	{
		return addr switch
		{
			>= 0x0000 and < 0x4000 => romBanks[lowRomBank * 0x4000 + addr],
			>= 0x4000 and < 0x8000 => romBanks[highRomBank * 0x4000 + (addr - 0x4000)],
			>= 0xA000 and < 0xC000 when ramEnabled => ramBanks[ramBank * 0x2000 + (addr - 0xA000)],
			_ => 0x00,
		};
	}

	public void WriteByte (int addr, byte value, byte[] romBanks, byte[] ramBanks)
	{
		switch (addr)
		{
			case >= 0x0000 and < 0x2000:
				ramEnabled = (value & 0b1111) == 0b1010;
				return;

			case >= 0x2000 and < 0x4000:
				lowerRegister = (byte)(value & 0b1_1111);
				if (lowerRegister == 0)
					++lowerRegister;
				CalculateAddresses();
				return;

			case >= 0x4000 and < 0x6000:
				upperRegister = (byte)(value & 0b11);
				CalculateAddresses();
				return;

			case >= 0x6000 and < 0x8000:
				bankingMode = value.GetBit(0);
				CalculateAddresses();
				return;

			case >= 0xA000 and < 0xC000:
				if (ramEnabled)
					ramBanks[ramBank * 0x2000 + (addr - 0xA000)] = value;
				return;
		}
	}

	public void Serialize (ISaveData saveData)
	{
		saveData.WriteBool(nameof(ramEnabled), ramEnabled);
		saveData.WriteByte(nameof(bankingMode), bankingMode);
		saveData.WriteByte(nameof(lowerRegister), lowerRegister);
		saveData.WriteByte(nameof(upperRegister), upperRegister);
	}

	public bool Deserialize (ILoadData loadData)
	{
		try
		{
			ramEnabled = loadData.ReadBool(nameof(ramEnabled));
			bankingMode = loadData.ReadByte(nameof(bankingMode));
			lowerRegister = loadData.ReadByte(nameof(lowerRegister));
			upperRegister = loadData.ReadByte(nameof(upperRegister));

			CalculateAddresses();

			return true;
		}
		catch
		{
			return false;
		}
	}
}