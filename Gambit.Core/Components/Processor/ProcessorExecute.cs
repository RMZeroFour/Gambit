using System;

namespace Gambit.Core;

public partial class Processor
{
	public void ExecuteOpcodeCycle ()
	{
		if (opcodeDone)
		{
			opcodeDone = false;
			currentCycles = 0;
			currentOpcode = FetchNext();
		}

		++currentCycles;

		byte main = (byte)(currentOpcode & 0xFF);
		if ((currentOpcode >> 8) != 0xCB)
		{
			switch (main.GetBit(7), main.GetBit(6), main.GetBit(5), main.GetBit(4), main.GetBit(3), main.GetBit(2), main.GetBit(1), main.GetBit(0))
			{
				case (0, 0, 0, 0, 0, 0, 0, 0): /* NOP */ opcodeDone = true; return;

				case (0, 0, 0, 1, 0, 0, 0, 0): /* STOP */ NextMode = EmulatorMode.Stopped; opcodeDone = true; return;
				case (0, 1, 1, 1, 0, 1, 1, 0): HandleHALT(); return;

				case (0, 0, 0, 1, 1, 0, 0, 0): HandleJR(); return;
				case (0, 0, 1, _, _, 0, 0, 0): HandleJR_CC(); return;

				case (0, 0, 0, 0, 1, 0, 0, 0): HandleLD_U16_SP(); return;

				case (0, 0, _, _, 0, 0, 0, 1): HandleLD_R16_U16(); return;

				case (0, 0, _, _, 1, 0, 0, 1): HandleADD_HL_R16(); return;

				case (0, 0, _, _, 0, 0, 1, 0): HandleLD_R16_A(); return;
				case (0, 0, _, _, 1, 0, 1, 0): HandleLD_A_R16(); return;

				case (0, 0, _, _, 0, 0, 1, 1): HandleINC_R16(); return;
				case (0, 0, _, _, 1, 0, 1, 1): HandleDEC_R16(); return;

				case (0, 0, _, _, _, 1, 0, 0): HandleINC_R8(); return;
				case (0, 0, _, _, _, 1, 0, 1): HandleDEC_R8(); return;

				case (0, 0, _, _, _, 1, 1, 0): HandleLD_R8_U8(); return;
				case (0, 1, _, _, _, _, _, _): HandleLD_R8_R8(); return;

				case (0, 0, _, _, _, 1, 1, 1): HandleOpcodeGroupA(); return;

				case (1, 0, _, _, _, _, _, _): HandleALU_R8(); return;
				case (1, 1, _, _, _, 1, 1, 0): HandleALU_U8(); return;

				case (1, 1, 0, 0, 0, 0, 1, 1): HandleJP(); return;
				case (1, 1, 1, 0, 1, 0, 0, 1): HandleJP_HL(); return;
				case (1, 1, 0, _, _, 0, 1, 0): HandleJP_CC(); return;

				case (1, 1, 1, 1, 1, 0, 0, 1): HandleLD_SP_HL(); return;
				case (1, 1, 1, 1, 1, 0, 0, 0): HandleLD_HL_SP_I8(); return;
				case (1, 1, 1, 0, 1, 0, 0, 0): HandleADD_SP_I8(); return;

				case (1, 1, 1, 0, 1, 0, 1, 0): HandleLD_U16_A(); return;
				case (1, 1, 1, 1, 1, 0, 1, 0): HandleLD_A_U16(); return;

				case (1, 1, 1, 0, 0, 0, 0, 0): HandleLD_IO_U8_A(); return;
				case (1, 1, 1, 1, 0, 0, 0, 0): HandleLD_A_IO_U8(); return;
				case (1, 1, 1, 0, 0, 0, 1, 0): HandleLD_IO_C_A(); return;
				case (1, 1, 1, 1, 0, 0, 1, 0): HandleLD_A_IO_C(); return;

				case (1, 1, _, _, 0, 1, 0, 1): HandlePUSH(); return;
				case (1, 1, _, _, 0, 0, 0, 1): HandlePOP(); return;

				case (1, 1, 0, 0, 1, 1, 0, 1): HandleCALL(); return;
				case (1, 1, 0, _, _, 1, 0, 0): HandleCALL_CC(); return;
				case (1, 1, 0, 0, 1, 0, 0, 1): HandleRET(); return;
				case (1, 1, 0, _, _, 0, 0, 0): HandleRET_CC(); return;
				case (1, 1, 0, 1, 1, 0, 0, 1): HandleRETI(); return;

				case (1, 1, 1, 1, 0, 0, 1, 1): /* DI */ interruptMasterEnable = false; opcodeDone = true; return;
				case (1, 1, 1, 1, 1, 0, 1, 1): /* EI */interruptMasterEnable = true; opcodeDone = true; return;

				case (1, 1, _, _, _, 1, 1, 1): HandleRST(); return;

				case (1, 1, 0, 0, 1, 0, 1, 1): /* CB */ currentOpcode = (ushort)(0xCB00 | FetchNext()); return;

				default:
					throw new NotImplementedException($"Invalid Opcode 0x{currentOpcode:X2}!");
			}
		}
		else
		{
			switch (main.GetBit(7), main.GetBit(6))
			{
				case (0, 0): HandleOpcodeGroupC(); return;
				case (0, 1): HandleBIT(); return;
				case (1, 0): HandleRES(); return;
				case (1, 1): HandleSET(); return;
			}
		}

		//switch (ext, main.GetBit(7), main.GetBit(6), main.GetBit(5), main.GetBit(4), main.GetBit(3), main.GetBit(2), main.GetBit(1), main.GetBit(0))
		//{
		//	case (false, 0, 0, 0, 0, 0, 0, 0, 0): /* NOP */ opcodeDone = true; return;

		//	case (false, 0, 0, 0, 1, 0, 0, 0, 0): /* STOP */ NextMode = EmulatorMode.Stopped; opcodeDone = true; return;
		//	case (false, 0, 1, 1, 1, 0, 1, 1, 0): HandleHALT(); return;

		//	case (false, 0, 0, 0, 1, 1, 0, 0, 0): HandleJR(); return;
		//	case (false, 0, 0, 1, _, _, 0, 0, 0): HandleJR_CC(); return;

		//	case (false, 0, 0, 0, 0, 1, 0, 0, 0): HandleLD_U16_SP(); return;

		//	case (false, 0, 0, _, _, 0, 0, 0, 1): HandleLD_R16_U16(); return;

		//	case (false, 0, 0, _, _, 1, 0, 0, 1): HandleADD_HL_R16(); return;

		//	case (false, 0, 0, _, _, 0, 0, 1, 0): HandleLD_R16_A(); return;
		//	case (false, 0, 0, _, _, 1, 0, 1, 0): HandleLD_A_R16(); return;

		//	case (false, 0, 0, _, _, 0, 0, 1, 1): HandleINC_R16(); return;
		//	case (false, 0, 0, _, _, 1, 0, 1, 1): HandleDEC_R16(); return;

		//	case (false, 0, 0, _, _, _, 1, 0, 0): HandleINC_R8(); return;
		//	case (false, 0, 0, _, _, _, 1, 0, 1): HandleDEC_R8(); return;

		//	case (false, 0, 0, _, _, _, 1, 1, 0): HandleLD_R8_U8(); return;
		//	case (false, 0, 1, _, _, _, _, _, _): HandleLD_R8_R8(); return;

		//	case (false, 0, 0, _, _, _, 1, 1, 1): HandleOpcodeGroupA(); return;

		//	case (false, 1, 0, _, _, _, _, _, _): HandleALU_R8(); return;
		//	case (false, 1, 1, _, _, _, 1, 1, 0): HandleALU_U8(); return;

		//	case (false, 1, 1, 0, 0, 0, 0, 1, 1): HandleJP(); return;
		//	case (false, 1, 1, 1, 0, 1, 0, 0, 1): HandleJP_HL(); return;
		//	case (false, 1, 1, 0, _, _, 0, 1, 0): HandleJP_CC(); return;

		//	case (false, 1, 1, 1, 1, 1, 0, 0, 1): HandleLD_SP_HL(); return;
		//	case (false, 1, 1, 1, 1, 1, 0, 0, 0): HandleLD_HL_SP_I8(); return;
		//	case (false, 1, 1, 1, 0, 1, 0, 0, 0): HandleADD_SP_I8(); return;

		//	case (false, 1, 1, 1, 0, 1, 0, 1, 0): HandleLD_U16_A(); return;
		//	case (false, 1, 1, 1, 1, 1, 0, 1, 0): HandleLD_A_U16(); return;

		//	case (false, 1, 1, 1, 0, 0, 0, 0, 0): HandleLD_IO_U8_A(); return;
		//	case (false, 1, 1, 1, 1, 0, 0, 0, 0): HandleLD_A_IO_U8(); return;
		//	case (false, 1, 1, 1, 0, 0, 0, 1, 0): HandleLD_IO_C_A(); return;
		//	case (false, 1, 1, 1, 1, 0, 0, 1, 0): HandleLD_A_IO_C(); return;

		//	case (false, 1, 1, _, _, 0, 1, 0, 1): HandlePUSH(); return;
		//	case (false, 1, 1, _, _, 0, 0, 0, 1): HandlePOP(); return;

		//	case (false, 1, 1, 0, 0, 1, 1, 0, 1): HandleCALL(); return;
		//	case (false, 1, 1, 0, _, _, 1, 0, 0): HandleCALL_CC(); return;
		//	case (false, 1, 1, 0, 0, 1, 0, 0, 1): HandleRET(); return;
		//	case (false, 1, 1, 0, _, _, 0, 0, 0): HandleRET_CC(); return;
		//	case (false, 1, 1, 0, 1, 1, 0, 0, 1): HandleRETI(); return;

		//	case (false, 1, 1, 1, 1, 0, 0, 1, 1): /* DI */ interruptMasterEnable = false; opcodeDone = true; return;
		//	case (false, 1, 1, 1, 1, 1, 0, 1, 1): /* EI */interruptMasterEnable = true; opcodeDone = true; return;

		//	case (false, 1, 1, _, _, _, 1, 1, 1): HandleRST(); return;

		//	case (false, 1, 1, 0, 0, 1, 0, 1, 1): /* CB */ currentOpcode = (ushort)(0xCB00 | FetchNext()); return;

		//	case (true, 0, 0, _, _, _, _, _, _): HandleOpcodeGroupC(); return;
		//	case (true, 0, 1, _, _, _, _, _, _): HandleBIT(); return;
		//	case (true, 1, 0, _, _, _, _, _, _): HandleRES(); return;
		//	case (true, 1, 1, _, _, _, _, _, _): HandleSET(); return;

		//	default:
		//		throw new NotImplementedException($"Unimplemented Opcode 0x{currentOpcode:X2}!");
		//}
	}

