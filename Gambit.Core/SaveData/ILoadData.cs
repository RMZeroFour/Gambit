using System;
using System.IO;
using System.Threading.Tasks;

namespace Gambit.Core;

public interface ILoadData
{
	Task LoadFromStream(Stream stream);

	void OpenScope(string scopeName);

	bool ReadBool(string name);
	byte ReadByte(string name);
	ushort ReadWord(string name);
	int ReadInt(string name);
	string ReadString(string name);
	void ReadSpan(string name, Span<byte> buffer);
}
