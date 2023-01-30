namespace Gambit.Core;

public class VideoSystem : IComponent, ISerializable
{
	public VideoBuffer FinalBuffer { get; } = new(160, 144);
	public VideoBuffer BackgroundLayer { get; } = new(256, 256);
	public VideoBuffer WindowLayer { get; } = new(160, 144);
	public VideoBuffer SpriteData { get; } = new(80, 64);
	public VideoBuffer TileData { get; } = new(16 * 8, 24 * 8);

	private readonly byte[] registers = new byte[0xC];
	private readonly byte[] oam = new byte[0xA0];
	private readonly byte[] videoRam = new byte[0x2000];

	private ref byte LcdControl => ref registers[0xFF40 - 0xFF40];
	private ref byte LcdStatus => ref registers[0xFF41 - 0xFF40];

	private byte ScrollY => registers[0xFF42 - 0xFF40];
	private byte ScrollX => registers[0xFF43 - 0xFF40];
	private byte WindowY => registers[0xFF4A - 0xFF40];
	private byte WindowX => (byte)(registers[0xFF4B - 0xFF40] - 7);

	private ref byte LcdY => ref registers[0xFF44 - 0xFF40];
	private byte LcdYCompare => registers[0xFF45 - 0xFF40];

	private byte BgPallete => registers[0xFF47 - 0xFF40];
	private byte ObjPalleteA => registers[0xFF48 - 0xFF40];
	private byte ObjPalleteB => registers[0xFF49 - 0xFF40];

	private VideoMode CurrentVideoMode
	{
		get => (VideoMode)(LcdStatus & 0b11);
		set => LcdStatus = (byte)((LcdStatus & ~0b11) | (byte)value);
	}

	private int modeClock;
	private int windowLineCounter;

	public void RegisterHandlers (AddressBus bus)
	{
		bus.AttachReader(0x8000..0xA000, i => videoRam[i - 0x8000]);
		bus.AttachWriter(0x8000..0xA000, (i, val) => videoRam[i - 0x8000] = val);

		bus.AttachReader(0xFF40..0xFF46, i => registers[i - 0xFF40]);
		bus.AttachWriter(0xFF40..0xFF46, (i, val) => registers[i - 0xFF40] = val);

		bus.AttachReader(0xFF47..0xFF4C, i => registers[i - 0xFF40]);
		bus.AttachWriter(0xFF47..0xFF4C, (i, val) => registers[i - 0xFF40] = val);

		bus.AttachReader(0xFE00..0xFEA0, i => oam[i - 0xFE00]);
		bus.AttachWriter(0xFE00..0xFEA0, (i, val) => oam[i - 0xFE00] = val);
	}

	public void AdvanceCycle (AddressBus bus, EmulatorMode currentMode, bool debug = false)
	{
#pragma warning Dr. Mario doesn't run past menu!
		//if (!LcdControl.TestBit(7)) // LCDC.7 (LCD Enable)
		//return;

		switch (CurrentVideoMode)
		{
			case VideoMode.OamSearch:
				if (modeClock == 20)
				{
					modeClock = 0;
					CurrentVideoMode = VideoMode.VRamTransfer;
				}
				break;

			case VideoMode.VRamTransfer:
				if (modeClock == 43)
				{
					modeClock = 0;

					CurrentVideoMode = VideoMode.HBlank;
					if (LcdStatus.TestBit(3)) // LCDC.3 (HBlank STAT Interrupt)
						bus[0xFF0F] = bus[0xFF0F].SetBit(1);

					RenderBgAndWindowLine();
					RenderSpriteLine();
				}
				break;

			case VideoMode.HBlank:
				if (modeClock == 51)
				{
					modeClock = 0;

					if (++LcdY > 143)
					{
						CurrentVideoMode = VideoMode.VBlank;
						if (LcdStatus.TestBit(4)) // LCDC.4 (VBlank STAT Interrupt)
							bus[0xFF0F] = bus[0xFF0F].SetBit(1);
						bus[0xFF0F] = bus[0xFF0F].SetBit(0);
					}
					else
					{
						CurrentVideoMode = VideoMode.OamSearch;
						if (LcdStatus.TestBit(2)) // LCDC.2 (Oam STAT Interrupt)
							bus[0xFF0F] = bus[0xFF0F].SetBit(1);
					}

					LcdStatus = LcdStatus.ModifyBit(2, LcdY == LcdYCompare); // LCDC.2 (LYC=LY Compare)
					if (LcdStatus.TestBit(6) && LcdY == LcdYCompare) // LCDC.6 (LYC=LY STAT Interrupt)
						bus[0xFF0F] = bus[0xFF0F].SetBit(1);
				}
				break;

			case VideoMode.VBlank:
				if (modeClock == 114)
				{
					modeClock = 0;

					if (++LcdY >= 153)
					{
						LcdY = 0;
						windowLineCounter = 0;

						CurrentVideoMode = VideoMode.OamSearch;
						if (LcdStatus.TestBit(2)) // LCDC.2 (Oam STAT Interrupt)
							bus[0xFF0F] = bus[0xFF0F].SetBit(1);

						if (debug)
						{
							OutputBackgroundData(bus);
							OutputWindowData(bus);
							OutputSpriteData(bus);
							OutputTileData(bus);
						}
					}
				}
				break;
		}

		++modeClock;
	}