	private void HandleHALT ()
	{
		if (interruptMasterEnable)
		{
			NextMode = EmulatorMode.Halted;
		}
		else
		{
			if ((interruptEnable & interruptFlags) != 0)
				haltBug = true;
			else
				NextMode = EmulatorMode.Halted;
		}
		opcodeDone = true;
	}

	private void HandleLD_U16_SP ()
	{
		if (currentCycles == 2)
		{
			workingStack.PushByte(FetchNext());
		}
		else if (currentCycles == 3)
		{
			workingStack.PushByte(FetchNext());
		}
		else if (currentCycles == 4)
		{
			ushort addr = workingStack.PopWord();
			bus[addr] = (byte)(StackPointer & 0xFF);
			workingStack.PushWord(addr);
		}
		else if (currentCycles == 5)
		{
			ushort addr = workingStack.PopWord();
			bus[addr + 1] = (byte)(StackPointer >> 8);
			opcodeDone = true;
		}
	}

	private void HandleJR ()
	{
		if (currentCycles == 2)
		{
			workingStack.PushByte(FetchNext());
		}
		else if (currentCycles == 3)
		{
			ProgramCounter = (ushort)(ProgramCounter + (sbyte)workingStack.PopByte());
			opcodeDone = true;
		}
	}

	private void HandleJR_CC ()
	{
		if (currentCycles == 2)
		{
			workingStack.PushByte(FetchNext());
			int cond = (currentOpcode & 0b0001_1000) >> 3;
			if (!CheckCondition(cond))
			{
				workingStack.Clear();
				opcodeDone = true;
			}
		}
		else if (currentCycles == 3)
		{
			ProgramCounter = (ushort)(ProgramCounter + (sbyte)workingStack.PopByte());
			opcodeDone = true;
		}
	}

