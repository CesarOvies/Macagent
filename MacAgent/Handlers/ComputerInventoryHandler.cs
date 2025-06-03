using System.Text.Json;
using System.Text.RegularExpressions;
using MacAgent.Interfaces;
using MacAgent.Services;

namespace MacAgent.Handlers;

public class ComputerInventoryHandler : IInventarioHandler
{
    public string Nome => "Computer";

    public Task Executa()
    {
        Computer computer = GetComputerInfos();
        string computer_serialized = JsonSerializer.Serialize(computer, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText($"{DirectoryReferences.UserProfile}/computer.json", computer_serialized);

        return Task.CompletedTask;
    }

    private Computer GetComputerInfos()
    {
        string computer_name = GetComputerName();
        string factory = "Apple";
        string sub_type = GetComputerSubType();

        return new Computer()
        {
            Name = computer_name,
            Factory = factory,
            SubType = sub_type
        };
    }

    private static string GetComputerName()
    {
        string info = ProcessAppService.ReadProcessOut("/usr/sbin/scutil", "--get ComputerName");

        return info;
    }

    private static string GetComputerSubType()
    {
        string infos = ProcessAppService.ReadProcessOut("/usr/sbin/system_profiler", "SPHardwareDataType");
        Match match_info = Regex.Match(infos, @"Model Name: (.+)");

        if (match_info.Success == false)
        {
            return string.Empty;
        }

        string sub_type = match_info.Groups[1].Value.Trim();

        if (sub_type.Contains("MacBook", StringComparison.OrdinalIgnoreCase))
            return "Laptop";
        else if (sub_type.Contains("iMac", StringComparison.OrdinalIgnoreCase) ||
                 sub_type.Contains("Mac mini", StringComparison.OrdinalIgnoreCase) ||
                 sub_type.Contains("Mac Pro", StringComparison.OrdinalIgnoreCase))
            return "Desktop";
        else
            return "Other (Apple Device)";
    }

    public class Computer
    {
        public string? Name { get; set; }

        public string? Factory { get; set; }

        public string? SubType { get; set; }
    }
}