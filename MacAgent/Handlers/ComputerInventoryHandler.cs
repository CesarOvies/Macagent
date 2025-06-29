using MacAgent.Components;
using MacAgent.Interfaces;
using MacAgent.Services;

namespace MacAgent.Handlers;

public class ComputerInventoryHandler : IInventarioHandler
{
    public string Nome => "Computer";

    public Task Executa()
    {
        ComputerSystem computer_system = HardwareInfo.GetComputer();

        return Task.CompletedTask;
    }
}