namespace Gambit.Core;

public class NoMBC : IMBC
{
	public string Name => "No MBC";

	public byte ReadByte (int addr, byte[] romBanks, byte[] ramBanks)
	{
		if (addr <= 0x8000)
			return romBanks[addr];
		return 0x00;
	}

	public void WriteByte (int addr, byte value, byte[] romBanks, byte[] ramBanks)
	{
		// Illegal Write
	}

	public void Serialize (ISaveData saveData) { }
	public bool Deserialize (ILoadData loadData) => true;
}