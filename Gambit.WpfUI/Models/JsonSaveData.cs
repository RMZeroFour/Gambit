using System;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Gambit.Core;

namespace Gambit.UI.Models;

public class JsonSaveData : ISaveData
{
	private readonly JsonObject root = new();
	private JsonObject currentScope;

	public JsonSaveData ()
	{
		currentScope = root;
	}

	public void CreateScope (string scopeName)
	{
		currentScope = new JsonObject();
		root[scopeName] = currentScope;
	}

	public void WriteBool (string name, bool value) => currentScope[name] = value;
	public void WriteByte (string name, byte value) => currentScope[name] = value;
	public void WriteWord (string name, ushort value) => currentScope[name] = value;
	public void WriteInt (string name, int value) => currentScope[name] = value;
	public void WriteString (string name, string value) => currentScope[name] = value;
	public void WriteSpan (string name, Span<byte> buffer) => currentScope[name] = Convert.ToBase64String(buffer);

	public async Task SaveToStream (Stream stream)
	{
		using StreamWriter writer = new(stream);
		await writer.WriteAsync(root.ToJsonString());
	}
}