	private void HandleLD_R16_U16 ()
	{
		if (currentCycles == 2)
		{
			workingStack.PushByte(FetchNext());
		}
		else if (currentCycles == 3)
		{
			workingStack.PushByte(FetchNext());
			int reg = (currentOpcode & 0b0011_0000) >> 4;
			WriteR16GroupA(reg, workingStack.PopWord());
			opcodeDone = true;
		}
	}

	private void HandleADD_HL_R16 ()
	{
		if (currentCycles == 2)
		{
			int reg = (currentOpcode & 0b0011_0000) >> 4;
			int regValue = ReadR16GroupA(reg);
			FlagN = false;
			FlagH = (HL & 0x0FFF) + (regValue & 0x0FFF) > 0x0FFF;
			FlagC = (HL + regValue) > 0xFFFF;
			HL = (ushort)(HL + regValue);
			opcodeDone = true;
		}
	}

	private void HandleINC_R16 ()
	{
		if (currentCycles == 1)
		{
			int reg = (currentOpcode & 0b0011_0000) >> 4;
			int prev = ReadR16GroupA(reg);
			int now = prev + 1;
			WriteR16GroupA(reg, (ushort)((prev & 0xFF00) | (now & 0x00FF)));
			workingStack.PushWord((ushort)now);
		}
		else if (currentCycles == 2)
		{
			int reg = (currentOpcode & 0b0011_0000) >> 4;
			WriteR16GroupA(reg, workingStack.PopWord());
			opcodeDone = true;
		}
	}

