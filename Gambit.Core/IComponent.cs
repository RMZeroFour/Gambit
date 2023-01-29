namespace Gambit.Core;

public interface IComponent
{
    void RegisterHandlers (AddressBus bus);
    void AdvanceCycle (AddressBus bus, EmulatorMode currentMode, bool debug = false);
}