using System;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Gambit.Core;

namespace Gambit.UI.Models;

public class JsonLoadData : ILoadData
{
	private JsonObject root;
	private JsonObject currentScope;

	public async Task LoadFromStream (Stream stream)
	{
		using StreamReader reader = new(stream);
		root = JsonNode.Parse(await reader.ReadToEndAsync()).AsObject();
		currentScope = root;
	}

	public void OpenScope (string scopeName) => currentScope = root[scopeName].AsObject();

	public bool ReadBool (string name) => currentScope[name].GetValue<bool>();
	public byte ReadByte (string name) => currentScope[name].GetValue<byte>();
	public ushort ReadWord (string name) => currentScope[name].GetValue<ushort>();
	public int ReadInt (string name) => currentScope[name].GetValue<int>();
	public string ReadString (string name) => currentScope[name].GetValue<string>();
	public void ReadSpan (string name, Span<byte> buffer) => Convert.FromBase64String(currentScope[name].GetValue<string>()).CopyTo(buffer);
}