	private void HandleDEC_R16 ()
	{
		if (currentCycles == 1)
		{
			int reg = (currentOpcode & 0b0011_0000) >> 4;
			int prev = ReadR16GroupA(reg);
			int now = prev - 1;
			WriteR16GroupA(reg, (ushort)((prev & 0xFF00) | (now & 0x00FF)));
			workingStack.PushWord((ushort)now);
		}
		else if (currentCycles == 2)
		{
			int reg = (currentOpcode & 0b0011_0000) >> 4;
			WriteR16GroupA(reg, workingStack.PopWord());
			opcodeDone = true;
		}
	}

	private void HandleLD_R16_A ()
	{
		if (currentCycles == 2)
		{
			int reg = (currentOpcode & 0b0011_0000) >> 4;
			WriteR16GroupB(reg, A);
			opcodeDone = true;
		}
	}

	private void HandleLD_A_R16 ()
	{
		if (currentCycles == 2)
		{
			int reg = (currentOpcode & 0b0011_0000) >> 4;
			A = ReadR16GroupB(reg);
			opcodeDone = true;
		}
	}

	private void HandleINC_R8 ()
	{
		int reg = (currentOpcode & 0b0011_1000) >> 3;
		if (reg == 0b110)
		{
			if (currentCycles == 2)
			{
				workingStack.PushByte(ReadR8(reg));
			}
			else if (currentCycles == 3)
			{
				int regValue = workingStack.PopByte();
				FlagZ = (byte)(regValue + 1) == 0;
				FlagN = false;
				FlagH = (regValue & 0x0F) + (1 & 0x0F) > 0x0F;
				WriteR8(reg, (byte)(regValue + 1));
				opcodeDone = true;
			}
		}
		else
		{
			int regValue = ReadR8(reg);
			FlagZ = (byte)(regValue + 1) == 0;
			FlagN = false;
			FlagH = (regValue & 0x0F) + (1 & 0x0F) > 0x0F;
			WriteR8(reg, (byte)(regValue + 1));
			opcodeDone = true;
		}
	}

	private void HandleDEC_R8 ()
	{
		int reg = (currentOpcode & 0b0011_1000) >> 3;
		if (reg == 0b110)
		{
			if (currentCycles == 2)
			{
				workingStack.PushByte(ReadR8(reg));
			}
			else if (currentCycles == 3)
			{
				int regValue = workingStack.PopByte();
				FlagZ = (byte)(regValue - 1) == 0;
				FlagN = true;
				FlagH = (regValue & 0x0F) - (1 & 0x0F) < 0;
				WriteR8(reg, (byte)(regValue - 1));
				opcodeDone = true;
			}
		}
		else
		{
			int regValue = ReadR8(reg);
			FlagZ = (byte)(regValue - 1) == 0;
			FlagN = true;
			FlagH = (regValue & 0x0F) - (1 & 0x0F) < 0;
			WriteR8(reg, (byte)(regValue - 1));
			opcodeDone = true;
		}
	}

	private void HandleLD_R8_U8 ()
	{
		int reg = (currentOpcode & 0b0011_1000) >> 3;
		if (reg == 0b110)
		{
			if (currentCycles == 2)
			{
				workingStack.PushByte(FetchNext());
			}
			else if (currentCycles == 3)
			{
				WriteR8(reg, workingStack.PopByte());
				opcodeDone = true;
			}
		}
		else
		{
			if (currentCycles == 2)
			{
				WriteR8(reg, FetchNext());
				opcodeDone = true;
			}
		}
	}

