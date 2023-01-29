using System;
using System.IO;
using System.Threading.Tasks;

namespace Gambit.Core;

public interface ISaveData
{
	void CreateScope(string scopeName);

	void WriteBool(string name, bool value);
	void WriteByte(string name, byte value);
	void WriteWord(string name, ushort value);
	void WriteInt(string name, int value);
	void WriteString(string name, string value);
	void WriteSpan(string name, Span<byte> buffer);

	Task SaveToStream(Stream stream);
}
