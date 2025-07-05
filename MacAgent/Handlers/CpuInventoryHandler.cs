using MacAgent.Components;
using MacAgent.Interfaces;
using MacAgent.Services;

namespace MacAgent.Handlers;

public class CpuInventoryHandler : IInventarioHandler
{
    public string Nome => "Cpu";

    public Task Executa()
    {
        List<CPU> Cpu_info = HardwareInfo.GetCpu();

        return Task.CompletedTask;
    }
}