	private void OutputBackgroundData (AddressBus bus)
	{
		if (LcdControl.TestBit(0))
		{
			bool unsignedAccess = LcdControl.TestBit(4);
			int tileDataStart = unsignedAccess ? 0x8000 : 0x8800;
			int tileMapStart = LcdControl.TestBit(3) ? 0x9C00 : 0x9800;

			for (int pixelY = 0; pixelY < 256; pixelY++)
			{
				int tileMapRow = pixelY / 8;

				for (int pixelX = 0; pixelX < 256; pixelX++)
				{
					int tileMapCol = pixelX / 8;

					int tileMapLocation = tileMapStart + tileMapRow * 32 + tileMapCol;

					int tileDataIndex = unsignedAccess
						? bus[tileMapLocation]
						: (sbyte)bus[tileMapLocation];

					int tileDataLocation = unsignedAccess
						? tileDataStart + tileDataIndex * 16
						: tileDataStart + (tileDataIndex + 128) * 16;

					int tileRow = pixelY % 8;
					byte lowerByte = bus[tileDataLocation + tileRow * 2];
					byte higherByte = bus[tileDataLocation + tileRow * 2 + 1];

					int tileCol = pixelX % 8;
					int colorIndex = (higherByte.GetBit(7 - tileCol) << 1) | lowerByte.GetBit(7 - tileCol);

					if (pixelX >= 0 && pixelX < BackgroundLayer.Width && pixelY >= 0 && pixelY < BackgroundLayer.Height)
						BackgroundLayer[pixelX, pixelY] = MapToPallete(colorIndex, BgPallete);
				}
			}
		}
		else
		{
			BackgroundLayer.Pixels.Fill(MapToPallete(0b00, BgPallete));
		}
	}

	private void OutputWindowData (AddressBus bus)
	{
		if (LcdControl.TestBit(0) && LcdControl.TestBit(5))
		{
			bool unsignedAccess = LcdControl.TestBit(4);
			int tileDataStart = unsignedAccess ? 0x8000 : 0x8800;
			int tileMapStart = LcdControl.TestBit(6) ? 0x9C00 : 0x9800;

			for (int pixelY = WindowY; pixelY < 160; pixelY++)
			{
				int tileMapRow = pixelY / 8;

				for (int pixelX = WindowX; pixelX < 144; pixelX++)
				{
					int tileMapCol = pixelX / 8;

					int tileMapLocation = tileMapStart + tileMapRow * 32 + tileMapCol;

					int tileDataIndex = unsignedAccess
						? bus[tileMapLocation]
						: (sbyte)bus[tileMapLocation];

					int tileDataLocation = unsignedAccess
						? tileDataStart + tileDataIndex * 16
						: tileDataStart + (tileDataIndex + 128) * 16;

					int tileRow = pixelY % 8;
					byte lowerByte = bus[tileDataLocation + tileRow * 2];
					byte higherByte = bus[tileDataLocation + tileRow * 2 + 1];

					int tileCol = pixelX % 8;
					int colorIndex = (higherByte.GetBit(7 - tileCol) << 1) | lowerByte.GetBit(7 - tileCol);

					if (pixelX >= 0 && pixelX < WindowLayer.Width && pixelY >= 0 && pixelY < WindowLayer.Height)
						WindowLayer[pixelX, pixelY] = MapToPallete(colorIndex, BgPallete);
				}
			}
		}
		else
		{
			WindowLayer.Pixels.Fill(MapToPallete(0b00, BgPallete));
		}
	}

