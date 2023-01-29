using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Gambit.Core;

public class Cartridge : IComponent, ISerializable
{
	public IMBC BankController { get; }

	public string GameTitle { get; }
	public bool HasBattery { get; }
	public int RomBankCount { get; }
	public int RamBankCount { get; }

	private readonly byte[] romBanks;
	private readonly byte[] ramBanks;

	private bool bootromEnabled = true;

	public Cartridge (Span<byte> romData)
	{
		GameTitle = Encoding.ASCII.GetString(romData[0x0134..0x0143]).Replace("\0", "");

		BankController = romData[0x0147] switch
		{
			0x01 or 0x02 or 0x03 => new MBC1(),
			0x0F or 0x10 or 0x11 or 0x12 or 0x13 => new MBC3(),
			0x00 or _ => new NoMBC(),
		};

		HasBattery = romData[0x0147] switch
		{
			0x03 or 0x06 or 0x09 or 0x0F or 0x0F or 0x10 or 0x13 or 0x1B or 0x1E or 0x22 or 0xFF => true,
			_ => false,
		};

		RomBankCount = romData[0x0148] switch
		{
			0x01 => 4,
			0x02 => 8,
			0x03 => 16,
			0x04 => 32,
			0x05 => 64,
			0x06 => 128,
			0x07 => 256,
			0x08 => 512,
			0x00 or _ => 2,
		};
		romBanks = new byte[RomBankCount * 0x4000];
		romData.CopyTo(romBanks);

		RamBankCount = romData[0x0149] switch
		{
			0x02 => 1,
			0x03 => 4,
			0x04 => 16,
			0x05 => 8,
			0x00 or _ => 0,
		};
		ramBanks = new byte[RamBankCount * 0x2000];
	}

	public async Task LoadRamFromStream (Stream stream) => await stream.ReadAsync(ramBanks);
	public async Task WriteRamToStream (Stream stream) => await stream.WriteAsync(ramBanks);

	public void RegisterHandlers (AddressBus bus)
	{
		bus.AttachReader(0x0000..0x0100, i => bootromEnabled ? BIOS.Fetch(i) : BankController.ReadByte(i, romBanks, ramBanks));
		bus.AttachReader(0x0100..0x8000, i => BankController.ReadByte(i, romBanks, ramBanks));

		bus.AttachWriter(0x0000..0x8000, (i, val) => BankController.WriteByte(i, val, romBanks, ramBanks));

		if (RamBankCount > 0)
		{
			bus.AttachReader(0xA000..0xC000, i => BankController.ReadByte(i, romBanks, ramBanks));
			bus.AttachWriter(0xA000..0xC000, (i, val) => BankController.WriteByte(i, val, romBanks, ramBanks));
		}

		bus.AttachWriter(0x0FF50, (_, _) => bootromEnabled = false);
	}

	public void AdvanceCycle (AddressBus bus, EmulatorMode currentMode, bool debug = false) { }

	public void Serialize (ISaveData saveData)
	{
		saveData.CreateScope(nameof(Cartridge));
		saveData.WriteBool(nameof(bootromEnabled), bootromEnabled);

		saveData.WriteSpan(nameof(romBanks), romBanks);
		saveData.WriteSpan(nameof(ramBanks), ramBanks);

		BankController.Serialize(saveData);
	}

	public bool Deserialize (ILoadData loadData)
	{
		try
		{
			loadData.OpenScope(nameof(Cartridge));
			bootromEnabled = loadData.ReadBool(nameof(bootromEnabled));

			loadData.ReadSpan(nameof(romBanks), romBanks);
			loadData.ReadSpan(nameof(ramBanks), ramBanks);

			return BankController.Deserialize(loadData);
		}
		catch
		{
			return false;
		}
	}
}
