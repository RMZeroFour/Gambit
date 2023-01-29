using System;

namespace Gambit.Core;

public class MBC3 : IMBC
{
	public string Name => "MBC 3";

	private byte highRomBank = 1;
	private bool ramAndTimerEnabled = false;
	private byte ramBankOrRtcRegister;
	private byte latchClockData;

	private RealTimeClock rtc;

	public MBC3 ()
	{
		DateTime now = DateTime.Now;
		rtc = new((byte)now.Second, (byte)now.Minute, (byte)now.Hour, 0, 0);
	}

	public byte ReadByte (int addr, byte[] romBanks, byte[] ramBanks)
	{
		return addr switch
		{
			>= 0x0000 and < 0x4000 => romBanks[0x0000 + addr],
			>= 0x4000 and < 0x8000 => romBanks[highRomBank * 0x4000 + (addr - 0x4000)],
			>= 0xA000 and < 0xC000 when ramAndTimerEnabled =>
				ramBankOrRtcRegister switch
				{
					>= 0x0 and < 0x4 => ramBanks[ramBankOrRtcRegister * 0x2000 + (addr - 0xA000)],
					0x8 => rtc.Seconds,
					0x9 => rtc.Minutes,
					0xA => rtc.Hours,
					0xB => rtc.DaysLower,
					0xC => rtc.DaysHigher,
					_ => 0x00
				},
			_ => 0x00,
		};
	}

	public void WriteByte (int addr, byte value, byte[] romBanks, byte[] ramBanks)
	{
		switch (addr)
		{
			case >= 0x0000 and < 0x2000:
				ramAndTimerEnabled = (value & 0b1111) == 0b1010;
				return;

			case >= 0x2000 and < 0x4000:
				highRomBank = (byte)(value & 0x7F);
				if (highRomBank == 0)
					++highRomBank;
				return;

			case >= 0x4000 and < 0x6000:
				ramBankOrRtcRegister = (byte)(value & 0x0F);
				return;

			case >= 0x6000 and < 0x8000:
				if (latchClockData == 0x00 && value == 0x01)
				{
					DateTime now = DateTime.Now;
					rtc = rtc with { Seconds = (byte)now.Second, Minutes = (byte)now.Minute, Hours = (byte)now.Hour };
				}
				latchClockData = value;
				return;

			case >= 0xA000 and < 0xC000:
				if (ramAndTimerEnabled)
				{
					switch (ramBankOrRtcRegister)
					{
						case >= 0x0 and < 0x4: ramBanks[ramBankOrRtcRegister * 0x2000 + (addr - 0xA000)] = value; break;
						case 0x8: rtc = rtc with { Seconds = value }; break;
						case 0x9: rtc = rtc with { Minutes = value }; break;
						case 0xA: rtc = rtc with { Hours = value }; break;
						case 0xB: rtc = rtc with { DaysLower = value }; break;
						case 0xC: rtc = rtc with { DaysHigher = value }; break;
					};
				}
				return;
		}
	}

	public void Serialize (ISaveData saveData)
	{
		saveData.WriteBool(nameof(ramAndTimerEnabled), ramAndTimerEnabled);
		saveData.WriteByte(nameof(highRomBank), highRomBank);
		saveData.WriteByte(nameof(ramBankOrRtcRegister), ramBankOrRtcRegister);
		saveData.WriteByte(nameof(latchClockData), latchClockData);

		saveData.WriteByte(nameof(rtc.Seconds), rtc.Seconds);
		saveData.WriteByte(nameof(rtc.Minutes), rtc.Minutes);
		saveData.WriteByte(nameof(rtc.Hours), rtc.Hours);
		saveData.WriteByte(nameof(rtc.DaysLower), rtc.DaysLower);
		saveData.WriteByte(nameof(rtc.DaysHigher), rtc.DaysHigher);
	}

	public bool Deserialize (ILoadData loadData)
	{
		try
		{
			ramAndTimerEnabled = loadData.ReadBool(nameof(ramAndTimerEnabled));
			highRomBank = loadData.ReadByte(nameof(highRomBank));
			ramBankOrRtcRegister = loadData.ReadByte(nameof(ramBankOrRtcRegister));
			latchClockData = loadData.ReadByte(nameof(latchClockData));

			byte seconds = loadData.ReadByte(nameof(rtc.Seconds));
			byte minutes = loadData.ReadByte(nameof(rtc.Minutes));
			byte hours = loadData.ReadByte(nameof(rtc.Hours));
			byte daysLower = loadData.ReadByte(nameof(rtc.DaysLower));
			byte daysHigher = loadData.ReadByte(nameof(rtc.DaysHigher));
			rtc = new RealTimeClock(seconds, minutes, hours, daysLower, daysHigher);

			return true;
		}
		catch
		{
			return false;
		}
	}

	private record RealTimeClock (byte Seconds, byte Minutes, byte Hours, byte DaysLower, byte DaysHigher);
}