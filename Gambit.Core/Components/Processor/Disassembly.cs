using System;

namespace Gambit.Core;

public static class Disassembler
{
	public static (string Text, int Length) Disassemble(Func<byte> fetch)
	{
		byte op = fetch();
		bool ext = false;

		if (op == 0xCB)
		{
			op = fetch();
			ext = true;
		}

		ushort Word() => (ushort)(fetch() | (fetch() << 8));

		return (ext, op.GetBit(7), op.GetBit(6), op.GetBit(5), op.GetBit(4), op.GetBit(3), op.GetBit(2), op.GetBit(1), op.GetBit(0)) switch
		{
			(false, 0, 0, 0, 0, 0, 0, 0, 0) => ("NOP", 1),

			(false, 0, 0, 0, 1, 0, 0, 0, 0) => ("STOP", 1),
			(false, 0, 1, 1, 1, 0, 1, 1, 0) => ("HALT", 1),

			(false, 0, 0, 0, 0, 1, 0, 0, 0) => ($"LD ({Word():X4}), SP", 3),

			(false, 0, 0, 0, 1, 1, 0, 0, 0) => ($"JR {(sbyte)fetch()}", 2),
			(false, 0, 0, 1, _, _, 0, 0, 0) => ($"JR {Condition(op, 3)}, {(sbyte)fetch()}", 2),

			(false, 0, 0, _, _, 0, 0, 0, 1) => ($"LD {R16A(op, 4)}, {Word():X4}", 3),
			(false, 0, 0, _, _, 1, 0, 0, 1) => ($"ADD HL, {R16A(op, 4)}", 1),

			(false, 0, 0, _, _, 0, 0, 1, 0) => ($"LD {R16B(op, 4)}, A", 1),
			(false, 0, 0, _, _, 1, 0, 1, 0) => ($"LD A, {R16B(op, 4)}", 1),

			(false, 0, 0, _, _, 0, 0, 1, 1) => ($"INC {R16A(op, 4)}", 1),
			(false, 0, 0, _, _, 1, 0, 1, 1) => ($"DEC {R16A(op, 4)}", 1),

			(false, 0, 0, _, _, _, 1, 0, 0) => ($"INC {R8(op, 3)}", 1),
			(false, 0, 0, _, _, _, 1, 0, 1) => ($"DEC {R8(op, 3)}", 1),
			(false, 0, 0, _, _, _, 1, 1, 0) => ($"LD {R8(op, 3)}, {fetch():X2}", 2),
			(false, 0, 1, _, _, _, _, _, _) => ($"LD {R8(op, 3)}, {R8(op, 0)}", 1),

			(false, 0, 0, _, _, _, 1, 1, 1) => (OpcodeAcc(op, 3), 1),

			(false, 1, 0, _, _, _, _, _, _) => ($"{OpcodeAlu(op, 3)} A, {R8(op, 0)}", 1),
			(false, 1, 1, _, _, _, 1, 1, 0) => ($"{OpcodeAlu(op, 3)} A, {fetch():X2}", 2),

			(false, 1, 1, 0, 0, 1, 1, 0, 1) => ($"CALL {Word():X4}", 3),
			(false, 1, 1, 0, _, _, 1, 0, 0) => ($"CALL {Condition(op, 3)}, {Word():X4}", 3),
			(false, 1, 1, _, _, _, 1, 1, 1) => ($"RST {(op & 0b00111000):X2}", 1),
			(false, 1, 1, 0, 0, 1, 0, 0, 1) => ($"RET", 1),
			(false, 1, 1, 0, 1, 1, 0, 0, 1) => ($"RETI", 1),
			(false, 1, 1, 0, _, _, 0, 0, 0) => ($"RET {Condition(op, 3)}", 1),

			(false, 1, 1, 1, 0, 0, 0, 0, 0) => ($"LD (FF00 + {fetch():X2}), A", 2),
			(false, 1, 1, 1, 1, 0, 0, 0, 0) => ($"LD A, (FF00 + {fetch():X2})", 2),
			(false, 1, 1, 1, 1, 0, 0, 1, 0) => ($"LD A, (FF00 + C)", 1),
			(false, 1, 1, 1, 0, 0, 0, 1, 0) => ($"LD (FF00 + C), A", 1),
			(false, 1, 1, 1, 1, 1, 0, 1, 0) => ($"LD A, ({Word():X4})", 3),
			(false, 1, 1, 1, 0, 1, 0, 1, 0) => ($"LD ({Word():X4}), A", 3),

			(false, 1, 1, 1, 0, 1, 0, 0, 0) => ($"ADD SP, {(sbyte)fetch()}", 2),
			(false, 1, 1, 1, 1, 1, 0, 0, 0) => ($"LD HL, SP + {(sbyte)fetch()}", 2),
			(false, 1, 1, 1, 1, 1, 0, 0, 1) => ($"LD SP, HL", 1),

			(false, 1, 1, 0, 0, 0, 0, 1, 1) => ($"JP {Word():X4}", 3),
			(false, 1, 1, 1, 0, 0, 0, 0, 1) => ($"JP HL", 1),
			(false, 1, 1, 0, _, _, 0, 1, 0) => ($"JP {Condition(op, 3)}, {Word():X4}", 3),

			(false, 1, 1, _, _, 0, 1, 0, 1) => ($"PUSH {R16C(op, 4)}", 1),
			(false, 1, 1, _, _, 0, 0, 0, 1) => ($"POP {R16C(op, 4)}", 1),

			(false, 1, 1, 1, 1, 0, 0, 1, 1) => ("DI", 1),
			(false, 1, 1, 1, 1, 1, 0, 1, 1) => ("EI", 1),

			(true, 0, 0, _, _, _, _, _, _) => ($"{OpcodeCB(op, 3)} {R8(op, 0)}", 2),
			(true, 0, 1, _, _, _, _, _, _) => ($"BIT {(op & 0b00111000) >> 3}, {R8(op, 0)}", 2),
			(true, 1, 0, _, _, _, _, _, _) => ($"RES {(op & 0b00111000) >> 3}, {R8(op, 0)}", 2),
			(true, 1, 1, _, _, _, _, _, _) => ($"SET {(op & 0b00111000) >> 3}, {R8(op, 0)}", 2),

			_ => ("INVALID", 1),
		};
	}

