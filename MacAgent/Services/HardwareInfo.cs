using System.Text.RegularExpressions;
using MacAgent.Components;

namespace MacAgent.Services;

public class HardwareInfo
{
    public static ComputerSystem GetComputer()
    {
        ComputerSystem computer_system = new ComputerSystem
        {
            Vendor = "Apple"
        };

        try
        {
            List<string> lines = [.. ProcessInfo.ReadProcessOut("system_profiler", "SPHardwareDataType").Split('\n', StringSplitOptions.TrimEntries)];

            foreach (string line in lines)
            {
                if (line.StartsWith("Model Name: "))
                {
                    string _line = line.Replace("Model Name: ", string.Empty);
                    computer_system.Caption = _line;
                    computer_system.Name = _line;
                    computer_system.SubType = GetComputerSubType(_line);
                }
                else if (line.StartsWith("Model Identifier: "))
                {
                    computer_system.Description = line.Replace("Model Identifier: ", string.Empty);
                }
                else if (line.StartsWith("Serial Number (system): "))
                {
                    computer_system.IdentifyingNumber = line.Replace("Serial Number (system): ", string.Empty);
                }
                else if (line.StartsWith("Hardware UUID"))
                {
                    computer_system.UUID = line.Replace("Hardware UUID: ", string.Empty);
                }
                else if (line.StartsWith("Model Number: "))
                {
                    computer_system.SKUNumber = line.Replace("Model Number: ", string.Empty);
                }
                else if (line.StartsWith("System Firmware Version: "))
                {
                    computer_system.Version = line.Replace("System Firmware Version: ", string.Empty);
                }
            }
        }
        catch (Exception)
        {
        }

        return computer_system;
    }

    private static string GetComputerSubType(string model_name)
    {
        if (model_name.Contains("MacBook", StringComparison.OrdinalIgnoreCase))
            return "Laptop";
        else if (model_name.Contains("iMac", StringComparison.OrdinalIgnoreCase) ||
                 model_name.Contains("Mac mini", StringComparison.OrdinalIgnoreCase) ||
                 model_name.Contains("Mac Pro", StringComparison.OrdinalIgnoreCase))
            return "Desktop";
        else
            return "Other (Apple Device)";
    }

    public static List<CPU> GetCpu()
    {
        string brand_string = ProcessInfo.ReadProcessOut("sysctl", "-n machdep.cpu.brand_string");
        string cpu_name = brand_string.Split('@')[0].Trim();
        string nperf_levels = ProcessInfo.ReadProcessOut("sysctl", "-n hw.nperflevels");
        uint.TryParse(nperf_levels, out uint nperf_levels_out);
        List<CPU> cpu_list = new List<CPU>();

        if (nperf_levels_out > 1)
        {
            for (int i = 0; i < nperf_levels_out; i++)
            {
                CPU cpu = new CPU
                {
                    Name = cpu_name,
                    Caption = i.ToString(),
                    Description = $"perflevel{i}"
                };

                PopulateCpuDetails(cpu, $"hw.perflevel{i}.");
                cpu_list.Add(cpu);
            }
        }
        else
        {
            CPU cpu = new CPU { Name = cpu_name };
            PopulateCpuDetails(cpu, "hw.");
            cpu_list.Add(cpu);
        }

        return cpu_list;
    }

    /// <param name="cpu">O objeto CPU a ser preenchido.</param>
    /// <param name="key_prefix">O prefixo a ser usado para as chaves sysctl ("hw." ou "hw.perflevelX.").</param>
    private static void PopulateCpuDetails(CPU cpu, string key_prefix)
    {
        uint GetSysctlUint(string key)
        {
            string output = ProcessInfo.ReadProcessOut("sysctl", $"-n {key}");
            uint.TryParse(output, out uint value);
            return value;
        }

        cpu.MaxClockSpeed = GetSysctlUint("hw.cpufrequency_max") / 1_000_000;
        cpu.CurrentClockSpeed = GetSysctlUint("hw.cpufrequency") / 1_000_000;
        cpu.L1InstructionCacheSize = GetSysctlUint($"{key_prefix}l1icachesize");
        cpu.L1DataCacheSize = GetSysctlUint($"{key_prefix}l1dcachesize");
        cpu.L2CacheSize = GetSysctlUint($"{key_prefix}l2cachesize");
        cpu.L3CacheSize = GetSysctlUint($"{key_prefix}l3cachesize");
        cpu.NumberOfCores = GetSysctlUint($"{key_prefix}physicalcpu");
        cpu.NumberOfLogicalProcessors = GetSysctlUint($"{key_prefix}logicalcpu");
    }

    public static List<Battery> GetBattery()
    {
        Battery battery = new Battery();
        string processOutput = ProcessInfo.ReadProcessOut("pmset", "-g batt");

        Match chargeMatch = Regex.Match(processOutput, @"(\d+)%");
        if (chargeMatch.Success && ushort.TryParse(chargeMatch.Groups[1].Value, out ushort estimatedChargeRemaining))
        {
            battery.EstimatedChargeRemaining = estimatedChargeRemaining;
        }

        Match timeMatch = Regex.Match(processOutput, @"(\d+):(\d+)(?=\s)");
        if (timeMatch.Success && uint.TryParse(timeMatch.Groups[1].Value, out uint hours) && uint.TryParse(timeMatch.Groups[2].Value, out uint minutes))
        {
            battery.EstimatedRunTime = (hours * 60) + minutes;
        }

        return new List<Battery> { battery };
    }
}