	private void HandleLD_R8_R8 ()
	{
		int regSource = (currentOpcode & 0b0000_0111);
		int regTarget = (currentOpcode & 0b0011_1000) >> 3;
		if (regSource == 0b110 || regTarget == 0b110)
		{
			if (currentCycles == 1)
			{
				workingStack.PushByte(ReadR8(regSource));
			}
			else if (currentCycles == 2)
			{
				WriteR8(regTarget, workingStack.PopByte());
				opcodeDone = true;
			}
		}
		else
		{
			WriteR8(regTarget, ReadR8(regSource));
			opcodeDone = true;
		}
	}

	private void HandleOpcodeGroupA ()
	{
		int op = (currentOpcode & 0b0011_1000) >> 3;

		switch (op)
		{
			case 0b000: // RLCA
				HandleRL(0b111, circular: true);
				FlagZ = false;
				break;
			case 0b001: // RRCA
				HandleRR(0b111, circular: true);
				FlagZ = false;
				break;

			case 0b010: // RLA
				HandleRL(0b111, circular: false);
				FlagZ = false;
				break;
			case 0b011: // RRA
				HandleRR(0b111, circular: false);
				FlagZ = false;
				break;

			case 0b100: // DAA
				if (!FlagN)
				{
					if (FlagC || A > 0x99) { A += 0x60; FlagC = true; }
					if (FlagH || (A & 0x0f) > 0x09) { A += 0x6; }
				}
				else
				{
					if (FlagC) { A -= 0x60; }
					if (FlagH) { A -= 0x6; }
				}
				FlagZ = (A == 0);
				FlagH = false;
				break;

			case 0b101: // CPL
				A ^= 0xFF;
				FlagN = FlagH = true;
				break;

			case 0b110: // SCF
				FlagN = FlagH = false;
				FlagC = true;
				break;
			case 0b111: // CCF
				FlagN = FlagH = false;
				FlagC = !FlagC;
				break;
		}

		opcodeDone = true;
	}

	private void HandleRL (int reg, bool circular)
	{
		byte old = ReadR8(reg);
		byte now = (byte)((old << 1) | (circular ? old.GetBit(7) : FlagC.AsBit()));
		WriteR8(reg, now);
		FlagZ = now == 0;
		FlagN = false;
		FlagH = false;
		FlagC = old.TestBit(7);
	}

	private void HandleRR (int reg, bool circular)
	{
		byte old = ReadR8(reg);
		byte now = (byte)((old >> 1) | ((circular ? old.GetBit(0) : FlagC.AsBit()) << 7));
		WriteR8(reg, now);
		FlagZ = now == 0;
		FlagN = false;
		FlagH = false;
		FlagC = old.TestBit(0);
	}

	private void HandleALU_R8 ()
	{
		int reg = (currentOpcode & 0b0000_0111);
		if (reg == 0b110)
		{
			if (currentCycles == 1)
			{
				workingStack.PushByte(ReadR8(reg));
			}
			else if (currentCycles == 2)
			{
				HandleALU(workingStack.PopByte());
				opcodeDone = true;
			}
		}
		else
		{
			HandleALU(ReadR8(reg));
			opcodeDone = true;
		}
	}

	private void HandleALU_U8 ()
	{
		HandleALU(FetchNext());
		opcodeDone = true;
	}

	private void HandleALU (byte arg)
	{
		int op = (currentOpcode & 0b0011_1000) >> 3;

		int carry = FlagC.AsBit();
		int result;
		switch (op)
		{
			case 0b000: // ADD
				result = A + arg;
				FlagZ = (result & 0xFF) == 0;
				FlagN = false;
				FlagH = (A & 0x0F) + (arg & 0x0F) > 0x0F;
				FlagC = result > 0xFF;
				A = (byte)result;
				break;
			case 0b001: // ADC
				result = A + arg + carry;
				FlagZ = (result & 0xFF) == 0;
				FlagN = false;
				FlagH = ((A & 0x0F) + (arg & 0x0F) + carry) > 0x0F;
				FlagC = result > 0xFF;
				A = (byte)result;
				break;

			case 0b010: // SUB
				result = A - arg;
				FlagZ = (result & 0xFF) == 0;
				FlagN = true;
				FlagH = (A & 0x0F) - (arg & 0x0F) < 0;
				FlagC = result < 0;
				A = (byte)result;
				break;
			case 0b011: // SBC
				result = A - arg - carry;
				FlagZ = (result & 0xFF) == 0;
				FlagN = true;
				FlagH = ((A & 0x0F) - (arg & 0x0F) - carry) < 0;
				FlagC = result < 0;
				A = (byte)result;
				break;

			case 0b100: // AND
				result = A & arg;
				FlagZ = (result & 0xFF) == 0;
				FlagN = false;
				FlagH = true;
				FlagC = false;
				A = (byte)result;
				break;
			case 0b101: // XOR
				result = A ^ arg;
				FlagZ = (result & 0xFF) == 0;
				FlagN = false;
				FlagH = false;
				FlagC = false;
				A = (byte)result;
				break;
			case 0b110: // OR
				result = A | arg;
				FlagZ = (result & 0xFF) == 0;
				FlagN = false;
				FlagH = false;
				FlagC = false;
				A = (byte)result;
				break;

			case 0b111: // CP
				result = A - arg;
				FlagZ = (result & 0xFF) == 0;
				FlagN = true;
				FlagH = (A & 0x0F) - (arg & 0x0F) < 0;
				FlagC = result < 0;
				break;
		}
	}