	private void OutputSpriteData (AddressBus bus)
	{
		int sprHeight = LcdControl.TestBit(2) ? 16 : 8;

		int sx = 0, sy = 0;

		for (int i = 0; i < oam.Length; i += 4)
		{
			byte tileIndex = oam[i + 2];
			byte pallete = oam[i + 3].TestBit(4) ? ObjPalleteB : ObjPalleteA;
			bool xFlip = oam[i + 3].TestBit(5);
			bool yFlip = oam[i + 3].TestBit(6);

			for (int pixelY = 0; pixelY < sprHeight; pixelY++)
			{
				int tileRow = pixelY;
				byte lowerByte = bus[0x8000 + tileIndex * 16 + tileRow * 2];
				byte higherByte = bus[0x8000 + tileIndex * 16 + tileRow * 2 + 1];
				int yCoord = yFlip ? sy + sprHeight - pixelY : sy + pixelY;

				for (int pixelX = 0; pixelX < 8; pixelX++)
				{
					int tileCol = pixelX;
					int colorIndex = (higherByte.GetBit(7 - tileCol) << 1) | lowerByte.GetBit(7 - tileCol);

					int xCoord = xFlip ? sx + 8 - pixelX : sx + pixelX;

					if (xCoord >= 0 && xCoord < SpriteData.Width && yCoord >= 0 && yCoord < SpriteData.Height)
						SpriteData[xCoord, yCoord] = MapToPallete(colorIndex, pallete);
				}
			}

			sx += 8;
			if (sx == SpriteData.Width)
			{
				sx = 0;
				sy += sprHeight;
			}
		}
	}

	private void OutputTileData (AddressBus bus)
	{
		int pixelX = 0, pixelY = 0;
		for (int addr = 0x8000; addr < 0x9800; addr += 16)
		{
			for (int line = 0; line < 8; line++)
			{
				var (first, second) = (bus[addr + 2 * line], bus[addr + 2 * line + 1]);

				for (int k = 0; k < 8; k++)
				{
					int color = (second.GetBit(8 - k) << 1) | first.GetBit(8 - k);
					Pixel p = MapToPallete(color, BgPallete);
					TileData[pixelX + k, pixelY + line] = p;
				}
			}

			pixelX += 8;
			if (pixelX == TileData.Width)
			{
				pixelX = 0;
				pixelY += 8;
			}
		}
	}

	private void RenderBgAndWindowLine ()
	{
		if (LcdControl.TestBit(0)) // LCDC.0 (BG & Window Enable)
		{
			bool unsignedAccess = LcdControl.TestBit(4);
			bool drawingWindow = LcdControl.TestBit(5) && LcdY >= WindowY;

			int tileDataStart = unsignedAccess ? 0x0000 : 0x0800;
			int tileMapStart = LcdControl.TestBit(drawingWindow ? 6 : 3) ? 0x1C00 : 0x1800;

			//int yCoord = drawingWindow ? LcdY - WindowY : LcdY + ScrollY;
			int yCoord = drawingWindow ? windowLineCounter++ : LcdY + ScrollY;
			if (yCoord >= 256)
				yCoord -= 256;
			int tileMapRow = yCoord / 8;

			for (int pixelX = 0; pixelX < 160; pixelX++)
			{
				int xCoord = drawingWindow && pixelX >= WindowX ? pixelX - WindowX : pixelX + ScrollX;
				if (xCoord >= 256)
					xCoord -= 256;
				int tileMapCol = xCoord / 8;

				int tileMapLocation = tileMapStart + tileMapRow * 32 + tileMapCol;

				int tileDataIndex = unsignedAccess
					? videoRam[tileMapLocation]
					: (sbyte)videoRam[tileMapLocation];

				int tileDataLocation = unsignedAccess
					? tileDataStart + tileDataIndex * 16
					: tileDataStart + (tileDataIndex + 128) * 16;

				int tileRow = yCoord % 8;
				byte lowerByte = videoRam[tileDataLocation + tileRow * 2];
				byte higherByte = videoRam[tileDataLocation + tileRow * 2 + 1];

				int tileCol = xCoord % 8;
				int colorIndex = (higherByte.GetBit(7 - tileCol) << 1) | lowerByte.GetBit(7 - tileCol);

				if (pixelX >= 0 && pixelX < 160 && LcdY >= 0 && LcdY < 144)
					FinalBuffer[pixelX, LcdY] = MapToPallete(colorIndex, BgPallete);
			}
		}
		else
		{
			for (int pixelX = 0; pixelX < 160; pixelX++)
				FinalBuffer[pixelX, LcdY] = MapToPallete(0b00, BgPallete);
		}
	}

