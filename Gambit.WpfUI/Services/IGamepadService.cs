using System.Collections.Generic;
using System.Windows.Input;
using Gambit.Core;

namespace Gambit.UI.Services;

public interface IGamepadService
{
	void UpdateJoypad(Joypad jp);
}

public class KeyboardGamepadService : IGamepadService
{
	private readonly Dictionary<string, Key> keymap = new()
	{
		{ nameof(Joypad.A), Key.Z },
		{ nameof(Joypad.B), Key.X },
		{ nameof(Joypad.Start), Key.Enter },
		{ nameof(Joypad.Select), Key.Space },

		{ nameof(Joypad.Left), Key.Left },
		{ nameof(Joypad.Right), Key.Right },
		{ nameof(Joypad.Up), Key.Up },
		{ nameof(Joypad.Down), Key.Down },
	};

	public void UpdateJoypad(Joypad jp)
	{
		bool GetKey(string name) => Keyboard.IsKeyDown(keymap[name]);

		jp.A = GetKey(nameof(Joypad.A));
		jp.B = GetKey(nameof(Joypad.B));
		jp.Start = GetKey(nameof(Joypad.Start));
		jp.Select = GetKey(nameof(Joypad.Select));

		jp.Left = GetKey(nameof(Joypad.Left));
		jp.Right = GetKey(nameof(Joypad.Right));
		jp.Up = GetKey(nameof(Joypad.Up));
		jp.Down = GetKey(nameof(Joypad.Down));
	}
}