	private void HandleJP ()
	{
		if (currentCycles == 2)
		{
			workingStack.PushByte(FetchNext());
		}
		else if (currentCycles == 3)
		{
			workingStack.PushByte(FetchNext());
		}
		else if (currentCycles == 4)
		{
			ProgramCounter = workingStack.PopWord();
			opcodeDone = true;
		}
	}

	private void HandleJP_HL ()
	{
		ProgramCounter = HL;
		opcodeDone = true;
	}

	private void HandleJP_CC ()
	{
		if (currentCycles == 2)
		{
			workingStack.PushByte(FetchNext());
		}
		else if (currentCycles == 3)
		{
			workingStack.PushByte(FetchNext());
			int cond = (currentOpcode & 0b0001_1000) >> 3;
			if (!CheckCondition(cond))
			{
				workingStack.Clear();
				opcodeDone = true;
			}
		}
		else if (currentCycles == 4)
		{
			ProgramCounter = workingStack.PopWord();
			opcodeDone = true;
		}
	}

	private void HandleLD_SP_HL ()
	{
		if (currentCycles == 2)
		{
			StackPointer = HL;
			opcodeDone = true;
		}
	}

	private void HandleLD_HL_SP_I8 ()
	{
		if (currentCycles == 2)
		{
			workingStack.PushByte(FetchNext());
		}
		else if (currentCycles == 3)
		{
			sbyte val = (sbyte)workingStack.PopByte();
			FlagZ = false;
			FlagN = false;
			FlagH = ((StackPointer & 0x0f) + (val & 0x0f)) > 0x0f;
			FlagC = ((StackPointer & 0xff) + (val & 0xff)) > 0xff;
			HL = (ushort)(StackPointer + val);
			opcodeDone = true;
		}
	}

	private void HandleADD_SP_I8 ()
	{
		if (currentCycles == 2)
		{
			workingStack.PushByte(FetchNext());
		}
		else if (currentCycles == 3)
		{
			// Do nothing...
		}
		else if (currentCycles == 4)
		{
			sbyte val = (sbyte)workingStack.PopByte();
			FlagZ = false;
			FlagN = false;
			FlagH = ((StackPointer & 0x0f) + (val & 0x0f)) > 0x0f;
			FlagC = ((StackPointer & 0xff) + (val & 0xff)) > 0xff;
			StackPointer = (ushort)(StackPointer + val);
			opcodeDone = true;
		}
	}

	private void HandleLD_U16_A ()
	{
		if (currentCycles == 2)
		{
			workingStack.PushByte(FetchNext());
		}
		else if (currentCycles == 3)
		{
			workingStack.PushByte(FetchNext());
		}
		else if (currentCycles == 4)
		{
			bus[workingStack.PopWord()] = A;
			opcodeDone = true;
		}
	}

	private void HandleLD_A_U16 ()
	{
		if (currentCycles == 2)
		{
			workingStack.PushByte(FetchNext());
		}
		else if (currentCycles == 3)
		{
			workingStack.PushByte(FetchNext());
		}
		else if (currentCycles == 4)
		{
			A = bus[workingStack.PopWord()];
			opcodeDone = true;
		}
	}

	private void HandleLD_IO_U8_A ()
	{
		if (currentCycles == 2)
		{
			workingStack.PushByte(FetchNext());
		}
		else if (currentCycles == 3)
		{
			bus[0xFF00 + workingStack.PopByte()] = A;
			opcodeDone = true;
		}
	}