	private void RenderSpriteLine ()
	{
		int sprHeight = LcdControl.TestBit(2) ? 16 : 8;
		int spritesInThisLine = 0;

		for (int sprLocation = oam.Length - 4; sprLocation >= 0; sprLocation -= 4)
		{
			int sprY = oam[sprLocation] - 16;
			int sprX = oam[sprLocation + 1] - 8;
			byte tileLocation = oam[sprLocation + 2];

			byte sprAttrs = oam[sprLocation + 3];
			bool sprFlipX = sprAttrs.TestBit(5);
			bool sprFlipY = sprAttrs.TestBit(6);
			byte pallete = sprAttrs.TestBit(4) ? ObjPalleteB : ObjPalleteA;
			bool bgPriority = sprAttrs.TestBit(7);

			Pixel transparentColor = MapToPallete(0b00, pallete);

			if (LcdY >= sprY && LcdY < sprY + sprHeight)
			{
				++spritesInThisLine;

				int tileLine = sprFlipY ? sprHeight - 1 - (LcdY - sprY) : LcdY - sprY;

				int dataAddress = tileLocation * 16 + tileLine * 2;
				byte lowerByte = videoRam[dataAddress];
				byte higherByte = videoRam[dataAddress + 1];

				for (int tileCol = 0; tileCol < 8; tileCol++)
				{
					int requiredBit = sprFlipX ? tileCol : 7 - tileCol;

					int colorIndex = (higherByte.GetBit(requiredBit) << 1) | lowerByte.GetBit(requiredBit);

					Pixel col = MapToPallete(colorIndex, pallete);

					// Color 0b00 is transparent for sprites.
					if (col == transparentColor)
						continue;

					if (sprX + tileCol >= 0 && sprX + tileCol < 160 && LcdY >= 0 && LcdY < 144)
						if (!bgPriority || FinalBuffer[sprX + tileCol, LcdY] == transparentColor)
							FinalBuffer[sprX + tileCol, LcdY] = MapToPallete(colorIndex, BgPallete);
				}
			}

			if (spritesInThisLine == 10)
				break;
		}
	}

	private Pixel MapToPallete (int col, byte pallete)
	{
		int palleteBits = (col & 0b11) switch
		{
			0b00 => (pallete & 0b00000011),
			0b01 => (pallete & 0b00001100) >> 2,
			0b10 => (pallete & 0b00110000) >> 4,
			0b11 => (pallete & 0b11000000) >> 6,
			_ => 0,
		};

		return (Pixel)palleteBits;
	}

	public void Serialize (ISaveData saveData)
	{
		saveData.CreateScope(nameof(VideoSystem));

		saveData.WriteInt(nameof(modeClock), modeClock);
		saveData.WriteInt(nameof(windowLineCounter), windowLineCounter);
		saveData.WriteSpan(nameof(registers), registers);
		saveData.WriteSpan(nameof(oam), oam);
		saveData.WriteSpan(nameof(videoRam), videoRam);
	}

	public bool Deserialize (ILoadData loadData)
	{
		try
		{
			loadData.OpenScope(nameof(VideoSystem));

			modeClock = loadData.ReadInt(nameof(modeClock));
			windowLineCounter = loadData.ReadInt(nameof(windowLineCounter));
			loadData.ReadSpan(nameof(registers), registers);
			loadData.ReadSpan(nameof(oam), oam);
			loadData.ReadSpan(nameof(videoRam), videoRam);

			return true;
		}
		catch
		{
			return false;
		}
	}

	private enum VideoMode : byte
	{
		HBlank = 0b00,
		VBlank = 0b01,
		OamSearch = 0b10,
		VRamTransfer = 0b11,
	}
}
