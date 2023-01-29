using System;
using System.Collections.Generic;

namespace Gambit.Core;

public class Emulator
{
	private static readonly Version version = new(0, 0, 1);

	private const int CyclesPerFrame = 69905;
	private int cycleCount = 0;

	public EmulatorMode Mode { get; private set; } = EmulatorMode.Running;

	public Cartridge Cartridge { get; }
	public AddressBus Bus { get; } = new();
	public Processor Processor { get; } = new();
	public VideoSystem VideoSystem { get; } = new();
	public AudioSystem AudioSystem { get; } = new();
	public Serial Serial { get; } = new();
	public Joypad Joypad { get; } = new();
	public Timer Timer { get; } = new();
	public SystemRam SystemRam { get; } = new();
	public DmaHandler DmaHandler { get; } = new();

	public HashSet<ushort> Breakpoints { get; } = new();

	public Emulator (Cartridge cart)
	{
		Cartridge = cart;

		Cartridge.RegisterHandlers(Bus);
		Processor.RegisterHandlers(Bus);
		VideoSystem.RegisterHandlers(Bus);
		AudioSystem.RegisterHandlers(Bus);
		Serial.RegisterHandlers(Bus);
		Joypad.RegisterHandlers(Bus);
		Timer.RegisterHandlers(Bus);
		SystemRam.RegisterHandlers(Bus);
		DmaHandler.RegisterHandlers(Bus);

		Bus.BuildHandlerMaps();
	}

	public void NextCycle (bool debug)
	{
		Processor.AdvanceCycle(Bus, Mode, debug);
		VideoSystem.AdvanceCycle(Bus, Mode, debug);
		AudioSystem.AdvanceCycle(Bus, Mode, debug);
		Joypad.AdvanceCycle(Bus, Mode, debug);
		Timer.AdvanceCycle(Bus, Mode, debug);
		DmaHandler.AdvanceCycle(Bus, Mode, debug);

		Mode = Processor.NextMode;

		++cycleCount;
	}

	public bool RunToSyncPoint (bool debug)
	{
		do
		{
			NextCycle(debug);

			if (debug && Breakpoints.Contains(Processor.ProgramCounter))
				return true;
		}
		while (cycleCount < CyclesPerFrame);

		cycleCount = 0;

		return false;
	}

	public void SaveState (ISaveData saveData)
	{
		saveData.WriteString(nameof(version), version.ToString());

		Cartridge.Serialize(saveData);
		Processor.Serialize(saveData);
		VideoSystem.Serialize(saveData);
		AudioSystem.Serialize(saveData);
		Serial.Serialize(saveData);
		Joypad.Serialize(saveData);
		Timer.Serialize(saveData);
		SystemRam.Serialize(saveData);
		DmaHandler.Serialize(saveData);
	}

	public bool LoadState (ILoadData loadData)
	{
		var saveVer = loadData.ReadString(nameof(version));
		if (Version.Parse(saveVer) < version)
			return false;

		bool okay = true;

		okay &= Cartridge.Deserialize(loadData);
		okay &= Processor.Deserialize(loadData);
		okay &= VideoSystem.Deserialize(loadData);
		okay &= AudioSystem.Deserialize(loadData);
		okay &= Serial.Deserialize(loadData);
		okay &= Joypad.Deserialize(loadData);
		okay &= SystemRam.Deserialize(loadData);
		okay &= Timer.Deserialize(loadData);
		okay &= DmaHandler.Deserialize(loadData);

		return okay;
	}
}