	private void HandleLD_A_IO_U8 ()
	{
		if (currentCycles == 2)
		{
			workingStack.PushByte(FetchNext());
		}
		else if (currentCycles == 3)
		{
			A = bus[0xFF00 + workingStack.PopByte()];
			opcodeDone = true;
		}
	}

	private void HandleLD_IO_C_A ()
	{
		if (currentCycles == 2)
		{
			bus[0xFF00 + C] = A;
			opcodeDone = true;
		}
	}

	private void HandleLD_A_IO_C ()
	{
		if (currentCycles == 2)
		{
			A = bus[0xFF00 + C];
			opcodeDone = true;
		}
	}

	private void HandlePUSH ()
	{
		int reg = (currentOpcode & 0b0011_0000) >> 4;
		ushort regValue = ReadR16GroupC(reg);

		if (currentCycles == 2)
		{
			// Do nothing...
		}
		else if (currentCycles == 3)
		{
			bus[--StackPointer] = (byte)(regValue >> 8);
		}
		else if (currentCycles == 4)
		{
			bus[--StackPointer] = (byte)(regValue & 0xFF);
			opcodeDone = true;
		}
	}

	private void HandlePOP ()
	{
		int reg = (currentOpcode & 0b0011_0000) >> 4;
		int regValue = ReadR16GroupC(reg);

		if (currentCycles == 2)
		{
			WriteR16GroupC(reg, (ushort)((regValue & 0xFF00) | bus[StackPointer++]));
		}
		else if (currentCycles == 3)
		{
			WriteR16GroupC(reg, (ushort)((bus[StackPointer++] << 8) | (regValue & 0x00FF)));
			opcodeDone = true;
		}
	}

	private void HandleCALL ()
	{
		if (currentCycles == 2)
		{
			workingStack.PushByte(FetchNext());
		}
		else if (currentCycles == 3)
		{
			workingStack.PushByte(FetchNext());
		}
		else if (currentCycles == 4)
		{
			// Do nothing...
		}
		else if (currentCycles == 5)
		{
			bus[--StackPointer] = (byte)(ProgramCounter >> 8);
		}
		else if (currentCycles == 6)
		{
			bus[--StackPointer] = (byte)(ProgramCounter & 0xFF);
			ProgramCounter = workingStack.PopWord();
			opcodeDone = true;
		}
	}

	private void HandleCALL_CC ()
	{
		if (currentCycles == 2)
		{
			workingStack.PushByte(FetchNext());
		}
		else if (currentCycles == 3)
		{
			workingStack.PushByte(FetchNext());
			int cond = (currentOpcode & 0b0001_1000) >> 3;
			if (!CheckCondition(cond))
			{
				workingStack.Clear();
				opcodeDone = true;
			}
		}
		else if (currentCycles == 4)
		{
			// Do nothing...
		}
		else if (currentCycles == 5)
		{
			bus[--StackPointer] = (byte)(ProgramCounter >> 8);
		}
		else if (currentCycles == 6)
		{
			bus[--StackPointer] = (byte)(ProgramCounter & 0xFF);
			ProgramCounter = workingStack.PopWord();
			opcodeDone = true;
		}
	}

	private void HandleRET ()
	{
		if (currentCycles == 2)
		{
			workingStack.PushByte(bus[StackPointer++]);
		}
		else if (currentCycles == 3)
		{
			workingStack.PushByte(bus[StackPointer++]);
		}
		else if (currentCycles == 4)
		{
			byte high = workingStack.PopByte();
			byte low = workingStack.PopByte();
			ProgramCounter = (ushort)((high << 8) | low);
			opcodeDone = true;
		}
	}

	private void HandleRETI ()
	{
		HandleRET();
		if (currentCycles == 4)
			interruptMasterEnable = true;
	}

	private void HandleRET_CC ()
	{
		if (currentCycles == 2)
		{
			int cond = (currentOpcode & 0b0001_1000) >> 3;
			if (!CheckCondition(cond))
				opcodeDone = true;
		}
		else if (currentCycles == 3)
		{
			workingStack.PushByte(bus[StackPointer++]);
		}
		else if (currentCycles == 4)
		{
			workingStack.PushByte(bus[StackPointer++]);
		}
		else if (currentCycles == 5)
		{
			byte high = workingStack.PopByte();
			byte low = workingStack.PopByte();
			ProgramCounter = (ushort)((high << 8) | low);
			opcodeDone = true;
		}
	}

