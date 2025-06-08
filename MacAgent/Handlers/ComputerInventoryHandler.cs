using System.Text.Json;
using System.Text.RegularExpressions;
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
        string computer_serialized = JsonSerializer.Serialize(computer_system, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText($"{DirectoryReferences.UserProfile}/computer.json", computer_serialized);

        return Task.CompletedTask;
    }

    public class Computer
    {
        public string? Name { get; set; }

        public string? Factory { get; set; }

        public string? SubType { get; set; }
    }
}