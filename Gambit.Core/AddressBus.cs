using System;
using System.Collections.Generic;

namespace Gambit.Core;

public class AddressBus
{
    private readonly Dictionary<Range, ReadByteFunc> readFuncs = new();
    private readonly Dictionary<Range, WriteByteFunc> writeFuncs = new();

    private RangeMap<ReadByteFunc> readQuickMap;
    private RangeMap<WriteByteFunc> writeQuickMap;

    public byte this[int addr]
    {
        get => readQuickMap.GetValueOrDefault(addr)?.Invoke(addr) ?? 0xFF;
        set => writeQuickMap.GetValueOrDefault(addr)?.Invoke(addr, value);
    }

    public void AttachReader (int index, ReadByteFunc read) => readFuncs.Add(new(index, index + 1), read);
    public void AttachReader (Range range, ReadByteFunc read) => readFuncs.Add(range, read);

    public void AttachWriter (int index, WriteByteFunc write) => writeFuncs.Add(new(index, index + 1), write);
    public void AttachWriter (Range range, WriteByteFunc write) => writeFuncs.Add(range, write);

    public void BuildHandlerMaps ()
    {
        readQuickMap = new(readFuncs);
        writeQuickMap = new(writeFuncs);
    }

    public delegate byte ReadByteFunc (int addr);
    public delegate void WriteByteFunc (int addr, byte value);
}