namespace Gambit.Core;

public enum EmulatorMode : byte
{
    None = 0,

    Running,

    Halted,
    Stopped,
}