	private static string OpcodeAcc(int code, int shift) => ((code >> shift) & 0b111) switch
	{
		0 => "RLCA",
		1 => "RRCA",
		2 => "RLA",
		3 => "RRA",
		4 => "DAA",
		5 => "CPL",
		6 => "SCF",
		7 => "CCF",
		_ => ""
	};

	private static string OpcodeAlu(int code, int shift) => ((code >> shift) & 0b111) switch
	{
		0 => "ADD",
		1 => "ADC",
		2 => "SUB",
		3 => "SBC",
		4 => "AND",
		5 => "XOR",
		6 => "OR",
		7 => "CP",
		_ => ""
	};

	private static string OpcodeCB(int code, int shift) => ((code >> shift) & 0b111) switch
	{
		0 => "RLC",
		1 => "RRC",
		2 => "RL",
		3 => "RR",
		4 => "SLA",
		5 => "SRA",
		6 => "SWAP",
		7 => "SRL",
		_ => ""
	};

	private static string R8(int reg, int shift) => ((reg >> shift) & 0b111) switch
	{
		0b000 => "B",
		0b001 => "C",
		0b010 => "D",
		0b011 => "E",
		0b100 => "H",
		0b101 => "L",
		0b110 => "(HL)",
		0b111 => "A",
		_ => ""
	};

	private static string R16C(int reg, int shift) => ((reg >> shift) & 0b11) switch
	{
		0b00 => "BC",
		0b01 => "DE",
		0b10 => "HL",
		0b11 => "AF",
		_ => ""
	};

	private static string R16B(int reg, int shift) => ((reg >> shift) & 0b11) switch
	{
		0b00 => "(BC)",
		0b01 => "(DE)",
		0b10 => "(HL+)",
		0b11 => "(HL-)",
		_ => ""
	};

	private static string R16A(int reg, int shift) => ((reg >> shift) & 0b11) switch
	{
		0b00 => "BC",
		0b01 => "DE",
		0b10 => "HL",
		0b11 => "SP",
		_ => ""
	};

	private static string Condition(int cc, int shift) => ((cc >> shift) & 0b11) switch
	{
		0b00 => "NZ",
		0b01 => "Z",
		0b10 => "NC",
		0b11 => "C",
		_ => ""
	};
}