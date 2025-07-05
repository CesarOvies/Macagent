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
        List<CPU> cpuList = new List<CPU>();

        string processOutput = ProcessInfo.ReadProcessOut("sysctl", "-n hw.nperflevels");

        if (uint.TryParse(processOutput, out uint levels) && levels > 1)
        {
            for (int i = 0; i < levels; i++)
            {
                string perflevel = "perflevel" + i;

                CPU cpu = new CPU();

                cpu.Caption = i.ToString();
                cpu.Description = perflevel;

                processOutput = ProcessInfo.ReadProcessOut("sysctl", "-n machdep.cpu.brand_string");

                // Intel CPUs include the clock speed as part of the name
                cpu.Name = processOutput.Split('@')[0].Trim();

                processOutput = ProcessInfo.ReadProcessOut("sysctl", "-n hw.cpufrequency_max");

                if (uint.TryParse(processOutput, out uint maxFrequency))
                    cpu.MaxClockSpeed = maxFrequency / 1_000_000;

                processOutput = ProcessInfo.ReadProcessOut("sysctl", "-n hw.cpufrequency");

                if (uint.TryParse(processOutput, out uint frequency))
                    cpu.CurrentClockSpeed = frequency / 1_000_000;

                processOutput = ProcessInfo.ReadProcessOut("sysctl", $"-n hw.{perflevel}.l1icachesize");

                if (uint.TryParse(processOutput, out uint L1InstructionCacheSize))
                    cpu.L1InstructionCacheSize = L1InstructionCacheSize;

                processOutput = ProcessInfo.ReadProcessOut("sysctl", $"-n hw.{perflevel}.l1dcachesize");

                if (uint.TryParse(processOutput, out uint L1DataCacheSize))
                    cpu.L1DataCacheSize = L1DataCacheSize;

                processOutput = ProcessInfo.ReadProcessOut("sysctl", $"-n hw.{perflevel}.l2cachesize");

                if (uint.TryParse(processOutput, out uint L2CacheSize))
                    cpu.L2CacheSize = L2CacheSize;

                processOutput = ProcessInfo.ReadProcessOut("sysctl", $"-n hw.{perflevel}.l3cachesize");

                if (uint.TryParse(processOutput, out uint L3CacheSize))
                    cpu.L3CacheSize = L3CacheSize;

                processOutput = ProcessInfo.ReadProcessOut("sysctl", $"-n hw.{perflevel}.physicalcpu");

                if (uint.TryParse(processOutput, out uint numberOfCores))
                    cpu.NumberOfCores = numberOfCores;

                processOutput = ProcessInfo.ReadProcessOut("sysctl", $"-n hw.{perflevel}.logicalcpu");

                if (uint.TryParse(processOutput, out uint numberOfLogicalProcessors))
                    cpu.NumberOfLogicalProcessors = numberOfLogicalProcessors;

                cpuList.Add(cpu);
            }
        }
        else
        {
            CPU cpu = new CPU();

            processOutput = ProcessInfo.ReadProcessOut("sysctl", "-n machdep.cpu.brand_string");

            // Intel CPUs include the clock speed as part of the name
            cpu.Name = processOutput.Split('@')[0].Trim();

            processOutput = ProcessInfo.ReadProcessOut("sysctl", "-n hw.cpufrequency_max");

            if (uint.TryParse(processOutput, out uint maxFrequency))
                cpu.MaxClockSpeed = maxFrequency / 1_000_000;

            processOutput = ProcessInfo.ReadProcessOut("sysctl", "-n hw.cpufrequency");

            if (uint.TryParse(processOutput, out uint frequency))
                cpu.CurrentClockSpeed = frequency / 1_000_000;

            processOutput = ProcessInfo.ReadProcessOut("sysctl", "-n hw.l1icachesize");

            if (uint.TryParse(processOutput, out uint L1InstructionCacheSize))
                cpu.L1InstructionCacheSize = L1InstructionCacheSize;

            processOutput = ProcessInfo.ReadProcessOut("sysctl", "-n hw.l1dcachesize");

            if (uint.TryParse(processOutput, out uint L1DataCacheSize))
                cpu.L1DataCacheSize = L1DataCacheSize;

            processOutput = ProcessInfo.ReadProcessOut("sysctl", "-n hw.l2cachesize");

            if (uint.TryParse(processOutput, out uint L2CacheSize))
                cpu.L2CacheSize = L2CacheSize;

            processOutput = ProcessInfo.ReadProcessOut("sysctl", "-n hw.l3cachesize");

            if (uint.TryParse(processOutput, out uint L3CacheSize))
                cpu.L3CacheSize = L3CacheSize;

            processOutput = ProcessInfo.ReadProcessOut("sysctl", "-n hw.physicalcpu");

            if (uint.TryParse(processOutput, out uint numberOfCores))
                cpu.NumberOfCores = numberOfCores;

            processOutput = ProcessInfo.ReadProcessOut("sysctl", "-n hw.logicalcpu");

            if (uint.TryParse(processOutput, out uint numberOfLogicalProcessors))
                cpu.NumberOfLogicalProcessors = numberOfLogicalProcessors;

            cpuList.Add(cpu);
        }

        return cpuList;
    }

    public static List<Battery> GetBattery()
    {
        List<Battery> battery_list = new List<Battery>();
        Battery battery = new Battery();

        string processOutput = ProcessInfo.ReadProcessOut("pmset", "-g batt");

        Regex estimatedChargeRemainingRegex = new Regex("(\\d+)%");
        Match match = estimatedChargeRemainingRegex.Match(processOutput);

        if (match.Success && match.Groups.Count > 1)
        {
            if (ushort.TryParse(match.Groups[1].Value, out ushort estimatedChargeRemaining))
            {
                battery.EstimatedChargeRemaining = estimatedChargeRemaining;
            }
        }

        Regex estimatedRunTimeRegex = new Regex("(\\d+:\\d+)");
        match = estimatedRunTimeRegex.Match(processOutput);

        if (match.Success && match.Groups.Count > 1)
        {
            string[] estimatedRunTime = match.Groups[1].Value.Split(':');

            if (estimatedRunTime.Length == 2 && uint.TryParse(estimatedRunTime[0], out uint hours) && uint.TryParse(estimatedRunTime[1], out uint minutes))
            {
                battery.EstimatedRunTime = hours * 60 + minutes;
            }
        }

        battery_list.Add(battery);

        return battery_list;
    }
}