	private void HandleRST ()
	{
		if (currentCycles == 2)
		{
			// Do nothing...
		}
		else if (currentCycles == 3)
		{
			bus[--StackPointer] = (byte)(ProgramCounter >> 8);
		}
		else if (currentCycles == 4)
		{
			bus[--StackPointer] = (byte)(ProgramCounter & 0xFF);
			ProgramCounter = (ushort)(currentOpcode & 0b0011_1000);
			opcodeDone = true;
		}
	}

	private void HandleOpcodeGroupC ()
	{
		int op = (currentOpcode & 0b0011_1000) >> 3;
		int reg = currentOpcode & 0b0000_0111;

		if (reg == 0b110 && currentCycles == 4 || currentCycles == 2)
		{
			switch (op)
			{
				case 0b000: // RLC
					HandleRL(reg, circular: true);
					break;
				case 0b001: // RRC
					HandleRR(reg, circular: true);
					break;

				case 0b010: // RL
					HandleRL(reg, circular: false);
					break;
				case 0b011: // RR
					HandleRR(reg, circular: false);
					break;

				case 0b100: // SLA
				{
					byte old = ReadR8(reg);
					byte result = (byte)(old << 1);
					WriteR8(reg, result);
					FlagZ = result == 0;
					FlagN = false;
					FlagH = false;
					FlagC = old.TestBit(7);
					break;
				}

				case 0b101: // SRA
				{
					byte old = ReadR8(reg);
					byte result = (byte)((old >> 1) | (old & (1 << 7)));
					WriteR8(reg, result);
					FlagZ = result == 0;
					FlagN = false;
					FlagH = false;
					FlagC = old.TestBit(0);
					break;
				}

				case 0b110: // SWAP
				{
					byte old = ReadR8(reg);
					byte result = (byte)(((old & 0x0F) << 4) | ((old & 0xF0) >> 4));
					WriteR8(reg, result);
					FlagZ = result == 0;
					FlagN = false;
					FlagH = false;
					FlagC = false;
					break;
				}

				case 0b111: // SRL
				{
					byte old = ReadR8(reg);
					byte result = (byte)(old >> 1);
					WriteR8(reg, result);
					FlagZ = result == 0;
					FlagN = false;
					FlagH = false;
					FlagC = old.TestBit(0);
					break;
				}
			}

			opcodeDone = true;
		}
	}

	private void HandleBIT ()
	{
		int reg = currentOpcode & 0b0000_0111;
		if (reg == 0b110)
		{
			if (currentCycles == 3)
			{
				int bitNo = (currentOpcode & 0b0011_1000) >> 3;
				FlagZ = !ReadR8(reg).TestBit(bitNo);
				FlagN = false;
				FlagH = true;
				opcodeDone = true;
			}
		}
		else
		{
			int bitNo = (currentOpcode & 0b0011_1000) >> 3;
			FlagZ = !ReadR8(reg).TestBit(bitNo);
			FlagN = false;
			FlagH = true;
			opcodeDone = true;
		}
	}

	private void HandleRES ()
	{
		int reg = currentOpcode & 0b0000_0111;
		if (reg == 0b110)
		{
			if (currentCycles == 3)
			{
				workingStack.PushByte(ReadR8(reg));
			}
			else if (currentCycles == 4)
			{
				int bitNo = (currentOpcode & 0b0011_1000) >> 3;
				WriteR8(reg, workingStack.PopByte().UnsetBit(bitNo));
				opcodeDone = true;
			}
		}
		else
		{
			if (currentCycles == 2)
			{
				int bitNo = (currentOpcode & 0b0011_1000) >> 3;
				WriteR8(reg, ReadR8(reg).UnsetBit(bitNo));
				opcodeDone = true;
			}
		}
	}

	private void HandleSET ()
	{
		int reg = currentOpcode & 0b0000_0111;
		if (reg == 0b110)
		{
			if (currentCycles == 3)
			{
				workingStack.PushByte(ReadR8(reg));
			}
			else if (currentCycles == 4)
			{
				int bitNo = (currentOpcode & 0b0011_1000) >> 3;
				WriteR8(reg, workingStack.PopByte().SetBit(bitNo));
				opcodeDone = true;
			}
		}
		else
		{
			if (currentCycles == 2)
			{
				int bitNo = (currentOpcode & 0b0011_1000) >> 3;
				WriteR8(reg, ReadR8(reg).SetBit(bitNo));
				opcodeDone = true;
			}
		}
	}
}