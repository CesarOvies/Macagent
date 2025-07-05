using MacAgent.Components;
using MacAgent.Interfaces;
using MacAgent.Services;

namespace MacAgent.Handlers;

public class BatteryInventoryHandler : IInventarioHandler
{
    public string Nome => "Battery";

    public Task Executa()
    {
        List<Battery> battery_info = HardwareInfo.GetBattery();

        return Task.CompletedTask;
    }
}