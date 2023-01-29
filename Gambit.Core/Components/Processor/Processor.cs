namespace Gambit.Core;

public partial class Processor : IComponent, ISerializable
{
	public EmulatorMode NextMode { get; private set; }

	public byte A { get; private set; }
	public byte F
	{
		get => (byte)(FlagZ.AsBit() << 7 | FlagN.AsBit() << 6 | FlagH.AsBit() << 5 | FlagC.AsBit() << 4);
		private set { FlagZ = value.TestBit(7); FlagN = value.TestBit(6); FlagH = value.TestBit(5); FlagC = value.TestBit(4); }
	}
	public byte B { get; private set; }
	public byte C { get; private set; }
	public byte D { get; private set; }
	public byte E { get; private set; }
	public byte H { get; private set; }
	public byte L { get; private set; }

	public ushort AF { get => (ushort)(A << 8 | F); private set { A = (byte)(value >> 8); F = (byte)(value & 0xFF); } }
	public ushort BC { get => (ushort)(B << 8 | C); private set { B = (byte)(value >> 8); C = (byte)(value & 0xFF); } }
	public ushort DE { get => (ushort)(D << 8 | E); private set { D = (byte)(value >> 8); E = (byte)(value & 0xFF); } }
	public ushort HL { get => (ushort)(H << 8 | L); private set { H = (byte)(value >> 8); L = (byte)(value & 0xFF); } }

	public bool FlagZ { get; private set; }
	public bool FlagN { get; private set; }
	public bool FlagH { get; private set; }
	public bool FlagC { get; private set; }

	public ushort ProgramCounter { get; private set; } = 0x0000;
	public ushort StackPointer { get; private set; }

	private bool interruptMasterEnable;
	private byte interruptEnable;
	private byte interruptFlags;
	private InterruptType currentInterrupt = InterruptType.None;
	private byte currentInterruptCycles;

	private readonly ByteStack workingStack = new(4);
	private byte currentCycles;
	private ushort currentOpcode;
	private bool opcodeDone = true;

	private bool haltBug;

	private AddressBus bus;

	public void RegisterHandlers (AddressBus bus)
	{
		this.bus ??= bus;

		bus.AttachReader(0xFF0F, _ => interruptFlags);
		bus.AttachWriter(0xFF0F, (_, val) => interruptFlags = val);

		bus.AttachReader(0xFFFF, _ => interruptEnable);
		bus.AttachWriter(0xFFFF, (_, val) => interruptEnable = val);
	}

	public void AdvanceCycle (AddressBus bus, EmulatorMode currentMode, bool debug = false)
	{
		NextMode = currentMode;
		switch (currentMode)
		{
			case EmulatorMode.Running:
				if (currentInterrupt != InterruptType.None)
					ExecuteInterruptCycle();
				else
					ExecuteOpcodeCycle();

				if (opcodeDone && (interruptMasterEnable && (interruptEnable & interruptFlags) != 0))
				{
					AcknowledgeInterrupt();
					interruptMasterEnable = false;
				}
				return;

			case EmulatorMode.Halted:
			case EmulatorMode.Stopped:
				if ((interruptEnable & interruptFlags) != 0)
				{
					if (interruptMasterEnable)
					{
						AcknowledgeInterrupt();
						interruptMasterEnable = false;

						ExecuteInterruptCycle();
					}

					NextMode = EmulatorMode.Running;
				}
				return;
		}
	}

	public void Serialize (ISaveData saveData)
	{
		saveData.CreateScope(nameof(Processor));

		saveData.WriteWord(nameof(ProgramCounter), ProgramCounter);
		saveData.WriteWord(nameof(StackPointer), StackPointer);

		saveData.WriteByte(nameof(A), A); saveData.WriteByte(nameof(F), F);
		saveData.WriteByte(nameof(B), B); saveData.WriteByte(nameof(C), C);
		saveData.WriteByte(nameof(D), D); saveData.WriteByte(nameof(E), E);
		saveData.WriteByte(nameof(H), H); saveData.WriteByte(nameof(L), L);

		saveData.WriteBool(nameof(interruptMasterEnable), interruptMasterEnable);
		saveData.WriteByte(nameof(interruptEnable), interruptEnable);
		saveData.WriteByte(nameof(interruptFlags), interruptFlags);
		saveData.WriteByte(nameof(currentInterrupt), (byte)currentInterrupt);
		saveData.WriteByte(nameof(currentInterruptCycles), currentInterruptCycles);

		saveData.WriteByte(nameof(currentCycles), currentCycles);
		saveData.WriteWord(nameof(currentOpcode), currentOpcode);
		saveData.WriteBool(nameof(opcodeDone), opcodeDone);
		saveData.WriteSpan(nameof(workingStack), workingStack.Buffer);

		saveData.WriteBool(nameof(haltBug), haltBug);
	}

	public bool Deserialize (ILoadData loadData)
	{
		try
		{
			loadData.OpenScope(nameof(Processor));

			ProgramCounter = loadData.ReadWord(nameof(ProgramCounter));
			StackPointer = loadData.ReadWord(nameof(StackPointer));

			A = loadData.ReadByte(nameof(A)); F = loadData.ReadByte(nameof(F));
			B = loadData.ReadByte(nameof(B)); C = loadData.ReadByte(nameof(C));
			D = loadData.ReadByte(nameof(D)); E = loadData.ReadByte(nameof(E));
			H = loadData.ReadByte(nameof(H)); L = loadData.ReadByte(nameof(L));

			interruptMasterEnable = loadData.ReadBool(nameof(interruptMasterEnable));
			interruptEnable = loadData.ReadByte(nameof(interruptEnable));
			interruptFlags = loadData.ReadByte(nameof(interruptFlags));
			currentInterrupt = (InterruptType)loadData.ReadByte(nameof(currentInterrupt));
			currentInterruptCycles = loadData.ReadByte(nameof(currentInterruptCycles));

			currentCycles = loadData.ReadByte(nameof(currentCycles));
			currentOpcode = loadData.ReadWord(nameof(currentOpcode));
			opcodeDone = loadData.ReadBool(nameof(opcodeDone));
			loadData.ReadSpan(nameof(workingStack), workingStack.Buffer);

			haltBug = loadData.ReadBool(nameof(haltBug));

			return true;
		}
		catch
		{
			return false;
		}
	}
}