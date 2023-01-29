namespace Gambit.Core;

public interface IMBC : ISerializable
{
	string Name { get; }

	byte ReadByte (int addr, byte[] romBanks, byte[] ramBanks);
	void WriteByte (int addr, byte value, byte[] romBanks, byte[] ramBanks);
}